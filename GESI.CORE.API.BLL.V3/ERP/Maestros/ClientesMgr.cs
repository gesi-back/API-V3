using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.CORE.V3.BLL;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using GESI.ERP.Ventas.V3.BLL;
using GESI.ERP.Ventas.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IClientesServicio
    {
        cResponseClientes GetList(int pageNumber, int pageSize,
                                  string sucursales, string preciosespeciales, string fechamodificaciones,
                                  string contactos, string unidadesdecliente,
                                  string? tiposDeCliente, string? canalesDeVenta, string? formasDePago, string? condicionesIVA,
                                  string? ordenarPor, string? direccion, string definicionesSucursal);

        cResponseCliente GetItem(int clienteID, string sucursales, string preciosespeciales, string contactos, string unidadesdecliente);

        cResponseClientes GetSearchResults(string? expresion, string buscarDatosContacto, int pageNumber, int pageSize,
                                           string sucursales, string preciosespeciales, string contactos, string unidadesdecliente,
                                           string? tiposDeCliente, string? canalesDeVenta, string? formasDePago, string? condicionesIVA,
                                           string? ordenarPor, string? direccion);

        cResponseClientes Save(List<cCliente>? oCliente, bool Alta);

        cResponseSaveClientes Save(List<cCliente>? oCliente);

        cResponseSaldosClientes GetSaldos(int pageNumber, int pageSize, string? strClientes, string? strTiposDeClienteID, string? fechaHasta,
                                          bool blnIncluirRemitosPendientes, bool blnIncluirComprobanteMasAntiguo, bool blnIncluirChequesEnCartera);
    }

    /// <summary>
    /// Migrado de la V2 (ClientesMgr estático con _MIAPISessionMgr estático).
    /// Mantiene los mismos métodos y pasos que la V2; lo que cambia es que la sesión, los BLL del Core
    /// y el logger llegan por inyección de dependencias, y que los errores no esperados no se atrapan acá:
    /// los traduce el controller (Responder) a status HTTP.
    /// </summary>
    internal sealed class ClientesServicio(
        IClientesBLL clientesBLL,
        IUtilidadesBLL utilidadesBLL,
        ICuentaCorrienteBLL cuentaCorrienteBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IClientesServicio
    {
        /// <summary>
        /// Devuelve la lista de clientes
        /// </summary>
        public cResponseClientes GetList(int pageNumber, int pageSize,
                                         string sucursales, string preciosespeciales, string fechamodificaciones,
                                         string contactos, string unidadesdecliente,
                                         string? tiposDeCliente, string? canalesDeVenta, string? formasDePago, string? condicionesIVA,
                                         string? ordenarPor, string? direccion, string definicionesSucursal)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseClientes oRespuesta = new cResponseClientes();

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            // Igual que la V2: si la habilitación de la API tiene tipos de cliente, se usan esos
            if (Sesion.TiposDeClientes?.Length > 0)
                tiposDeCliente = Sesion.TiposDeClientes;

            try
            {
                List<string> lstCodigosClientes;

                if (fechamodificaciones?.Length > 0)
                {
                    DateTime oFecha = DateTime.Parse(fechamodificaciones);   // fecha inválida → 400

                    // Obtengo los clientes que fueron modificados desde la fecha
                    var lstCodigosClientesModificados = utilidadesBLL.GetRegistrosModificadosDesdeFecha(Sesion.EmpresaID,
                                                                                                         ((int)LogCambios.tClientesDatosGenerales).ToString(),
                                                                                                         oFecha);
                    if (lstCodigosClientesModificados == null || lstCodigosClientesModificados.Count == 0)
                        throw new cDatosNoEncontradosException("No se encontraron clientes");

                    lstCodigosClientes = lstCodigosClientesModificados.Select(x => x.Item2).Distinct().ToList();

                    // NUEVO respecto de la V2: si la habilitación tiene tipos de cliente, solo se dejan los clientes de esos tipos
                    // (en la V2 este camino devolvía clientes de cualquier tipo)
                    if (Sesion.TiposDeClientes?.Length > 0)
                    {
                        List<string> lstClientesHabilitados = clientesBLL.GetSearchResults(strTipoDeCliente: Sesion.TiposDeClientes) ?? new List<string>();
                        lstCodigosClientes = lstCodigosClientes.Where(x => lstClientesHabilitados.Contains(x)).ToList();
                    }
                }
                else
                {
                    lstCodigosClientes = clientesBLL.GetSearchResults(strTipoDeCliente: tiposDeCliente!,
                                                                      strCanalesDeVenta: canalesDeVenta!,
                                                                      strFormasDePago: formasDePago!,
                                                                      strCondicionesIVA: condicionesIVA!,
                                                                      strOrdenarPor: ordenarPor!,
                                                                      strDireccion: direccion!) ?? new List<string>();
                }

                if (lstCodigosClientes.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron clientes");

                // Toma los códigos de la página pedida (si la página no existe lanza cIntervalException)
                string strCodigosPagina = ApiHelper.DevolverCodigosSeparadosPorComa(lstCodigosClientes, pageNumber, pageSize);

                List<cCliente>? lstClientes = clientesBLL.GetList(strCodigosPagina,
                                                                  sucursales,
                                                                  preciosespeciales,
                                                                  contactos,
                                                                  unidadesdecliente,
                                                                  strDevolverDefinicionesPorSucursal: definicionesSucursal);

                if (lstClientes == null || lstClientes.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron clientes");

                oRespuesta.clientes = new List<cAPICliente>();
                foreach (cCliente oCliente in lstClientes)
                {
                    oRespuesta.clientes.Add(new cAPICliente(oCliente));
                }

                int intCantidadElementos = lstCodigosClientes.Count;

                oRespuesta.success = true;
                oRespuesta.paginacion = new cPaginacion(intCantidadElementos, (int)Math.Ceiling((double)intCantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success Clientes GetList", Endpoint.Maestros.Clientes("GetList"), stopwatch, intCantidadElementos, pageNumber, pageSize);

                return oRespuesta;
            }
            catch (cIntervalException oIntervalo)
            {
                oRespuesta.paginacion = oIntervalo._paginacion;
                oRespuesta.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);
                oRespuesta.success = true;
                return oRespuesta;
            }
            catch (cDatosNoEncontradosException mensaje)
            {
                LoguearInfo("Not Found Clientes GetList", Endpoint.Maestros.Clientes("GetList"), stopwatch, 0, pageNumber, pageSize);

                oRespuesta.error = new cError((int)cCodigosError.cDatosNoEncontrados, mensaje.Message, ApiErrors.errDatosNoEncontrados);
                oRespuesta.success = true;
                return oRespuesta;
            }
        }

        /// <summary>
        /// Devuelve un cliente pasado por parametro
        /// </summary>
        public cResponseCliente GetItem(int clienteID, string sucursales, string preciosespeciales, string contactos, string unidadesdecliente)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseCliente oRespuesta = new cResponseCliente();

            if (clienteID == 0)
                throw new cCodigoNoDetalladoException("No se detalló el clienteID");   // → 400

            List<cCliente>? oCliente = clientesBLL.GetList(clienteID.ToString(),
                                                           sucursales,
                                                           preciosespeciales,
                                                           contactos,
                                                           unidadesdecliente);

            if (oCliente == null || oCliente.Count == 0)
                throw new cDatosNoEncontradosException("No se encontró el cliente " + clienteID);   // → 404

            // NUEVO respecto de la V2: un cliente de un tipo no habilitado se informa como no encontrado
            if (!TipoDeClienteHabilitado(oCliente[0].TipoDeClienteID))
                throw new cDatosNoEncontradosException("No se encontró el cliente " + clienteID);   // → 404

            oRespuesta.cliente = new cAPICliente(oCliente[0]);
            oRespuesta.success = true;
            oRespuesta.paginacion = new cPaginacion(1, 1, 1, 1, 1);

            LoguearInfo("Success Clientes GetItem clienteID: " + clienteID, Endpoint.Maestros.Clientes("GetItem"), stopwatch, 1, 1, 1);

            return oRespuesta;
        }

        /// <summary>
        /// Devuelve un resultado de busqueda de clientes
        /// </summary>
        public cResponseClientes GetSearchResults(string? expresion, string buscarDatosContacto, int pageNumber, int pageSize,
                                                  string sucursales, string preciosespeciales, string contactos, string unidadesdecliente,
                                                  string? tiposDeCliente, string? canalesDeVenta, string? formasDePago, string? condicionesIVA,
                                                  string? ordenarPor, string? direccion)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseClientes oResponseClientes = new cResponseClientes();

            if (string.IsNullOrEmpty(expresion))
                throw new cCodigoNoDetalladoException("No se detalló la expresion de busqueda");   // → 400

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            // Igual que la V2: si la habilitación de la API tiene tipos de cliente, se usan esos
            if (Sesion.TiposDeClientes?.Length > 0)
                tiposDeCliente = Sesion.TiposDeClientes;

            try
            {
                List<string>? lstCodigosClientesERP = clientesBLL.GetSearchResults(expresion,
                                                                                    buscarDatosContacto == "S",
                                                                                    strTipoDeCliente: tiposDeCliente!,
                                                                                    strCanalesDeVenta: canalesDeVenta!,
                                                                                    strFormasDePago: formasDePago!,
                                                                                    strCondicionesIVA: condicionesIVA!,
                                                                                    strOrdenarPor: ordenarPor!,
                                                                                    strDireccion: direccion!);

                if (lstCodigosClientesERP == null || lstCodigosClientesERP.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron resultados");

                string strCodigosPagina = ApiHelper.DevolverCodigosSeparadosPorComa(lstCodigosClientesERP, pageNumber, pageSize);

                // NUEVO respecto de la V2: se pasan sucursales/preciosespeciales/contactos/unidades
                // (la V2 los recibía pero no se los pasaba al Core)
                List<cCliente>? lstClientes = clientesBLL.GetList(strCodigosPagina,
                                                                  sucursales,
                                                                  preciosespeciales,
                                                                  contactos,
                                                                  unidadesdecliente);

                if (lstClientes == null || lstClientes.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron resultados");

                oResponseClientes.clientes = new List<cAPICliente>();
                foreach (cCliente oClienteERP in lstClientes)
                {
                    oResponseClientes.clientes.Add(new cAPICliente(oClienteERP));
                }

                oResponseClientes.success = true;
                oResponseClientes.paginacion = new cPaginacion(lstCodigosClientesERP.Count, (int)Math.Ceiling((double)lstCodigosClientesERP.Count / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success Clientes GetSearchResults " + expresion, Endpoint.Maestros.Clientes("GetSearchResults"), stopwatch, lstCodigosClientesERP.Count, pageNumber, pageSize);

                return oResponseClientes;
            }
            catch (cIntervalException oIntervalo)
            {
                oResponseClientes.paginacion = oIntervalo._paginacion;
                oResponseClientes.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);
                oResponseClientes.success = true;
                return oResponseClientes;
            }
            catch (cDatosNoEncontradosException mensaje)
            {
                LoguearInfo("Not Found Clientes GetSearchResults " + expresion, Endpoint.Maestros.Clientes("GetSearchResults"), stopwatch, 0, 0, 0);

                oResponseClientes.error = new cError((int)cCodigosError.cDatosNoEncontrados, mensaje.Message, ApiErrors.errDatosNoEncontrados);
                oResponseClientes.success = true;
                return oResponseClientes;
            }
        }

        /// <summary>
        /// Graba clientes en la base (Create / Update)
        /// </summary>
        public cResponseClientes Save(List<cCliente>? oCliente, bool Alta)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseClientes oRespuesta = new cResponseClientes();
            string accion = Alta ? "Create" : "Update";

            if (oCliente == null || oCliente.Count == 0)
                throw new cCodigoNoDetalladoException("No se informaron clientes a grabar");   // → 400

            string respuesta = "";
            List<cCliente> lstClientesAgregados = new List<cCliente>();

            foreach (cCliente objCliente in oCliente)
            {
                try
                {
                    if (string.IsNullOrEmpty(objCliente.NumeroDeDocumento))
                    {
                        respuesta = respuesta + "El cliente " + objCliente.RazonSocial + " no posee numero de documento " + Environment.NewLine;
                        continue;
                    }

                    if (!Alta && objCliente.ClienteID == 0)
                    {
                        respuesta = respuesta + "No se informo el ClienteID del usuario con dni: " + objCliente.NumeroDeDocumento + Environment.NewLine;
                        continue;
                    }

                    // NUEVO respecto de la V2: la empresa sale siempre de la sesión
                    objCliente.EmpresaID = (short)Sesion.EmpresaID;

                    // NUEVO respecto de la V2: se respetan los tipos de cliente de la habilitación
                    string strRechazo = ValidarTipoDeCliente(objCliente, Alta, parcial: false);
                    if (strRechazo.Length > 0)
                    {
                        respuesta = respuesta + strRechazo + Environment.NewLine;
                        continue;
                    }

                    cCliente? oClienteERP = clientesBLL.Save(objCliente, Alta, out string result);

                    if (oClienteERP != null)
                        lstClientesAgregados.Add(oClienteERP);

                    // La V2 agregaba el salto de línea aunque result viniera vacío (y así siempre devolvía error)
                    if (!string.IsNullOrEmpty(result))
                        respuesta = respuesta + result + Environment.NewLine;
                }
                catch (Exception ex)
                {
                    Logger.LoguearSucesosAPI("Error al grabar / actualizar cliente " + objCliente.NumeroDeDocumento + " D:" + ex.Message,
                                             tTiposDeError.tError,
                                             Sesion.Usuario,
                                             Endpoint.Maestros.Clientes(accion),
                                             Sesion.EmpresaID,
                                             Sesion.SucursalID,
                                             IPRemoteDesktop: Sesion.RemoteIPAddress, IPPublica: Sesion.Forwarded, Agent: Sesion.UserAgent);

                    respuesta = respuesta + "Error al grabar cliente " + objCliente.RazonSocial + ". Descripcion: " + ex.Message + Environment.NewLine;
                }
            }

            if (lstClientesAgregados.Count > 0)
            {
                oRespuesta.clientes = new List<cAPICliente>();
                foreach (cCliente ClienteAgregado in lstClientesAgregados)
                {
                    oRespuesta.clientes.Add(new cAPICliente(ClienteAgregado));
                }
            }

            if (respuesta.Length > 0)
                oRespuesta.error = new cError((int)cCodigosError.cSintaxisIncorrecta, respuesta, ApiErrors.errJsonInvalido);

            oRespuesta.success = true;

            LoguearInfo("Success Clientes " + accion, Endpoint.Maestros.Clientes(accion), stopwatch, oCliente.Count, 1, oCliente.Count);

            return oRespuesta;
        }

        /// <summary>
        /// PATCH: actualiza solamente los datos informados
        /// </summary>
        public cResponseSaveClientes Save(List<cCliente>? oCliente)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseSaveClientes oRespuesta = new cResponseSaveClientes();

            if (oCliente == null || oCliente.Count == 0)
                throw new cCodigoNoDetalladoException("No se informaron clientes a actualizar");   // → 400

            string respuesta = "";
            List<cResponseSaveCliente> lstClientesAgregados = new List<cResponseSaveCliente>();

            foreach (cCliente objCliente in oCliente)
            {
                try
                {
                    // NUEVO respecto de la V2: sin ClienteID no se puede actualizar
                    if (objCliente.ClienteID == 0)
                    {
                        respuesta = respuesta + "No se informo el ClienteID del cliente " + objCliente.RazonSocial + Environment.NewLine;
                        continue;
                    }

                    // NUEVO respecto de la V2: la empresa sale siempre de la sesión
                    objCliente.EmpresaID = (short)Sesion.EmpresaID;

                    // NUEVO respecto de la V2: se respetan los tipos de cliente de la habilitación
                    string strRechazo = ValidarTipoDeCliente(objCliente, Alta: false, parcial: true);
                    if (strRechazo.Length > 0)
                    {
                        respuesta = respuesta + strRechazo + Environment.NewLine;
                        continue;
                    }

                    cCliente? oClienteERP = clientesBLL.Save(objCliente, out string result);

                    if (oClienteERP != null)
                        lstClientesAgregados.Add(new cResponseSaveCliente(oClienteERP));

                    if (result?.Length > 0)
                        respuesta = respuesta + " | " + result + Environment.NewLine;
                }
                catch (Exception ex)
                {
                    Logger.LoguearSucesosAPI("Error al grabar / actualizar cliente " + objCliente.NumeroDeDocumento + " D:" + ex.Message,
                                             tTiposDeError.tError,
                                             Sesion.Usuario,
                                             Endpoint.Maestros.Clientes("update-parcial"),
                                             Sesion.EmpresaID,
                                             Sesion.SucursalID,
                                             IPRemoteDesktop: Sesion.RemoteIPAddress, IPPublica: Sesion.Forwarded, Agent: Sesion.UserAgent);

                    respuesta = respuesta + "Error al grabar cliente " + objCliente.RazonSocial + ". Descripcion: " + ex.Message + Environment.NewLine;
                }
            }

            if (lstClientesAgregados.Count > 0)
                oRespuesta.clientes = lstClientesAgregados;

            if (respuesta.Length > 0)
                oRespuesta.error = new cError((int)cCodigosError.cSintaxisIncorrecta, respuesta, ApiErrors.errJsonInvalido);

            oRespuesta.success = true;

            LoguearInfo("Success Clientes update-parcial", Endpoint.Maestros.Clientes("update-parcial"), stopwatch, oCliente.Count, 1, oCliente.Count);

            return oRespuesta;
        }

        /// <summary>
        /// Obtiene el listado de saldos de clientes
        /// </summary>
        public cResponseSaldosClientes GetSaldos(int pageNumber, int pageSize, string? strClientes, string? strTiposDeClienteID, string? fechaHasta,
                                                 bool blnIncluirRemitosPendientes, bool blnIncluirComprobanteMasAntiguo, bool blnIncluirChequesEnCartera)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseSaldosClientes oResponseSaldosClientes = new cResponseSaldosClientes();

            // NUEVO respecto de la V2: se respeta el tamaño máximo de página de la habilitación
            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            List<string>? lstClientes = null;
            if (strClientes?.Length > 0)
                lstClientes = strClientes.Split(',').Select(x => x.Trim()).ToList();

            DateTime? dtfechaHasta = null;
            if (fechaHasta?.Length > 0)
                dtfechaHasta = DateTime.Parse(fechaHasta);   // fecha inválida → 400

            // Igual que la V2: si la habilitación de la API tiene tipos de cliente, se usan esos
            if (Sesion.TiposDeClientes?.Length > 0)
                strTiposDeClienteID = Sesion.TiposDeClientes;

            try
            {
                List<dSaldosCliente>? lstSaldosClientes = cuentaCorrienteBLL.GetTotalesSaldosClientes(lstClientes!,
                                                                                                      strTiposDeClienteID!,
                                                                                                      datFechaHasta: dtfechaHasta,
                                                                                                      blnIncluirRemitosPendientes: blnIncluirRemitosPendientes,
                                                                                                      blnIncluirChequesEnCartera: blnIncluirChequesEnCartera,
                                                                                                      blnInformarComprobanteAdeudadoMasAntiguo: blnIncluirComprobanteMasAntiguo);

                if (lstSaldosClientes == null || lstSaldosClientes.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron saldos de clientes");

                int intCantidadElementos = lstSaldosClientes.Count;

                oResponseSaldosClientes.success = true;
                oResponseSaldosClientes.paginacion = new cPaginacion(intCantidadElementos, (int)Math.Ceiling((double)intCantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                oResponseSaldosClientes.saldos = lstSaldosClientes.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                // NUEVO respecto de la V2: si la página no existe se avisa (antes volvía la lista vacía sin error)
                if (oResponseSaldosClientes.saldos.Count == 0)
                    throw new cIntervalException(oResponseSaldosClientes.paginacion);

                LoguearInfo("Success Clientes GetSaldos", Endpoint.Maestros.Clientes("GetSaldos"), stopwatch, intCantidadElementos, pageNumber, pageSize);

                return oResponseSaldosClientes;
            }
            catch (cIntervalException oIntervalo)
            {
                oResponseSaldosClientes.saldos = null;
                oResponseSaldosClientes.paginacion = oIntervalo._paginacion;
                oResponseSaldosClientes.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);
                oResponseSaldosClientes.success = true;
                return oResponseSaldosClientes;
            }
            catch (cDatosNoEncontradosException mensaje)
            {
                LoguearInfo("Not Found Clientes GetSaldos", Endpoint.Maestros.Clientes("GetSaldos"), stopwatch, 0, 0, 0);

                oResponseSaldosClientes.error = new cError((int)cCodigosError.cDatosNoEncontrados, mensaje.Message, ApiErrors.errDatosNoEncontrados);
                oResponseSaldosClientes.success = true;
                return oResponseSaldosClientes;
            }
        }

        #region Restricción por tipos de cliente (NUEVO respecto de la V2)

        /// <summary>
        /// Indica si el tipo de cliente está habilitado para esta integración.
        /// Si la habilitación de la API no tiene tipos de cliente cargados, todos están habilitados.
        /// </summary>
        private bool TipoDeClienteHabilitado(short? tipoDeClienteID)
        {
            if (string.IsNullOrEmpty(Sesion.TiposDeClientes))
                return true;

            if (tipoDeClienteID == null)
                return false;

            List<string> lstTiposHabilitados = Sesion.TiposDeClientes.Split(',').Select(x => x.Trim()).ToList();

            return lstTiposHabilitados.Contains(tipoDeClienteID.Value.ToString());
        }

        /// <summary>
        /// Valida que el cliente a grabar sea de un tipo habilitado y, si es una modificación,
        /// que el cliente que ya existe en la base también lo sea.
        /// Devuelve el motivo del rechazo, o "" si se puede grabar.
        /// </summary>
        private string ValidarTipoDeCliente(cCliente objCliente, bool Alta, bool parcial)
        {
            if (string.IsNullOrEmpty(Sesion.TiposDeClientes))
                return "";

            // En el PATCH el tipo puede no venir (no se modifica); si viene, tiene que estar habilitado
            bool blnValidarTipoNuevo = !parcial || objCliente.TipoDeClienteID != null;

            if (blnValidarTipoNuevo && !TipoDeClienteHabilitado(objCliente.TipoDeClienteID))
                return "El cliente " + objCliente.RazonSocial + " tiene un tipo de cliente no habilitado (" + objCliente.TipoDeClienteID + ")";

            if (!Alta)
            {
                cCliente? oClienteExistente = clientesBLL.GetItem((uint)objCliente.ClienteID);

                if (oClienteExistente == null || !TipoDeClienteHabilitado(oClienteExistente.TipoDeClienteID))
                    return "No se encontró el cliente " + objCliente.ClienteID;
            }

            return "";
        }

        #endregion
    }
}

using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.ERP.Ventas.V3.BLL;
using GESI.ERP.Ventas.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Ventas
{
    public interface IRepartosServicio
    {
        cResponseFojasDeRepartos GetList(int repartidorID = 0,
                                         int zonaID = 0,
                                         string? fechaDesde = null,
                                         string? estado = null,
                                         int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                         int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize);

        cResponseFojaDeReparto ObtenerFojaReparto(int empresaID, int fojaID);

        cResponseGetProductoEncontradoFoja GetProductoFoja(int empresaID, int fojaID, string? productoID);

        cResponseCompararProductos CompararProductos(int empresaID, int fojaID, Dictionary<string, decimal>? lstProductos);

        cResponseCompararProductos CerrarCargaFoja(int empresaID, int fojaID, cCierreFoja? oCierreFoja);

        cResponseMovimientoDeClientes GetComprobantesNoAsignados(string fechadesde = "",
                                                                 string fechahasta = "",
                                                                 int vendedorID = 0,
                                                                 int repartidorID = 0,
                                                                 int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                 int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize);

        cResponseFojasDeRepartosSave Create(List<cFojaDeReparto>? lstFoja);
    }

    /// <summary>
    /// Migrado de la V2 (Repartos.BLL.RepartosMgr estático con _MiAPISessionMgr estático).
    /// Fojas de reparto: consulta, control de carga de productos, cierre y alta.
    /// </summary>
    internal sealed class RepartosServicio(
        IFojaDeRepartoBLL fojaDeRepartoBLL,
        IComprobanteDeVentaBLL comprobanteDeVentaBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IRepartosServicio
    {
        /// <summary>
        /// Devuelve la lista de fojas de reparto
        /// </summary>
        public cResponseFojasDeRepartos GetList(int repartidorID = 0,
                                                int zonaID = 0,
                                                string? fechaDesde = null,
                                                string? estado = null,
                                                int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseFojasDeRepartos oRespuesta = new cResponseFojasDeRepartos();

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            // Igual que la V2: el estado se recibe como A (abierta) / C (cerrada) y el Core lo espera como 1 / 2
            if (estado != null && estado.Length > 0)
            {
                if (estado.Equals("A"))
                    estado = "1";
                else if (estado.Equals("C"))
                    estado = "2";
            }

            DateTime? fecha = null;
            if (fechaDesde != null && fechaDesde.Length > 0)
            {
                DateTime fechaAux;
                // NUEVO respecto de la V2: fecha inválida devuelve 400 (la V2 devolvía 500)
                if (!DateTime.TryParse(fechaDesde, out fechaAux))
                    throw new cFormatoInvalidoException("Formato invalido de fecha");   // → 400
                fecha = fechaAux;
            }

            try
            {
                List<cFojaDeReparto>? lstFojas = fojaDeRepartoBLL.GetList(repartidorID, estado!, fecha, zonaID);

                if (lstFojas == null || lstFojas.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron fojas de reparto");

                int cantidadElementos = lstFojas.Count;

                lstFojas = lstFojas.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstFojas.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, 0));

                List<cFojaDeRepartoAPI> lstFojasAPI = new List<cFojaDeRepartoAPI>();
                foreach (cFojaDeReparto oFoja in lstFojas)
                {
                    lstFojasAPI.Add(new cFojaDeRepartoAPI(oFoja, true));
                }

                oRespuesta.FojasReparto = lstFojasAPI;
                oRespuesta.success = true;
                oRespuesta.Paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success Repartos GetList", Endpoint.Repartos.GetList, stopwatch, cantidadElementos, pageNumber, pageSize);

                return oRespuesta;
            }
            catch (cDatosNoEncontradosException ex)
            {
                // Igual que la V2: 200 con success = false y el error adentro
                oRespuesta.error = new cError((int)cCodigosError.cDatosNoEncontrados, ex.Message, ApiErrors.errDatosNoEncontrados);
                return oRespuesta;
            }
            catch (cIntervalException oIntervalo)
            {
                // NUEVO respecto de la V2: "Paginación Excedida" como el resto de la API (la V2 terminaba en el catch general)
                oRespuesta.Paginacion = oIntervalo._paginacion;
                oRespuesta.success = true;
                oRespuesta.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);
                return oRespuesta;
            }
        }

        /// <summary>
        /// Devuelve una foja de reparto con sus comprobantes asignados
        /// </summary>
        public cResponseFojaDeReparto ObtenerFojaReparto(int empresaID, int fojaID)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseFojaDeReparto oRespuesta = new cResponseFojaDeReparto();

            ValidarEmpresa(empresaID);

            // NUEVO respecto de la V2: sin fojaID devuelve 400 y una foja inexistente 404
            if (fojaID <= 0)
                throw new cCodigoNoDetalladoException("Se requiere el atributo fojaID en la solicitud");   // → 400

            cFojaDeReparto? oFojaReparto = fojaDeRepartoBLL.GetItem(fojaID);

            if (oFojaReparto == null || oFojaReparto.FojaID <= 0)
                throw new cDatosNoEncontradosException("No se encontró la foja de reparto");   // → 404

            if (oFojaReparto.ListaDeComprobantesAsignados?.Count > 0)
            {
                List<dClaveComprobanteDeVenta> lstClaves = new List<dClaveComprobanteDeVenta>();

                foreach (cComprobanteVenta oComprob in oFojaReparto.ListaDeComprobantesAsignados)
                {
                    lstClaves.Add(new dClaveComprobanteDeVenta(Sesion.EmpresaID, oComprob.ComprobanteID, oComprob.Serie, oComprob.PuntoDeVentaID, oComprob.Numero));
                }

                // NUEVO respecto de la V2: se traen todos los comprobantes de la foja.
                // La V2 los pedía con la paginación por defecto y devolvía solo los primeros 10.
                List<cComprobanteVenta>? lstComprobantesERP = comprobanteDeVentaBLL.GetItemsByIDs(lstClaves, false, false, false, false, false);

                // Igual que la V2: pasa por cMovimientoDeCliente y vuelve a cComprobanteVenta, así salen los mismos campos que antes
                List<cMovimientoDeCliente> lstMovimientos = new List<cMovimientoDeCliente>();
                if (lstComprobantesERP != null)
                {
                    foreach (cComprobanteVenta oComprobanteERP in lstComprobantesERP)
                    {
                        lstMovimientos.Add(new cMovimientoDeCliente(oComprobanteERP));
                    }
                }

                oFojaReparto.ListaDeComprobantesAsignados = MovimientosDeClientesServicio.ConvertirMovimientoAComprobanteDeVenta(lstMovimientos);
            }

            oRespuesta.fojaReparto = oFojaReparto;
            oRespuesta.success = true;
            oRespuesta.paginacion = new cPaginacion(1, 1, 1, 1, 1);

            LoguearInfo("Success GetFoja", Endpoint.Repartos.GetFoja, stopwatch, 1, 1, 1);

            return oRespuesta;
        }

        /// <summary>
        /// Determina si un producto está en la foja
        /// </summary>
        public cResponseGetProductoEncontradoFoja GetProductoFoja(int empresaID, int fojaID, string? productoID)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseGetProductoEncontradoFoja oResponseProducto = new cResponseGetProductoEncontradoFoja();

            // Igual que la V2: los tres parámetros son obligatorios → 400
            if (empresaID <= 0)
                throw new cCodigoNoDetalladoException("Se requiere el atributo empresaID en la solicitud");
            if (fojaID <= 0)
                throw new cCodigoNoDetalladoException("Se requiere el atributo fojaID en la solicitud");
            if (productoID == null || productoID.Length == 0)
                throw new cCodigoNoDetalladoException("Se requiere el atributo productoID en la solicitud");

            ValidarEmpresa(empresaID);

            List<dDetalleProductoEnFoja>? oDetalle = fojaDeRepartoBLL.GetProductos(fojaID, productoID);

            oResponseProducto.success = true;

            if (oDetalle?.Count > 0)
            {
                oResponseProducto.productoEncontradoEnFoja = true;
                oResponseProducto.codigo = oDetalle[0].ProductoID;
            }
            else
            {
                oResponseProducto.productoEncontradoEnFoja = false;
            }

            LoguearInfo("Success GetProductoFoja", Endpoint.Repartos.GetProducto, stopwatch, 1, 1, 1);

            return oResponseProducto;
        }

        /// <summary>
        /// Compara los productos y cantidades cargados contra la foja y devuelve las diferencias
        /// </summary>
        public cResponseCompararProductos CompararProductos(int empresaID, int fojaID, Dictionary<string, decimal>? lstProductos)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseCompararProductos oResponse = new cResponseCompararProductos();

            // Igual que la V2: los tres parámetros son obligatorios → 400
            if (empresaID <= 0)
                throw new cCodigoNoDetalladoException("Se requiere el atributo empresaID en la solicitud");
            if (fojaID <= 0)
                throw new cCodigoNoDetalladoException("Se requiere el atributo fojaID en la solicitud");
            if (lstProductos == null)
                throw new cCodigoNoDetalladoException("Se requieren productos a evaluar");

            ValidarEmpresa(empresaID);

            Dictionary<string, decimal>? dcResultado = fojaDeRepartoBLL.CompararCantidadesProductos(fojaID, lstProductos);

            oResponse.success = true;
            oResponse.resultado = new List<cResultadoCompararProducto>();

            if (dcResultado?.Count > 0)
            {
                foreach (KeyValuePair<string, decimal> elemento in dcResultado)
                {
                    oResponse.resultado.Add(new cResultadoCompararProducto(elemento.Key, elemento.Value));
                }
            }

            LoguearInfo("Success CompararProductos Diferencias:" + oResponse.resultado.Count, Endpoint.Repartos.CompararProductos, stopwatch, lstProductos.Count, 1, lstProductos.Count);

            return oResponse;
        }

        /// <summary>
        /// Cierra la carga de la foja: graba el comentario y los productos controlados y cambia su estado
        /// </summary>
        public cResponseCompararProductos CerrarCargaFoja(int empresaID, int fojaID, cCierreFoja? oCierreFoja)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseCompararProductos oResponse = new cResponseCompararProductos();

            // Igual que la V2: los tres parámetros son obligatorios → 400
            if (empresaID <= 0)
                throw new cCodigoNoDetalladoException("Se requiere el atributo empresaID en la solicitud");
            if (fojaID <= 0)
                throw new cCodigoNoDetalladoException("Se requiere el atributo fojaID en la solicitud");
            if (oCierreFoja == null)
                throw new cCodigoNoDetalladoException("Se requieren productos a evaluar");

            ValidarEmpresa(empresaID);

            bool oGrabar = fojaDeRepartoBLL.CerrarCargaFoja(fojaID, oCierreFoja.productos!, oCierreFoja.comentario!);

            // Igual que la V2: si el Core no la cerró responde 200 con success = false y sin error
            if (oGrabar)
            {
                oResponse.success = true;
                oResponse.resultado = new List<cResultadoCompararProducto>();
            }

            LoguearInfo("CerrarCargaFoja FojaID:" + fojaID + " Resultado:" + oGrabar, Endpoint.Repartos.CerrarFoja, stopwatch, 1, 1, 1);

            return oResponse;
        }

        /// <summary>
        /// Devuelve los comprobantes que todavía no están asignados a ninguna foja de reparto.
        /// Igual que la V2: devuelve todos (no pagina); la paginación solo informa los totales.
        /// </summary>
        public cResponseMovimientoDeClientes GetComprobantesNoAsignados(string fechadesde = "",
                                                                        string fechahasta = "",
                                                                        int vendedorID = 0,
                                                                        int repartidorID = 0,
                                                                        int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                        int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseMovimientoDeClientes oRespuesta = new cResponseMovimientoDeClientes();

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            // Fecha inválida → FormatException → 400
            DateTime? oFechaDesde = null;
            DateTime? oFechahasta = null;

            if (fechadesde?.Length > 0)
                oFechaDesde = DateTime.Parse(fechadesde);

            if (fechahasta?.Length > 0)
                oFechahasta = DateTime.Parse(fechahasta);

            List<dClaveComprobanteDeVenta>? lstClaves = fojaDeRepartoBLL.GetComprobantesNoAsignados(oFechaDesde, oFechahasta, repartidorID, vendedorID);

            // Igual que la V2: sin comprobantes → 404
            if (lstClaves == null || lstClaves.Count == 0)
                throw new cDatosNoEncontradosException("No se encontraron comprobantes sin asignar");

            List<cComprobanteVenta>? lstMovimientos = comprobanteDeVentaBLL.GetItemsByIDs(lstClaves, false, false, false, false, false);

            int cantidadElementos = lstMovimientos?.Count ?? 0;
            oRespuesta.comprobantes = new List<cMovimientoDeCliente>();

            if (lstMovimientos != null)
            {
                foreach (cComprobanteVenta oComprobante in lstMovimientos)
                {
                    oRespuesta.comprobantes.Add(new cMovimientoDeCliente(oComprobante));
                }
            }

            // NUEVO respecto de la V2: success = true cuando responde bien (la V2 nunca lo seteaba y devolvía false con datos)
            oRespuesta.success = true;
            oRespuesta.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

            LoguearInfo("Success GetComprobantesNoAsignados", Endpoint.Repartos.GetComprobantesNoAsignados, stopwatch, cantidadElementos, pageNumber, pageSize);

            return oRespuesta;
        }

        /// <summary>
        /// Crea fojas de reparto con sus comprobantes asignados.
        /// Igual que la V2: graba de a una; si una falla, sigue con las demás y la devuelve con success = false y el motivo en Comentario.
        /// </summary>
        public cResponseFojasDeRepartosSave Create(List<cFojaDeReparto>? lstFoja)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (lstFoja == null || lstFoja.Count == 0)
                throw new cCodigoNoDetalladoException("No se detallaron fojas de reparto a grabar");   // → 400

            // Igual que la V2: todas las fojas se graban en la empresa de la sesión
            lstFoja.ForEach(x => x.EmpresaID = (short)Sesion.EmpresaID);

            cResponseFojasDeRepartosSave oRespuesta = new cResponseFojasDeRepartosSave();
            oRespuesta.fojasReparto = new List<cFojaDeRepartoAPI>();
            oRespuesta.FojasProcesadas = lstFoja.Count;

            int importadas = 0;
            int erroneas = 0;

            foreach (cFojaDeReparto oFoja in lstFoja)
            {
                try
                {
                    if (oFoja.ListaDeComprobantesAsignados?.Count > 0)
                        oFoja.ListaDeComprobantesAsignados.ForEach(x => x.EmpresaID = (short)Sesion.EmpresaID);

                    oRespuesta.fojasReparto.Add(new cFojaDeRepartoAPI(fojaDeRepartoBLL.Save(oFoja), true));
                    importadas++;
                }
                catch (Exception ex)
                {
                    Logger.LoguearSucesosAPI("Foja No importada EmpresaID:" + oFoja.EmpresaID + " | Fecha: " + oFoja.Desde + " | Repartidor: " + oFoja.RepartidorID + " | Descripcion: " + ex.Message,
                                             tTiposDeError.tError, Sesion.Usuario, Endpoint.Repartos.Create, Sesion.EmpresaID, Sesion.SucursalID,
                                             IPRemoteDesktop: Sesion.RemoteIPAddress, IPPublica: Sesion.Forwarded, Agent: Sesion.UserAgent);

                    oFoja.Comentario = ex.Message;
                    oRespuesta.fojasReparto.Add(new cFojaDeRepartoAPI(oFoja, false));
                    erroneas++;
                }
            }

            oRespuesta.FojasImportadas = importadas;
            oRespuesta.FojasConErrores = erroneas;
            oRespuesta.success = true;

            LoguearInfo("Success Create Fojas Reparto Importadas:" + importadas + " Errores:" + erroneas, Endpoint.Repartos.Create, stopwatch, lstFoja.Count, 1, lstFoja.Count);

            return oRespuesta;
        }

        #region Auxiliares

        /// <summary>
        /// NUEVO respecto de la V2: la V2 pisaba la empresa de la sesión con el empresaID del query string,
        /// así que se podía operar con fojas de cualquier empresa. Ahora el empresaID tiene que ser el de la sesión;
        /// para trabajar con otra empresa habilitada se usan los headers empresaID / sucursalID (los valida el middleware).
        /// </summary>
        private void ValidarEmpresa(int empresaID)
        {
            if (empresaID > 0 && empresaID != Sesion.EmpresaID)
                throw new UnauthorizedAccessException("El empresaID " + empresaID + " no corresponde a la empresa de la sesión (" + Sesion.EmpresaID + "). Para operar con otra empresa envíe los headers empresaID y sucursalID.");   // → 403
        }

        #endregion
    }
}

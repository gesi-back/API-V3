using FluentFTP;
using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.CORE.V3.BLL;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using GESI.ERP.Core.V3.BO.Dinamicos;
using GESI.ERP.Finanzas.V3.BO;
using GESI.ERP.Ventas.V3.BLL;
using GESI.ERP.Ventas.V3.BO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Ventas
{
    public interface IMovimientosDeClientesServicio
    {
        cResponseMovimientoDeClientes GetItem(List<dClaveComprobanteDeVenta>? lstClaves,
                                              string subTipo = "",
                                              string Acopios = "N",
                                              int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                              int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                              string blIncluirDetalleDeVenta = "N",
                                              string blIncluirDetalleImpuestos = "N",
                                              string blIncluirRelacionesItems = "N",
                                              string blIncluirDetalleDeMedioDePago = "N",
                                              string blincluirDetalleDeVencimientos = "N");

        cResponseMovimientoDeClientes GetList(dParametrosConsultaDeComprobantesDatosDeLaOperacion? datosOperacion,
                                              dParametrosConsultaDeComprobantesDatosDelCliente? datosCliente,
                                              string subTipo,
                                              string EsAcopio = "N",
                                              int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                              int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                              bool DevolverFojaID = false);

        cResponse SetearEstadoComprobante(List<dClaveComprobanteDeVenta>? lstClaves, int estadoComprobanteID);

        List<cResponseSaveMovimientosClientes> SaveComprobanteDeVenta(List<cMovimientoDeCliente>? lstComprobantes, string subTipo, bool blEsUnAlta = true);

        List<cResponseSaveMovimientosClientes> SaveComprobanteDeCobro(List<cComprobanteCobro>? lstComprobantes);

        List<cResponseSaveRelaciones> SaveAplicacionesSaldos(List<cRelacionComprobantes>? lstRelaciones);

        List<cMovimientoDeCliente>? LeerComprobantesDePedido(string json);

        List<cComprobanteCobro>? LeerComprobantesDeCobro(string json);

        Task<List<cPDFComprobante>> DevolverJSONPDF(List<dClavesComprobante>? lstClaves, string subTipo);
    }

    /// <summary>
    /// Migrado de la V2 (Ventas.BLL.MovimientosDeClientesMgr estático con _MIAPISessionMgr estático).
    /// Lo usan todos los controllers de comprobantes de venta (Facturas, Pedidos, Remitos, Cobros, etc.);
    /// cada uno le pasa su subtipo de comprobante.
    ///
    /// ETAPA 1: consultas (GetItem, GetList) y SetearEstado.
    /// ETAPA 2: grabaciones (SaveComprobanteDeVenta, SaveComprobanteDeCobro, conversores de la API vieja, SaveAplicacionesSaldos).
    /// ETAPA 3: DevolverJSONPDF (links a los PDF del FTP).
    /// </summary>
    internal sealed class MovimientosDeClientesServicio(
        IComprobanteDeVentaBLL comprobanteDeVentaBLL,
        IFojaDeRepartoBLL fojaDeRepartoBLL,
        IComprobanteDeCobroBLL comprobanteDeCobroBLL,
        IRelacionesComprobantesBLL relacionesComprobantesBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger,
        IConfiguracionBaseBLL configuracionesBLL,
        IEmpresasBLL empresasBLL) : ServicioBase(apiHelper, sesion, apiLogger), IMovimientosDeClientesServicio
    {
        // Igual que la V2: un solo Save a la vez por tipo de comprobante, para que dos requests simultáneos no se pisen la numeración
        private static readonly object _lockPedidos = new object();
        private static readonly object _lockCobros = new object();

        /// <summary>
        /// Devuelve los comprobantes pedidos por clave (empresa, comprobante, serie, punto de venta, numero)
        /// </summary>
        public cResponseMovimientoDeClientes GetItem(List<dClaveComprobanteDeVenta>? lstClaves,
                                                     string subTipo = "",
                                                     string Acopios = "N",
                                                     int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                     int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                     string blIncluirDetalleDeVenta = "N",
                                                     string blIncluirDetalleImpuestos = "N",
                                                     string blIncluirRelacionesItems = "N",
                                                     string blIncluirDetalleDeMedioDePago = "N",
                                                     string blincluirDetalleDeVencimientos = "N")
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseMovimientoDeClientes oResponse = new cResponseMovimientoDeClientes();

            // NUEVO respecto de la V2: sin claves devuelve 400 (la V2 fallaba con error 500)
            if (lstClaves == null || lstClaves.Count == 0)
                throw new cCodigoNoDetalladoException("No se detallaron las claves de los comprobantes");   // → 400

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            string strEndpoint = DevolverEndpoint(subTipo, "GetItem");

            try
            {
                // Igual que la V2: todas las claves se buscan en la empresa de la sesión
                lstClaves.ForEach(comprobante => comprobante.EmpresaID = Sesion.EmpresaID);

                List<cComprobanteVenta>? lstComprobantes = comprobanteDeVentaBLL.GetItemsByIDs(lstClaves,
                                                                                              blIncluirDetalleDeVenta == "S",
                                                                                              blIncluirDetalleImpuestos == "S",
                                                                                              blIncluirRelacionesItems == "S",
                                                                                              blIncluirDetalleDeMedioDePago == "S",
                                                                                              blincluirDetalleDeVencimientos == "S");

                if (lstComprobantes == null || lstComprobantes.Count == 0)
                    throw new cDatosNoEncontradosException();

                if (subTipo?.Length > 0)
                    lstComprobantes = lstComprobantes.Where(x => x.Subtipo == subTipo).ToList();   // FILTRO POR EL SUBTIPO

                if (Acopios.Equals("S"))
                    lstComprobantes = lstComprobantes.Where(x => x.EsUnAcopio == true).ToList();

                if (lstComprobantes.Count == 0)
                    throw new cDatosNoEncontradosException();

                int cantidadElementos = lstComprobantes.Count;

                List<cComprobanteVenta> nuevosplit = lstComprobantes.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();   // GENERO LA PAGINACION DE LA RESPUESTA

                if (nuevosplit.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber));

                List<cMovimientoDeCliente> lstMovimientosDeClientes = new List<cMovimientoDeCliente>();
                foreach (cComprobanteVenta oComprobanteERP in nuevosplit)
                {
                    lstMovimientosDeClientes.Add(new cMovimientoDeCliente(oComprobanteERP));
                }

                oResponse.success = true;
                oResponse.comprobantes = lstMovimientosDeClientes;
                oResponse.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success MovimientosDeClientes GetItem SubTipo:" + subTipo + " | Detalle:" + blIncluirDetalleDeVenta + " | Relaciones:" + blIncluirRelacionesItems + " | MedioDePago:" + blIncluirDetalleDeMedioDePago,
                            strEndpoint, stopwatch, cantidadElementos, pageNumber, pageSize);

                return oResponse;
            }
            catch (cDatosNoEncontradosException)
            {
                // Igual que la V2: 200 con el error adentro y success = false
                LoguearInfo("Not Found MovimientosDeClientes GetItem", strEndpoint, stopwatch, 0, pageNumber, 0);

                oResponse.error = new cError((int)cCodigosError.cDatosNoEncontrados, "No se encontraron elementos", ApiErrors.errDatosNoEncontrados);
                return oResponse;
            }
            catch (cIntervalException oIntervalo)
            {
                // Igual que la V2: 200 con el error adentro y success = false
                oResponse.paginacion = oIntervalo._paginacion;
                oResponse.error = new cError((int)cCodigosError.cPaginacionExcedida, "Intervalo de Paginacion Excedido", ApiErrors.errPaginacionExcedida);
                return oResponse;
            }
        }

        /// <summary>
        /// Devuelve los comprobantes que cumplen los filtros
        /// </summary>
        public cResponseMovimientoDeClientes GetList(dParametrosConsultaDeComprobantesDatosDeLaOperacion? datosOperacion,
                                                     dParametrosConsultaDeComprobantesDatosDelCliente? datosCliente,
                                                     string subTipo,
                                                     string EsAcopio = "N",
                                                     int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                     int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                     bool DevolverFojaID = false)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseMovimientoDeClientes oRespuesta = new cResponseMovimientoDeClientes();

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            if (datosOperacion == null)
                datosOperacion = new dParametrosConsultaDeComprobantesDatosDeLaOperacion();

            // El subtipo lo decide el controller, no el body
            datosOperacion.subTipo = subTipo;

            string strEndpoint = DevolverEndpoint(subTipo, "GetList");

            try
            {
                List<cComprobanteVenta>? lstLista = comprobanteDeVentaBLL.GetList(datosOperacion, datosCliente!, true);

                if (lstLista == null || lstLista.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron comprobantes en la busqueda");

                lstLista = lstLista.OrderBy(x => x.Numero).ToList();

                if (EsAcopio.Equals("S"))
                    lstLista = lstLista.Where(x => x.EsUnAcopio == true).ToList();

                int cantidadElementos = lstLista.Count;

                lstLista = lstLista.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstLista.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, 0));

                oRespuesta.success = true;
                oRespuesta.comprobantes = new List<cMovimientoDeCliente>();
                foreach (cComprobanteVenta oComprobante in lstLista)
                {
                    oRespuesta.comprobantes.Add(new cMovimientoDeCliente(oComprobante));
                }

                if (DevolverFojaID)   // DEVUELVE EL CODIGO DE FOJA DE REPARTO RELACIONADA
                    CargarFojasDeReparto(oRespuesta.comprobantes);

                oRespuesta.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                // La V2 además metía en el log todo el JSON de la respuesta; acá se loguean solo los parámetros
                LoguearInfo("Success MovimientosDeClientes GetList SubTipo:" + subTipo + " DevolverIDFoja:" + DevolverFojaID + " Acopio:" + EsAcopio,
                            strEndpoint, stopwatch, cantidadElementos, pageNumber, pageSize);

                return oRespuesta;
            }
            catch (cIntervalException oIntervalo)
            {
                oRespuesta.paginacion = oIntervalo._paginacion;
                oRespuesta.success = true;
                oRespuesta.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);

                return oRespuesta;
            }
            catch (cDatosNoEncontradosException oDatosNoEncontrados)
            {
                oRespuesta.success = true;
                oRespuesta.error = new cError((int)cCodigosError.cDatosNoEncontrados, oDatosNoEncontrados.Message, ApiErrors.errDatosNoEncontrados);

                return oRespuesta;
            }
        }

        /// <summary>
        /// Cambia el estado de los comprobantes indicados
        /// </summary>
        public cResponse SetearEstadoComprobante(List<dClaveComprobanteDeVenta>? lstClaves, int estadoComprobanteID)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponse oRespuesta = new cResponse();

            // NUEVO respecto de la V2: sin estado o sin claves devuelve 400 (la V2 devolvía 200 con el error adentro)
            if (estadoComprobanteID == 0)
                throw new cCodigoNoDetalladoException("No se especificó el estadoComprobanteID");   // → 400

            if (lstClaves == null || lstClaves.Count == 0)
                throw new cCodigoNoDetalladoException("No se detallaron las claves de los comprobantes");   // → 400

            // Igual que la V2: solo se tocan comprobantes de la empresa de la sesión
            lstClaves.ForEach(x => x.EmpresaID = Sesion.EmpresaID);

            (bool blRespuesta, List<string>? lstErrores) = comprobanteDeVentaBLL.SetEstado(lstClaves, estadoComprobanteID);

            if (lstErrores?.Count > 0)
                oRespuesta.error = new cError((int)cCodigosError.cErrorInternoAplicacion, string.Join(",", lstErrores), ApiErrors.errInternalServerError);

            oRespuesta.success = blRespuesta;

            LoguearInfo("MovimientosDeClientes SetearEstado " + estadoComprobanteID + " Resultado:" + blRespuesta,
                        Endpoint.Ventas.Comprobantes.Facturas("SetearEstado"), stopwatch, lstClaves.Count, 1, lstClaves.Count);

            return oRespuesta;
        }

        #region Grabaciones

        /// <summary>
        /// Graba / Edita una lista de comprobantes de venta (pedidos, presupuestos, devoluciones).
        /// Devuelve un resultado por comprobante; los rechazados vienen con success = false y el motivo en mensaje.
        /// </summary>
        public List<cResponseSaveMovimientosClientes> SaveComprobanteDeVenta(List<cMovimientoDeCliente>? lstComprobantes,
                                                                             string subTipo,
                                                                             bool blEsUnAlta = true)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string strQueEstabaHaciendo = "";

            // NUEVO respecto de la V2: el log usa el endpoint del subtipo (la V2 logueaba las devoluciones como "Pedidos")
            string strEndpoint = DevolverEndpoint(subTipo, "Create");

            if (lstComprobantes == null || lstComprobantes.Count == 0)
                throw new cCodigoNoDetalladoException("No se detallaron comprobantes a Grabar");   // → 400

            try
            {
                List<cResponseSaveMovimientosClientes> lstRespuestaSave = new List<cResponseSaveMovimientosClientes>();

                List<cMovimientoDeCliente> lstComprobantesAImportar = lstComprobantes.Where(x => x.Subtipo == subTipo).ToList();
                List<cMovimientoDeCliente> lstComprobantesSinComprobanteID = lstComprobantes.Where(x => x.ComprobanteID == 0).ToList();
                List<cMovimientoDeCliente> lstComprobantesConDistintoSubTipo = lstComprobantes.Where(x => x.Subtipo != subTipo).ToList();   // VERIFICO LOS COMPROBANTES QUE TIENEN DISTINTO SUBTIPO O NO TIENEN NINGUNO ASIGNADO

                lstComprobantesAImportar = lstComprobantesAImportar.Where(x => x.ComprobanteID > 0).ToList();

                #region Comprobantes con Distinto SubTipo

                strQueEstabaHaciendo = "Verifica los comprobantes con Distinto SubTipo";

                if (lstComprobantesConDistintoSubTipo.Count > 0)
                {
                    foreach (cComprobanteVenta oMovimientoConDistintoSubTipo in ConvertirMovimientoAComprobanteDeVenta(lstComprobantesConDistintoSubTipo))
                    {
                        lstRespuestaSave.Add(ConvertirComprobanteDeVentaASaveMovimientos(oMovimientoConDistintoSubTipo, false, "Comprobante con distinto SubTipo"));
                    }
                }

                #endregion

                #region Comprobantes sin ComprobanteID

                strQueEstabaHaciendo = "Verifico los comprobantes sin ComprobanteID";

                if (lstComprobantesSinComprobanteID.Count > 0)
                {
                    foreach (cComprobanteVenta oMovimientoSinComprobanteID in ConvertirMovimientoAComprobanteDeVenta(lstComprobantesSinComprobanteID))
                    {
                        lstRespuestaSave.Add(ConvertirComprobanteDeVentaASaveMovimientos(oMovimientoSinComprobanteID, false, "Comprobante sin ComprobanteID"));
                    }
                }

                #endregion

                #region Comprobantes sin Codigo de Importacion

                strQueEstabaHaciendo = "Verifico los comprobantes sin codigo de importacion";

                if (Sesion.UsuarioExterno)
                {
                    List<cMovimientoDeCliente> lstComprobantesSinCodigoDeImportacion = lstComprobantesAImportar.Where(x => x.CodigoDeImportacion == null || x.CodigoDeImportacion.Equals("")).ToList();

                    if (lstComprobantesSinCodigoDeImportacion.Count > 0)
                    {
                        foreach (cComprobanteVenta oMovimientoSinCodigoDeImportacion in ConvertirMovimientoAComprobanteDeVenta(lstComprobantesSinCodigoDeImportacion))
                        {
                            lstRespuestaSave.Add(ConvertirComprobanteDeVentaASaveMovimientos(oMovimientoSinCodigoDeImportacion, false, "Comprobante sin Codigo de Importacion"));
                        }
                    }

                    lstComprobantesAImportar = lstComprobantesAImportar.Where(x => x.CodigoDeImportacion?.Length > 0).ToList();
                }

                #endregion

                if (lstComprobantesAImportar.Count > 0)
                {
                    lstComprobantesAImportar.ForEach(comprobante => comprobante.EmpresaID = Sesion.EmpresaID);   // ASIGNO A TODOS LOS ELEMENTOS LA EMPRESAID

                    List<cComprobanteVenta> listaComprobantes = ConvertirMovimientoAComprobanteDeVenta(lstComprobantesAImportar);
                    List<cComprobanteVenta>? lstComprobantesImportados;
                    Dictionary<string, (string, string)>? lstErrores;

                    strQueEstabaHaciendo = "Mando a grabar los comprobantes a la DLL Save";

                    lock (_lockPedidos)
                    {
                        (lstComprobantesImportados, lstErrores) = comprobanteDeVentaBLL.Save(listaComprobantes,
                                                                                             blEsUnAlta,
                                                                                             Sesion.ReleePreciosAlImportarComprobantes!);   // GRABO LOS COMPROBANTES EN CONDICIONES
                    }

                    #region Comprobantes Importados

                    if (lstComprobantesImportados?.Count > 0)
                    {
                        foreach (cComprobanteVenta oComprobanteImportado in lstComprobantesImportados)
                        {
                            LoguearComprobante("Comprobante Importado : " + oComprobanteImportado.ComprobanteID + ";" + oComprobanteImportado.Serie + ";" + oComprobanteImportado.PuntoDeVentaID + ";" + oComprobanteImportado.Numero,
                                               tTiposDeError.tInformativo, strEndpoint, stopwatch);

                            lstRespuestaSave.Add(ConvertirComprobanteDeVentaASaveMovimientos(oComprobanteImportado, true));
                        }
                    }

                    #endregion

                    #region Comprobantes con Errores
                    // FORMATO DE LA CLAVE DEL ERROR - [IndiceCodigoError;ComprobanteID;Serie;PuntoDeVenta;Numero;CodigoDeImportacion]

                    strQueEstabaHaciendo = "Convierte a SaveMovimientosClientes y a su vez loggea el tipo de error";

                    if (lstErrores?.Count > 0)
                    {
                        int numeroComprSumar = 0;
                        foreach (KeyValuePair<string, (string, string)> oErrores in lstErrores)
                        {
                            List<string> listaStrings = oErrores.Key.Split(';').ToList();

                            string strnumeroComprobante = listaStrings[4];
                            if (listaStrings[4].IndexOf("-") != -1)   // SI VIENE CON UN GUION LO SEPARO PARA TOMAR SOLAMENTE EL NUMERO DE COMPROBANTE
                                strnumeroComprobante = listaStrings[4].Split('-').First();

                            int numeroCompr = Convert.ToInt32(strnumeroComprobante) + numeroComprSumar;
                            string strcomprobanteID = listaStrings[1];
                            string strserie = listaStrings[2];
                            string strpuntoDeVentaID = listaStrings[3];
                            string strCodigoImportacion = listaStrings[5];

                            if (strCodigoImportacion.IndexOf("_") != -1)
                                strCodigoImportacion = strCodigoImportacion.Split('_').Last();

                            dClaveComprobanteDeVenta oClave = new dClaveComprobanteDeVenta(Sesion.EmpresaID,
                                                                                           Convert.ToInt32(strcomprobanteID),
                                                                                           strserie,
                                                                                           Convert.ToInt32(strpuntoDeVentaID),
                                                                                           numeroCompr);

                            List<cResponseSaveMovimientosClientes> oRespuestaSave = lstRespuestaSave.Where(x => x.comprobanteID == oClave.ComprobanteID &&
                                                                                                               x.serie == oClave.Serie &&
                                                                                                               x.numero == oClave.Numero &&
                                                                                                               x.puntoDeVentaID == oClave.PuntoDeVentaID &&
                                                                                                               x.codigoDeImportacion == strCodigoImportacion).ToList();

                            string mensaje = oErrores.Value.Item1;
                            bool blEsError = oErrores.Value.Item2.Equals(tTiposDeError.tError);

                            if (oRespuestaSave.Count == 0)
                            {
                                cComprobanteVenta? oComprobanteABuscar = listaComprobantes.FirstOrDefault(x => x.EmpresaID == oClave.EmpresaID &&
                                                                                                               x.ComprobanteID == oClave.ComprobanteID &&
                                                                                                               x.Serie == oClave.Serie &&
                                                                                                               x.PuntoDeVentaID == oClave.PuntoDeVentaID &&
                                                                                                               x.Numero == oClave.Numero &&
                                                                                                               x.CodigoDeImportacion == strCodigoImportacion);

                                if (oComprobanteABuscar != null)
                                {
                                    lstRespuestaSave.Add(ConvertirComprobanteDeVentaASaveMovimientos(oComprobanteABuscar, false, mensaje));
                                }
                                else
                                {
                                    cResponseSaveMovimientosClientes oRespuesta = new cResponseSaveMovimientosClientes();
                                    oRespuesta.mensaje = mensaje;
                                    oRespuesta.success = !blEsError;
                                    oRespuesta.serie = oClave.Serie;
                                    oRespuesta.numero = oClave.Numero;
                                    oRespuesta.codigoDeImportacion = strCodigoImportacion;
                                    oRespuesta.comprobanteID = oClave.ComprobanteID;
                                    oRespuesta.puntoDeVentaID = oClave.PuntoDeVentaID;
                                    lstRespuestaSave.Add(oRespuesta);
                                }
                            }
                            else
                            {
                                // El comprobante ya estaba en la respuesta: le pongo el mensaje y, si es un error, lo marco como fallido
                                foreach (cResponseSaveMovimientosClientes oExistente in lstRespuestaSave)
                                {
                                    if (oExistente.comprobanteID == oClave.ComprobanteID &&
                                        oExistente.serie == oClave.Serie &&
                                        oExistente.numero == oClave.Numero &&
                                        oExistente.puntoDeVentaID == oClave.PuntoDeVentaID)
                                    {
                                        oExistente.mensaje = mensaje;
                                        oExistente.success = !blEsError;
                                    }
                                }
                            }

                            numeroComprSumar++;
                        }
                    }

                    #endregion

                    lstRespuestaSave = AgruparRespuestas(lstRespuestaSave);   // AGRUPA LA RESPUESTA EN CASO QUE HAYA UN COMPROBANTE CON MAS DE UN ERROR

                    #region Loguea los Comprobantes con errores

                    foreach (cResponseSaveMovimientosClientes oMovimientoALoguear in lstRespuestaSave)
                    {
                        if (!oMovimientoALoguear.success)
                            LoguearComprobante("Error en Importacion Comprobante : " + oMovimientoALoguear.comprobanteID + ";" + oMovimientoALoguear.serie + ";" + oMovimientoALoguear.puntoDeVentaID + ";" + oMovimientoALoguear.numero + ";" + oMovimientoALoguear.codigoDeImportacion,
                                               tTiposDeError.tError, strEndpoint, stopwatch);
                    }

                    #endregion
                }

                LoguearInfo("Success MovimientosDeClientes Save SubTipo: " + subTipo, strEndpoint, stopwatch, lstComprobantes.Count, 1, lstComprobantes.Count);

                return lstRespuestaSave;
            }
            catch (Exception ex) when (ex is not cCodigoNoDetalladoException)
            {
                // Igual que la V2: deja en el log en qué paso falló y relanza (el controller arma la respuesta de error)
                Logger.LoguearSucesosAPI(strQueEstabaHaciendo + " | EmpresaID: " + Sesion.EmpresaID + " | " + ex.Message,
                                         tTiposDeError.tError, Sesion.Usuario, strEndpoint, Sesion.EmpresaID, Sesion.SucursalID);
                throw;
            }
        }

        /// <summary>
        /// Graba / Edita una lista de comprobantes de cobro (recibos), con sus valores y aplicaciones.
        /// Devuelve un resultado por comprobante; los rechazados vienen con success = false y el motivo en mensaje.
        /// </summary>
        public List<cResponseSaveMovimientosClientes> SaveComprobanteDeCobro(List<cComprobanteCobro>? lstComprobantes)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string strEndpoint = Endpoint.Ventas.Comprobantes.Cobros("Create");

            if (lstComprobantes == null || lstComprobantes.Count == 0)
                throw new cCodigoNoDetalladoException("No se detallaron comprobantes a Grabar");   // → 400

            List<cResponseSaveMovimientosClientes> lstRespuestaSave = new List<cResponseSaveMovimientosClientes>();

            List<cComprobanteCobro> lstComprobantesConDistintoSubTipo = lstComprobantes.Where(x => x.Subtipo != SubTiposComprobantes.sRecibo).ToList();
            List<cComprobanteCobro> lstComprobantesSinComprobanteID = lstComprobantes.Where(x => x.ComprobanteID == 0).ToList();

            lstComprobantes = lstComprobantes.Where(x => x.Subtipo == SubTiposComprobantes.sRecibo).ToList();   // Filtro solamente los SubTipo R
            lstComprobantes = lstComprobantes.Where(x => x.ComprobanteID > 0).ToList();                        // Filtro solamente los comprobanteID > 0

            #region Comprobantes Con Distinto SubTipo

            foreach (cComprobanteCobro oCobroConDistintoSubTipo in lstComprobantesConDistintoSubTipo)
            {
                lstRespuestaSave.Add(ConvertirComprobanteDeCobroASaveMovimientos(oCobroConDistintoSubTipo, false, "Comprobante con distinto Subtipo"));
            }

            #endregion

            #region Comprobantes sin ComprobanteID

            foreach (cComprobanteCobro oCobroSinComprobanteID in lstComprobantesSinComprobanteID)
            {
                lstRespuestaSave.Add(ConvertirComprobanteDeCobroASaveMovimientos(oCobroSinComprobanteID, false, "Comprobante sin ComprobanteID"));
            }

            #endregion

            #region Comprobantes Habilitados a Importar

            if (lstComprobantes.Count > 0)
            {
                // NUEVO respecto de la V2: se fuerza la empresa de la sesión (la V2 grababa la EmpresaID que viniera en el body)
                lstComprobantes.ForEach(comprobante => comprobante.EmpresaID = Sesion.EmpresaID);

                List<cComprobanteCobro>? lstComprobantesDeCobroGrabados;
                Dictionary<string, (string, string)>? dicErrores;

                lock (_lockCobros)   // BLOQUEO PARA EVITAR POSIBLES EJECUCIONES SIMULTANEAS
                {
                    (lstComprobantesDeCobroGrabados, dicErrores) = comprobanteDeCobroBLL.Save(lstComprobantes);
                }

                if (lstComprobantesDeCobroGrabados?.Count > 0)
                {
                    foreach (cComprobanteCobro oComprobanteImportado in lstComprobantesDeCobroGrabados)
                    {
                        LoguearComprobante("Cobro Importado : " + oComprobanteImportado.ComprobanteID + ";" + oComprobanteImportado.Serie + ";" + oComprobanteImportado.PuntoDeVentaID + ";" + oComprobanteImportado.Numero,
                                           tTiposDeError.tInformativo, strEndpoint, stopwatch);

                        lstRespuestaSave.Add(ConvertirComprobanteDeCobroASaveMovimientos(oComprobanteImportado, true));
                    }
                }

                #region Comprobantes con Errores
                // FORMATO DE LA CLAVE DEL ERROR - [IndiceCodigoError;ComprobanteID;Serie;PuntoDeVenta;Numero;CodigoDeImportacion]

                if (dicErrores?.Count > 0)
                {
                    int numeroComprSumar = 0;
                    foreach (KeyValuePair<string, (string, string)> oError in dicErrores)
                    {
                        List<string> listaStrings = oError.Key.Split(';').ToList();

                        string strnumeroComprobante = listaStrings[4];
                        if (listaStrings[4].IndexOf("-") != -1)   // SI VIENE CON UN GUION LO SEPARO PARA TOMAR SOLAMENTE EL NUMERO DE COMPROBANTE
                            strnumeroComprobante = listaStrings[4].Split('-').First();

                        int numeroCompr = Convert.ToInt32(strnumeroComprobante) + numeroComprSumar;
                        string strcomprobanteID = listaStrings[1];
                        string strserie = listaStrings[2];
                        string strpuntoDeVentaID = listaStrings[3];
                        string strCodigoImportacion = listaStrings[5];
                        string strCodigoImportacionsinformatear = strCodigoImportacion;

                        if (strCodigoImportacion.IndexOf("_") != -1)
                            strCodigoImportacion = strCodigoImportacion.Split('_').Last();

                        dClaveComprobanteDeVenta oClave = new dClaveComprobanteDeVenta(Sesion.EmpresaID,
                                                                                       Convert.ToInt32(strcomprobanteID),
                                                                                       strserie,
                                                                                       Convert.ToInt32(strpuntoDeVentaID),
                                                                                       numeroCompr);

                        // Igual que la V2: en cobros la búsqueda no compara el número (se asigna al grabar)
                        List<cResponseSaveMovimientosClientes> oRespuestaSave = lstRespuestaSave.Where(x => x.comprobanteID == oClave.ComprobanteID &&
                                                                                                           x.serie == oClave.Serie &&
                                                                                                           x.puntoDeVentaID == oClave.PuntoDeVentaID &&
                                                                                                           x.codigoDeImportacion == strCodigoImportacion).ToList();

                        string mensaje = oError.Value.Item1;
                        bool blEsError = oError.Value.Item2.Equals(tTiposDeError.tError);

                        if (oRespuestaSave.Count == 0)
                        {
                            cComprobanteCobro? oComprobanteABuscar = lstComprobantes.FirstOrDefault(x => x.EmpresaID == oClave.EmpresaID &&
                                                                                                         x.ComprobanteID == oClave.ComprobanteID &&
                                                                                                         x.Serie == oClave.Serie &&
                                                                                                         x.PuntoDeVentaID == oClave.PuntoDeVentaID &&
                                                                                                         x.CodigoDeImportacion == strCodigoImportacionsinformatear);

                            if (oComprobanteABuscar != null)
                            {
                                lstRespuestaSave.Add(ConvertirComprobanteDeCobroASaveMovimientos(oComprobanteABuscar, false, mensaje));
                            }
                            else
                            {
                                cResponseSaveMovimientosClientes oRespuesta = new cResponseSaveMovimientosClientes();
                                oRespuesta.mensaje = mensaje;
                                oRespuesta.serie = oClave.Serie;
                                oRespuesta.numero = oClave.Numero;
                                oRespuesta.codigoDeImportacion = strCodigoImportacion;
                                oRespuesta.comprobanteID = oClave.ComprobanteID;
                                oRespuesta.puntoDeVentaID = oClave.PuntoDeVentaID;
                                lstRespuestaSave.Add(oRespuesta);
                            }
                        }
                        else
                        {
                            // El comprobante ya estaba en la respuesta: le pongo el mensaje y, si es un error, lo marco como fallido
                            foreach (cResponseSaveMovimientosClientes oExistente in lstRespuestaSave)
                            {
                                if (oExistente.comprobanteID == oClave.ComprobanteID &&
                                    oExistente.serie == oClave.Serie &&
                                    oExistente.numero == oClave.Numero &&
                                    oExistente.puntoDeVentaID == oClave.PuntoDeVentaID)
                                {
                                    oExistente.mensaje = mensaje;
                                    oExistente.success = !blEsError;
                                }
                            }
                        }

                        numeroComprSumar++;
                    }

                    lstRespuestaSave = AgruparRespuestas(lstRespuestaSave);   // AGRUPA LA RESPUESTA EN CASO QUE HAYA UN COMPROBANTE CON MAS DE UN ERROR

                    #region Loguea los Comprobantes con errores

                    foreach (cResponseSaveMovimientosClientes oMovimientoALoguear in lstRespuestaSave)
                    {
                        if (!oMovimientoALoguear.success)
                            LoguearComprobante("Error en Importacion Cobro : " + oMovimientoALoguear.comprobanteID + ";" + oMovimientoALoguear.serie + ";" + oMovimientoALoguear.puntoDeVentaID + ";" + oMovimientoALoguear.numero + ";" + oMovimientoALoguear.codigoDeImportacion + " Razon:" + oMovimientoALoguear.mensaje,
                                               tTiposDeError.tError, strEndpoint, stopwatch);
                    }

                    #endregion
                }

                #endregion
            }

            #endregion

            LoguearInfo("Success Movimientos de Clientes Save", strEndpoint, stopwatch, lstComprobantes.Count, 1, lstComprobantes.Count);

            return lstRespuestaSave;
        }

        /// <summary>
        /// Aplicacion de saldos de Recibos / NCR a facturas.
        /// Devuelve una fila por aplicación informada por el Core: "OK" → success = true; cualquier otro texto → success = false con el error.
        /// </summary>
        public List<cResponseSaveRelaciones> SaveAplicacionesSaldos(List<cRelacionComprobantes>? lstRelaciones)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<cResponseSaveRelaciones> oRespuesta = new List<cResponseSaveRelaciones>();

            if (lstRelaciones == null || lstRelaciones.Count == 0)
                throw new cCodigoNoDetalladoException("No se detallaron comprobantes a Grabar");   // → 400

            // Igual que la V2: solo se aplican comprobantes de la empresa de la sesión
            lstRelaciones.ForEach(x => x.EmpresaID = Sesion.EmpresaID);

            (bool success, Dictionary<cRelacionComprobantes, string>? lstErrores) = relacionesComprobantesBLL.Save(lstRelaciones);

            if (lstErrores?.Count > 0)
            {
                foreach (KeyValuePair<cRelacionComprobantes, string> Elemento in lstErrores)
                {
                    cResponseSaveRelaciones oSaveRelacion = new cResponseSaveRelaciones();
                    oSaveRelacion.SubTipo = Elemento.Key.SubTipo;
                    oSaveRelacion.PuntoDeVentaID = Elemento.Key.PuntoDeVentaID;
                    oSaveRelacion.Vto_PuntoDeVentaID = Elemento.Key.Vto_PuntoDeVentaID;
                    oSaveRelacion.ComprobanteID = Elemento.Key.ComprobanteID;
                    oSaveRelacion.Importe = Elemento.Key.Importe;
                    oSaveRelacion.Serie = Elemento.Key.Serie;
                    oSaveRelacion.Vto_Serie = Elemento.Key.Vto_Serie;
                    oSaveRelacion.Numero = Elemento.Key.Numero;
                    oSaveRelacion.Vto_ComprobanteID = Elemento.Key.Vto_ComprobanteID;
                    oSaveRelacion.Vto_ItemID = Elemento.Key.Vto_ItemID;
                    oSaveRelacion.Vto_Numero = Elemento.Key.Vto_Numero;
                    oSaveRelacion.Fecha = Elemento.Key.Fecha;

                    if (Elemento.Value.Equals("OK"))
                    {
                        oSaveRelacion.success = true;
                    }
                    else
                    {
                        oSaveRelacion.success = false;
                        oSaveRelacion.error = new cError((int)cCodigosError.cErrorInternoAplicacion, Elemento.Value, ApiErrors.errParametroInvalido);
                    }

                    oRespuesta.Add(oSaveRelacion);
                }
            }

            LoguearInfo("MovimientosDeClientes AplicarSaldo Resultado:" + success, Endpoint.Ventas.Comprobantes.NotasDeCredito("AplicarSaldo"),
                        stopwatch, lstRelaciones.Count, 1, lstRelaciones.Count);

            return oRespuesta;
        }

        #endregion

        #region Lectura del body de Pedidos / Cobros (formato nuevo o formato de la API vieja)

        /// <summary>
        /// Igual que la V2: si algún comprobante trae "Cliente" es el formato de la API vieja (cComprobanteDePedidoAPI) y se convierte;
        /// si no, es una lista de cMovimientoDeCliente. Se usa Newtonsoft como la V2, que acepta fechas y números en más formatos.
        /// </summary>
        public List<cMovimientoDeCliente>? LeerComprobantesDePedido(string json)
        {
            try
            {
                List<cComprobanteDePedidoAPI>? lstComprobantesPedidosAPIVieja = JsonConvert.DeserializeObject<List<cComprobanteDePedidoAPI>>(json);

                if (lstComprobantesPedidosAPIVieja == null)
                    return null;

                bool todosSonNulos = lstComprobantesPedidosAPIVieja.All(c => c.Cliente == null);   // SI TODOS LOS OBJETOS CLIENTE SON NULL, ES EL FORMATO NUEVO

                if (todosSonNulos)
                    return JsonConvert.DeserializeObject<List<cMovimientoDeCliente>>(json);

                return ConvertirComprobantePedidoAPIViejaAComprobanteDeVenta(lstComprobantesPedidosAPIVieja);
            }
            catch (JsonException ex)
            {
                // NUEVO respecto de la V2: un body que no se puede leer devuelve 400 (la V2 devolvía 500)
                throw new cFormatoInvalidoException("El formato de los comprobantes es invalido: " + ex.Message);
            }
        }

        /// <summary>
        /// Igual que la V2: si algún cobro trae "Cliente" es el formato de la API vieja (cComprobanteCobroAPI) y se convierte;
        /// si no, es una lista de cComprobanteCobro.
        /// </summary>
        public List<cComprobanteCobro>? LeerComprobantesDeCobro(string json)
        {
            try
            {
                List<cComprobanteCobroAPI>? lstComprobantesCobroAPIVieja = JsonConvert.DeserializeObject<List<cComprobanteCobroAPI>>(json);

                if (lstComprobantesCobroAPIVieja == null)
                    return null;

                bool todosSonNulos = lstComprobantesCobroAPIVieja.All(c => c.Cliente == null);   // SI TODOS LOS OBJETOS CLIENTE SON NULL, ES EL FORMATO NUEVO

                if (todosSonNulos)
                    return JsonConvert.DeserializeObject<List<cComprobanteCobro>>(json);

                return ConvertirComprobanteCobroAPIViejaAComprobanteDeCobro(lstComprobantesCobroAPIVieja);
            }
            catch (JsonException ex)
            {
                // NUEVO respecto de la V2: un body que no se puede leer devuelve 400 (la V2 devolvía 500)
                throw new cFormatoInvalidoException("El formato de los cobros es invalido: " + ex.Message);
            }
        }

        /// <summary>
        /// Convierte el pedido de la API vieja en cMovimientoDeCliente
        /// </summary>
        private List<cMovimientoDeCliente> ConvertirComprobantePedidoAPIViejaAComprobanteDeVenta(List<cComprobanteDePedidoAPI> lstComprobanteAPIVieja)
        {
            List<cMovimientoDeCliente> lstMovimientoCliente = new List<cMovimientoDeCliente>();

            foreach (cComprobanteDePedidoAPI ComprobanteAPIVieja in lstComprobanteAPIVieja)
            {
                #region ENCABEZADO DEL COMPROBANTE

                cMovimientoDeCliente oMovimiento = new cMovimientoDeCliente();
                oMovimiento.ClienteID = ComprobanteAPIVieja.Cliente?.ClienteID ?? 0;
                oMovimiento.ComprobanteID = ComprobanteAPIVieja.ComprobanteID;
                oMovimiento.CodigoDeImportacion = Sesion.Usuario + "_" + ComprobanteAPIVieja.PedidoID;
                oMovimiento.FechaDeEmision = DateTime.Parse(ComprobanteAPIVieja.FechaRegistracion ?? "");   // fecha inválida → 400
                oMovimiento.FechaDeEntrega = DateTime.Parse(ComprobanteAPIVieja.FechaEntrega ?? "");
                oMovimiento.Notas = ComprobanteAPIVieja.Notas;
                oMovimiento.Subtipo = SubTiposComprobantes.sNPE;
                oMovimiento.CanalDeAtencionID = ComprobanteAPIVieja.FormaIngreso;
                oMovimiento.DescuentoPorcentaje = ComprobanteAPIVieja.DescuentoPorcentajes;
                oMovimiento.DescuentoImporte = ComprobanteAPIVieja.DescuentoImporte;
                oMovimiento.DescuentoTexto = ComprobanteAPIVieja.DescuentoTexto;
                oMovimiento.VendedorID = ComprobanteAPIVieja.VendedorID;

                if (!string.IsNullOrWhiteSpace(ComprobanteAPIVieja.Hora))
                {
                    TimeSpan hora = TimeSpan.Parse(ComprobanteAPIVieja.Hora);
                    oMovimiento.Hora = oMovimiento.FechaDeEmision.Date.Add(hora);
                }

                if (ComprobanteAPIVieja.SucursalCliente != null)
                    oMovimiento.SucursalClienteID = ComprobanteAPIVieja.SucursalCliente.SucursalID;

                #endregion

                #region Detalle de venta

                if (ComprobanteAPIVieja.Items?.Count > 0)
                {
                    oMovimiento.DetalleDeItems = new List<cItemComprobanteDeVenta>();
                    foreach (cItemAPI oItem in ComprobanteAPIVieja.Items)
                    {
                        cItemComprobanteDeVenta oItemNuevaAPI = new cItemComprobanteDeVenta();
                        oItemNuevaAPI.CodigoDeItem = oItem.ProductoID ?? "";
                        oItemNuevaAPI.CantidadU1 = oItem.UnidadU1;
                        oItemNuevaAPI.CantidadU2 = oItem.UnidadU2;
                        oItemNuevaAPI.TipoDeItem = "P";
                        oItemNuevaAPI.PrecioUnitarioNeto = oItem.Precio;   // PARA QUE LO TOME EL METODO TIENE QUE ESTAR SETEADO CON VALOR PrecioUnitarioNeto
                        oItemNuevaAPI.Comentario = oItem.Comentario ?? "";
                        oItemNuevaAPI.DescuentosPorcentaje = oItem.DescuentoPorcentajes ?? "";
                        oMovimiento.DetalleDeItems.Add(oItemNuevaAPI);
                    }
                }

                #endregion

                lstMovimientoCliente.Add(oMovimiento);
            }

            return lstMovimientoCliente;
        }

        /// <summary>
        /// Convierte el cobro de la API vieja en cComprobanteCobro
        /// </summary>
        private List<cComprobanteCobro> ConvertirComprobanteCobroAPIViejaAComprobanteDeCobro(List<cComprobanteCobroAPI> lstComprobanteAPIVieja)
        {
            List<cComprobanteCobro> lstComprobantesCobro = new List<cComprobanteCobro>();

            foreach (cComprobanteCobroAPI oCobroAPI in lstComprobanteAPIVieja)
            {
                cComprobanteCobro oCobroNuevaAPI = new cComprobanteCobro();
                oCobroNuevaAPI.EmpresaID = Sesion.EmpresaID;
                oCobroNuevaAPI.CodigoDeImportacion = Sesion.Usuario + "_" + oCobroAPI.CobroID;
                oCobroNuevaAPI.ImporteTotal = oCobroAPI.TotalCobrado;
                oCobroNuevaAPI.FechaDeEmision = DateTime.Parse(oCobroAPI.FechaRegistracion ?? "");   // fecha inválida → 400
                oCobroNuevaAPI.ClienteID = oCobroAPI.Cliente?.ClienteID ?? 0;
                oCobroNuevaAPI.Numero = 0;
                oCobroNuevaAPI.SucursalID = Sesion.SucursalID;
                oCobroNuevaAPI.ComprobanteID = oCobroAPI.ComprobanteID;
                oCobroNuevaAPI.Notas = oCobroAPI.Notas;
                oCobroNuevaAPI.Subtipo = SubTiposComprobantes.sRecibo;
                oCobroNuevaAPI.TipoDeOperacionID = 1;   // Igual que la V2 (TODO de la V2: tomar el de valores predeterminados)
                oCobroNuevaAPI.cobradorID = oCobroAPI.CobradorID;

                #region Aplicacion Cobros Vencimientos

                if (oCobroAPI.ComprobantesAplicados?.Count > 0)
                {
                    oCobroNuevaAPI.AplicacionesADebitos = new List<cRelacionComprobantes>();
                    foreach (cComprobanteAplicadoAPI oAplicacion in oCobroAPI.ComprobantesAplicados)
                    {
                        cRelacionComprobantes oRelacion = new cRelacionComprobantes();
                        oRelacion.EmpresaID = Sesion.EmpresaID;
                        oRelacion.Vto_ComprobanteID = oAplicacion.ComprobanteID;
                        oRelacion.Vto_Numero = oAplicacion.Numero;
                        oRelacion.Vto_PuntoDeVentaID = oAplicacion.PuntoDeVentaID;
                        oRelacion.Vto_Serie = oAplicacion.Serie ?? "";
                        oRelacion.TipoDeRelacion = 3;   // 3 = Relaciones de COBROS
                        oRelacion.Vto_ItemID = oAplicacion.Vto_ItemID;
                        oRelacion.Numero = oAplicacion.Numero;
                        oRelacion.Importe = oAplicacion.ImporteAplicado;
                        oCobroNuevaAPI.AplicacionesADebitos.Add(oRelacion);
                    }
                }

                #endregion

                #region Movimiento De Valores

                if (oCobroAPI.Valores?.Count > 0)
                {
                    oCobroNuevaAPI.DetalleDeValores = new List<cMovimientoDeValor>();
                    foreach (cValorAPI oValor in oCobroAPI.Valores)
                    {
                        cMovimientoDeValor oMovimientoValor = new cMovimientoDeValor();
                        oMovimientoValor.ValorID = (short)oValor.ValorID;
                        oMovimientoValor.NumeroDeValor = oValor.NumeroDeValor;
                        oMovimientoValor.CajaBancoID = (short)oValor.CajaBancoID;
                        oMovimientoValor.PoC = "C";
                        oMovimientoValor.PoCID = oCobroNuevaAPI.ClienteID;
                        oMovimientoValor.FechaDC = oValor.FechaDC;
                        oMovimientoValor.EmpresaID = (short)Sesion.EmpresaID;
                        oMovimientoValor.Importe = oValor.Importe;
                        oMovimientoValor.Numero = oCobroNuevaAPI.Numero;
                        oCobroNuevaAPI.DetalleDeValores.Add(oMovimientoValor);
                    }
                }

                #endregion

                lstComprobantesCobro.Add(oCobroNuevaAPI);
            }

            return lstComprobantesCobro;
        }


        /// <summary>
        /// Devuelve el archivo PDF en base 64
        /// </summary>
        /// <param name="lstClaves"></param>
        /// <returns></returns>
        public async Task<List<cPDFComprobante>> DevolverJSONPDF(List<GESI.ERP.Core.V3.BO.Dinamicos.dClavesComprobante>? lstClaves, string subTipo)
        {
            List<cPDFComprobante> lstRespuesta = new List<cPDFComprobante>();
            try
            {

                if (lstClaves == null || lstClaves.Count == 0)
                    throw new ApplicationException("No se encontraron claves de comprobante");

                if (lstClaves.Count > 5)
                    throw new ApplicationException("Cantidad de comprobantes superada. El limite son 5");

                // NUEVO respecto de la V2: se fuerza la empresa de la sesión, igual que en GetItem
                // (la V2 buscaba con la EmpresaID del body y el PDF en la carpeta FTP de la empresa de la sesión)
                lstClaves.ForEach(x => x.EmpresaID = Sesion.EmpresaID);

                string? strServidor = configuracionesBLL.GetValorConfiguracionBase("Configuracion", "FTPPDFs", "Server");
                string? strUsuario = configuracionesBLL.GetValorConfiguracionBase("Configuracion", "FTPPDFs", "Usuario");
                string? strPassword = configuracionesBLL.GetValorConfiguracionBase("Configuracion", "FTPPDFs", "Password");
                
                strPassword = GESI.CORE.V3.SEC.Seguridad.DesencriptarTexto(strPassword);

                string? strDominioDePdfs = configuracionesBLL.GetValorConfiguracionBase("Configuracion", "FTPPDFs", "Dominio");

                if (string.IsNullOrEmpty(strServidor) ||
                    string.IsNullOrEmpty(strUsuario) ||
                    string.IsNullOrEmpty(strPassword) ||
                    string.IsNullOrEmpty(strDominioDePdfs))
                    throw new ApplicationException("No se encontraron las configuraciones internas al FTP (Credenciales de acceso)");

                cEmpresa oEmpresa = empresasBLL.GetItem(Sesion.EmpresaID);

                if (string.IsNullOrEmpty(oEmpresa.PDFComprobantesCarpetaDestinoFTP))
                    throw new ApplicationException("No se encontraron las configuraciones internas al FTP (Carpeta FTP)");

                List<GESI.ERP.Ventas.V3.BO.cComprobanteVenta>? lstComprobantesDeVenta = comprobanteDeVentaBLL.GetItemsByIDs(ConvertirAClavesComprobante(lstClaves));

                if (lstComprobantesDeVenta == null)
                    throw new ApplicationException("No se encontraron los comprobantes detallados en la solicitud");


                using var cliente = new AsyncFtpClient(
                                        strServidor,
                                        strUsuario,
                                        strPassword);

                await cliente.Connect();

                foreach (GESI.ERP.Core.V3.BO.Dinamicos.dClavesComprobante oClave in lstClaves)
                {
                    cPDFComprobante oPDFADevolver = new cPDFComprobante();

                    GESI.ERP.Ventas.V3.BO.cComprobanteVenta? oComprobante = lstComprobantesDeVenta.FirstOrDefault(x => x.EmpresaID == oClave.EmpresaID && x.PuntoDeVentaID == oClave.PuntoDeVentaID && x.Serie == oClave.Serie && x.Numero == oClave.Numero && x.ComprobanteID == oClave.ComprobanteID);

                    if (oComprobante == null)
                    {
                        oPDFADevolver.success = false;
                        oPDFADevolver.error = new cError((int)cCodigosError.cDatosNoEncontrados, "No se encontró el comprobante en el ERP, reintente nuevamente", ApiErrors.errDatosNoEncontrados);
                        lstRespuesta.Add(oPDFADevolver);
                        continue;
                    }


                    string prefijo = "";
                    if (oClave.EntidadID <= 0)
                        prefijo = subTipo + "-" + oClave.Serie + "-" + oClave.PuntoDeVentaID.ToString("D4") + "-" + oClave.Numero.ToString("D8");
                    else
                        prefijo = subTipo + "-" + oClave.Serie + "-" + oClave.PuntoDeVentaID.ToString("D4") + "-" + oClave.Numero.ToString("D8") + "-" + oClave.EntidadID.ToString("D6");


                    string directorio = "/" + oEmpresa.PDFComprobantesCarpetaDestinoFTP;

                    cPDFComprobante? oPDFDevuelto = await DevolverPDFComprobante(directorio, cliente, prefijo, strDominioDePdfs, oEmpresa.PDFComprobantesCarpetaDestinoFTP);

                    if (oPDFDevuelto != null)
                    {
                        lstRespuesta.Add(oPDFDevuelto);
                        continue;
                    }

                    if (oClave.EntidadID <= 0)
                        prefijo = subTipo + "-CR-" + oClave.Serie + "-" + oClave.PuntoDeVentaID.ToString("D4") + "-" + oClave.Numero.ToString("D8");
                    else
                        prefijo = subTipo + "-CR-" + oClave.Serie + "-" + oClave.PuntoDeVentaID.ToString("D4") + "-" + oClave.Numero.ToString("D8") + "-" + oClave.EntidadID.ToString("D6");

                    oPDFDevuelto = await DevolverPDFComprobante(directorio, cliente, prefijo, strDominioDePdfs, oEmpresa.PDFComprobantesCarpetaDestinoFTP);

                    if (oPDFDevuelto != null)
                    {
                        lstRespuesta.Add(oPDFDevuelto);
                        continue;
                    }
                    else
                    {
                        oPDFADevolver.success = false;
                        oPDFADevolver.error = new cError((int)cCodigosError.cDatosNoEncontrados, "No se encontró el PDF en el servidor, reintente nuevamente", ApiErrors.errDatosNoEncontrados);
                        lstRespuesta.Add(oPDFADevolver);
                    }


                }
            }
            catch (ApplicationException app)
            {
                cPDFComprobante oPdf = new cPDFComprobante();
                oPdf.error = new cError(400, app.Message, ApiErrors.errInternalServerError);

                lstRespuesta.Add(oPdf);
            }
            catch (Exception ex)
            {
                cPDFComprobante oPdf = new cPDFComprobante();
                oPdf.error = new cError(500, ex.Message, ApiErrors.errInternalServerError);

                lstRespuesta.Add(oPdf);
            }

            return lstRespuesta;
        }

        /// <summary>
        /// Busca el pdf dentro del FTP
        /// </summary>
        /// <param name="directorio"></param>
        /// <param name="cliente"></param>
        /// <param name="prefijo"></param>
        /// <param name="strDominioDePdfs"></param>
        /// <param name="CarpetaDestinoPDF"></param>
        /// <returns></returns>
        private async Task<cPDFComprobante?> DevolverPDFComprobante(string directorio,
                                                                    AsyncFtpClient cliente,
                                                                    string prefijo,
                                                                    string strDominioDePdfs,
                                                                    string CarpetaDestinoPDF)
        {
            cPDFComprobante? oPDFADevolver = null;

            FtpListItem[] archivos = await cliente.GetListing(directorio);
            var archivo = archivos.FirstOrDefault(x =>
                            x.Type == FtpObjectType.File &&
                            x.Name.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase) &&
                            x.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));

            if (archivo != null)
            {
                oPDFADevolver = new cPDFComprobante();
                oPDFADevolver.success = true;
                oPDFADevolver.url = strDominioDePdfs + "/" + CarpetaDestinoPDF + "/" + archivo.Name;
            }

            return oPDFADevolver;
        }


        private List<GESI.ERP.Ventas.V3.BO.dClaveComprobanteDeVenta> ConvertirAClavesComprobante(List<GESI.ERP.Core.V3.BO.Dinamicos.dClavesComprobante> lstClaves)
        {
            List<GESI.ERP.Ventas.V3.BO.dClaveComprobanteDeVenta> lstClavesADevolver = new List<dClaveComprobanteDeVenta>();

            foreach (GESI.ERP.Core.V3.BO.Dinamicos.dClavesComprobante oClave in lstClaves)
            {
                GESI.ERP.Ventas.V3.BO.dClaveComprobanteDeVenta oClaveADevolver = new dClaveComprobanteDeVenta();
                oClaveADevolver.EmpresaID = oClave.EmpresaID;
                oClaveADevolver.PuntoDeVentaID = oClave.PuntoDeVentaID;
                oClaveADevolver.ComprobanteID = oClave.ComprobanteID;
                oClaveADevolver.Serie = oClave.Serie;
                oClaveADevolver.Numero = oClave.Numero;
                lstClavesADevolver.Add(oClaveADevolver);
            }

            return lstClavesADevolver;
        }

        #endregion

        #region Auxiliares

        /// <summary>
        /// Convierte un cMovimientoDeCliente (lo que recibe la API) en el cComprobanteVenta que graba el Core.
        /// Copia los mismos campos que la V2. También lo usa RepartosMgr (GetItem de la foja), igual que en la V2.
        /// </summary>
        internal static List<cComprobanteVenta> ConvertirMovimientoAComprobanteDeVenta(List<cMovimientoDeCliente> lstMovimiento)
        {
            List<cComprobanteVenta> lstComprobantes = new List<cComprobanteVenta>();

            foreach (cMovimientoDeCliente oMovimiento in lstMovimiento)
            {
                #region Encabezado

                cComprobanteVenta oComprobanteVenta = new cComprobanteVenta();
                oComprobanteVenta.EmpresaID = oMovimiento.EmpresaID;
                oComprobanteVenta.ComprobanteID = oMovimiento.ComprobanteID;
                oComprobanteVenta.Serie = oMovimiento.Serie;
                oComprobanteVenta.PuntoDeVentaID = oMovimiento.PuntoDeVentaID;
                oComprobanteVenta.Numero = oMovimiento.Numero;
                oComprobanteVenta.Subtipo = oMovimiento.Subtipo;
                oComprobanteVenta.FechaDeEmision = oMovimiento.FechaDeEmision;
                oComprobanteVenta.FechaDePrimerVencimiento = oMovimiento.FechaDePrimerVencimiento;
                oComprobanteVenta.ClienteID = oMovimiento.ClienteID;
                oComprobanteVenta.AgrupacionID = oMovimiento.AgrupacionID;
                oComprobanteVenta.ImporteNeto = oMovimiento.ImporteNeto;
                oComprobanteVenta.ImporteExento = oMovimiento.ImporteExento;
                oComprobanteVenta.ImporteNoGravado = oMovimiento.ImporteNoGravado;
                oComprobanteVenta.ImporteDescuento = oMovimiento.ImporteDescuento;
                oComprobanteVenta.ImporteIVAGeneral = oMovimiento.ImporteIVAGeneral;
                oComprobanteVenta.ImporteIVAAdicional = oMovimiento.ImporteIVAAdicional;
                oComprobanteVenta.ImporteImpuestos = oMovimiento.ImporteImpuestos;
                oComprobanteVenta.ImporteTotal = oMovimiento.ImporteTotal;
                oComprobanteVenta.DescuentoImporte = oMovimiento.DescuentoImporte;
                oComprobanteVenta.DescuentoPorcentaje = oMovimiento.DescuentoPorcentaje;
                oComprobanteVenta.Redondeo = oMovimiento.Redondeo;
                oComprobanteVenta.SaldoPendiente = oMovimiento.SaldoPendiente;
                oComprobanteVenta.Notas = oMovimiento.Notas;
                oComprobanteVenta.Notas2 = oMovimiento.Notas2;
                oComprobanteVenta.RazonSocial = oMovimiento.RazonSocial;
                oComprobanteVenta.TipoDeDocumentoID = oMovimiento.TipoDeDocumentoID;
                oComprobanteVenta.NumeroDeDocumento = oMovimiento.NumeroDeDocumento;
                oComprobanteVenta.DivisaID = oMovimiento.DivisaID;
                oComprobanteVenta.Cotizacion = oMovimiento.Cotizacion;
                oComprobanteVenta.DetalleDeImpuestos = oMovimiento.DetalleDeImpuestos;
                oComprobanteVenta.DetalleDeItems = oMovimiento.DetalleDeItems;
                oComprobanteVenta.Anulado = oMovimiento.Anulado;
                oComprobanteVenta.EsUnAcopio = oMovimiento.EsUnAcopio;
                oComprobanteVenta.VendedorID = oMovimiento.VendedorID;
                oComprobanteVenta.RepartidorID = oMovimiento.RepartidorID;
                oComprobanteVenta.FormaDePagoID = oMovimiento.FormaDePagoID;
                oComprobanteVenta.CanalDeVentaID = oMovimiento.CanalDeVentaID;
                oComprobanteVenta.SucursalClienteID = oMovimiento.SucursalClienteID;
                oComprobanteVenta.TipoDeOperacionID = oMovimiento.TipoDeOperacionID;
                oComprobanteVenta.AlmacenID = oMovimiento.AlmacenID;
                oComprobanteVenta.CodigoDeImportacion = oMovimiento.CodigoDeImportacion;
                oComprobanteVenta.AcopioComprometeStock = oMovimiento.AcopioComprometeStock;
                oComprobanteVenta.AcopioFechaCaducidad = oMovimiento.AcopioFechaCaducidad;
                oComprobanteVenta.Domicilio = oMovimiento.Domicilio;
                oComprobanteVenta.DomicilioEntrega = oMovimiento.DomicilioEntrega;
                oComprobanteVenta.Telefono = oMovimiento.Telefono;
                oComprobanteVenta.Email = oMovimiento.Email;
                oComprobanteVenta.Localidad = oMovimiento.Localidad;
                oComprobanteVenta.Referencia = oMovimiento.Referencia;
                oComprobanteVenta.Hora = oMovimiento.Hora;

                // NUEVO respecto de la V2: la V2 no copiaba estos tres campos y se perdían al grabar
                // (el conversor de la API vieja los carga desde FechaEntrega, FormaIngreso y DescuentoTexto)
                oComprobanteVenta.FechaDeEntrega = oMovimiento.FechaDeEntrega;
                oComprobanteVenta.CanalDeAtencionID = oMovimiento.CanalDeAtencionID;
                oComprobanteVenta.DescuentoTexto = oMovimiento.DescuentoTexto;

                #endregion

                #region Detalle
                // Si vino el detalle en formato API (DetalleItems) reemplaza al DetalleDeItems copiado arriba

                if (oMovimiento.DetalleItems?.Count > 0)
                {
                    oComprobanteVenta.DetalleDeItems = new List<cItemComprobanteDeVenta>();

                    foreach (cDetalleDeVenta oDetalle in oMovimiento.DetalleItems)
                    {
                        cItemComprobanteDeVenta oItem = new cItemComprobanteDeVenta();
                        oItem.EmpresaID = oComprobanteVenta.EmpresaID;
                        oItem.ComprobanteID = oComprobanteVenta.ComprobanteID;
                        oItem.Serie = oComprobanteVenta.Serie;
                        oItem.PuntoDeVentaID = oComprobanteVenta.PuntoDeVentaID;
                        oItem.Numero = oComprobanteVenta.Numero;
                        oItem.TipoDeItem = oDetalle.TipoDeItem;
                        oItem.CodigoDeItem = oDetalle.CodigoDeItem;
                        oItem.CantidadU1 = oDetalle.CantidadU1;
                        oItem.CantidadU2 = oDetalle.CantidadU2;
                        oItem.CantidadU3 = oDetalle.CantidadU3;
                        oItem.TipoDeFacturacion = oDetalle.TipoDeFacturacion;
                        oItem.PrecioUnitarioNeto = oDetalle.PrecioUnitarioNeto;
                        oItem.PrecioUnitarioFinal = oDetalle.PrecioUnitarioFinal;
                        oItem.DescuentosPorcentaje = oDetalle.DescuentosPorcentaje;
                        oItem.DescuentoImporteNeto = oDetalle.DescuentoImporteNeto;
                        oItem.DescuentoImporteFinal = oDetalle.DescuentoImporteFinal;
                        oItem.ImporteTotalNeto = oDetalle.ImporteTotalNeto;
                        oItem.ImporteTotalFinal = oDetalle.ImporteTotalFinal;
                        oItem.ItemID = oDetalle.ItemID;
                        oItem.AlicuotaID = oDetalle.AlicuotaID;
                        oItem.AlicuotaIVA = oDetalle.AlicuotaIVA;
                        oItem.Comentario = oDetalle.Comentario;
                        oItem.ComponenteDeItem = oDetalle.ComponenteDeItem;
                        oItem.SaldoFactura_U1 = oDetalle.SaldoFactura_U1;
                        oItem.SaldoFactura_U2 = oDetalle.SaldoFactura_U2;
                        oItem.SaldoEntrega_U1 = oDetalle.SaldoEntrega_U1;
                        oItem.SaldoEntrega_U2 = oDetalle.SaldoEntrega_U2;
                        oItem.AgenteID = oDetalle.AgenteID;
                        oItem.LoteID = oDetalle.LoteID;
                        oItem.PromocionID = oDetalle.PromocionID;
                        oItem.DescuentoAutorizado = oDetalle.DescuentoAutorizado;
                        oItem.AutorizadoVenderSinExistencias = oDetalle.AutorizadoVenderSinExistencias;
                        oItem.EsNoGravado = oDetalle.EsNoGravado;
                        oItem.YaEstaba = oDetalle.YaEstaba;
                        oItem.CodigoDeItemQueYaEstaba = oDetalle.CodigoDeItemQueYaEstaba;
                        oItem.AlicuotaIDQueYaEstaba = oDetalle.AlicuotaIDQueYaEstaba;
                        oItem.CampoExtra = oDetalle.CampoExtra;
                        oItem.CampoExtra2 = oDetalle.CampoExtra2;
                        oComprobanteVenta.DetalleDeItems.Add(oItem);
                    }
                }

                #endregion

                lstComprobantes.Add(oComprobanteVenta);
            }

            return lstComprobantes;
        }

        /// <summary>
        /// Arma el resultado de un comprobante de venta para la respuesta del Save
        /// </summary>
        private static cResponseSaveMovimientosClientes ConvertirComprobanteDeVentaASaveMovimientos(cComprobanteVenta oComprobanteDeVenta,
                                                                                                    bool success,
                                                                                                    string mensaje = "")
        {
            cResponseSaveMovimientosClientes oRespuestaAAcumular = new cResponseSaveMovimientosClientes();
            oRespuestaAAcumular.success = success;
            oRespuestaAAcumular.numero = oComprobanteDeVenta.Numero;
            oRespuestaAAcumular.comprobanteID = oComprobanteDeVenta.ComprobanteID;
            oRespuestaAAcumular.puntoDeVentaID = oComprobanteDeVenta.PuntoDeVentaID;
            oRespuestaAAcumular.notas = oComprobanteDeVenta.Notas;
            oRespuestaAAcumular.serie = oComprobanteDeVenta.Serie;
            oRespuestaAAcumular.subTipo = oComprobanteDeVenta.Subtipo;
            oRespuestaAAcumular.clienteID = oComprobanteDeVenta.ClienteID;
            oRespuestaAAcumular.razonSocial = oComprobanteDeVenta.RazonSocial;
            oRespuestaAAcumular.notas2 = oComprobanteDeVenta.Notas2;
            oRespuestaAAcumular.codigoDeImportacion = QuitarPrefijoUsuario(oComprobanteDeVenta.CodigoDeImportacion);

            if (!success)
                oRespuestaAAcumular.mensaje = mensaje;

            return oRespuestaAAcumular;
        }

        /// <summary>
        /// Arma el resultado de un comprobante de cobro para la respuesta del Save
        /// </summary>
        private static cResponseSaveMovimientosClientes ConvertirComprobanteDeCobroASaveMovimientos(cComprobanteCobro oComprobanteDeCobro,
                                                                                                    bool success,
                                                                                                    string mensaje = "")
        {
            cResponseSaveMovimientosClientes oRespuestaAAcumular = new cResponseSaveMovimientosClientes();
            oRespuestaAAcumular.success = success;
            oRespuestaAAcumular.numero = oComprobanteDeCobro.Numero;
            oRespuestaAAcumular.comprobanteID = oComprobanteDeCobro.ComprobanteID;
            oRespuestaAAcumular.puntoDeVentaID = oComprobanteDeCobro.PuntoDeVentaID;
            oRespuestaAAcumular.notas = oComprobanteDeCobro.Notas;
            oRespuestaAAcumular.serie = oComprobanteDeCobro.Serie;
            oRespuestaAAcumular.subTipo = oComprobanteDeCobro.Subtipo;
            oRespuestaAAcumular.clienteID = oComprobanteDeCobro.ClienteID;
            oRespuestaAAcumular.razonSocial = oComprobanteDeCobro.RazonSocial;

            // Igual que la V2 (a diferencia de ventas, un código vacío se devuelve "" y no null)
            if (oComprobanteDeCobro.CodigoDeImportacion?.IndexOf("_") != -1)
                oRespuestaAAcumular.codigoDeImportacion = oComprobanteDeCobro.CodigoDeImportacion?.Split('_').Last();
            else
                oRespuestaAAcumular.codigoDeImportacion = oComprobanteDeCobro.CodigoDeImportacion;

            if (!success)
                oRespuestaAAcumular.mensaje = mensaje;

            return oRespuestaAAcumular;
        }

        /// <summary>
        /// El código de importación de la API vieja se graba como "usuario_código": al cliente se le devuelve solo su código.
        /// Igual que la V2 en ventas: un código vacío se devuelve null.
        /// </summary>
        private static string? QuitarPrefijoUsuario(string? strCodigoDeImportacion)
        {
            if (strCodigoDeImportacion == null || strCodigoDeImportacion.Length == 0)
                return null;

            if (strCodigoDeImportacion.IndexOf("_") != -1)
                return strCodigoDeImportacion.Split('_').Last();

            return strCodigoDeImportacion;
        }

        /// <summary>
        /// Igual que la V2: junta en una sola fila los resultados del mismo comprobante (si tuvo más de un error, une los mensajes con " | ")
        /// </summary>
        private static List<cResponseSaveMovimientosClientes> AgruparRespuestas(List<cResponseSaveMovimientosClientes> lstRespuestaSave)
        {
            List<cResponseSaveMovimientosClientes> lstAgrupada = new List<cResponseSaveMovimientosClientes>();

            foreach (var grupo in lstRespuestaSave.GroupBy(x => new { x.puntoDeVentaID, x.serie, x.comprobanteID, x.codigoDeImportacion }))
            {
                cResponseSaveMovimientosClientes oPrimero = grupo.First();

                cResponseSaveMovimientosClientes oRespuesta = new cResponseSaveMovimientosClientes();
                oRespuesta.puntoDeVentaID = grupo.Key.puntoDeVentaID;
                oRespuesta.serie = grupo.Key.serie;
                oRespuesta.comprobanteID = grupo.Key.comprobanteID;
                oRespuesta.codigoDeImportacion = grupo.Key.codigoDeImportacion;
                oRespuesta.mensaje = string.Join(" | ", grupo.Where(x => !string.IsNullOrWhiteSpace(x.mensaje)).Select(x => x.mensaje));
                oRespuesta.success = grupo.All(x => x.success);
                oRespuesta.notas = oPrimero.notas;
                oRespuesta.notas2 = oPrimero.notas2;
                oRespuesta.numero = oPrimero.numero;
                oRespuesta.subTipo = oPrimero.subTipo;
                oRespuesta.clienteID = oPrimero.clienteID;
                oRespuesta.razonSocial = oPrimero.razonSocial;
                lstAgrupada.Add(oRespuesta);
            }

            return lstAgrupada;
        }

        /// <summary>
        /// Log de cada comprobante grabado o rechazado (igual que la V2)
        /// </summary>
        private void LoguearComprobante(string mensaje, string tipo, string endpoint, Stopwatch stopwatch)
        {
            TimeSpan ts = stopwatch.Elapsed;
            Logger.LoguearSucesosAPI(mensaje, tipo, Sesion.Usuario, endpoint, Sesion.EmpresaID, Sesion.SucursalID,
                                     TiempoEjecucion: $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");
        }

        /// <summary>
        /// Completa FojaID en cada comprobante que esté asignado a una foja de reparto (igual que la V2; los recibos no tienen foja)
        /// </summary>
        private void CargarFojasDeReparto(List<cMovimientoDeCliente> lstComprobantes)
        {
            List<dClavesComprobante> lstClaves = new List<dClavesComprobante>();

            foreach (cMovimientoDeCliente oMovimiento in lstComprobantes)
            {
                if (oMovimiento.Subtipo != SubTiposComprobantes.sRecibo)
                {
                    dClavesComprobante oClave = new dClavesComprobante();
                    oClave.EmpresaID = oMovimiento.EmpresaID;
                    oClave.ComprobanteID = oMovimiento.ComprobanteID;
                    oClave.Serie = oMovimiento.Serie;
                    oClave.PuntoDeVentaID = oMovimiento.PuntoDeVentaID;
                    oClave.Numero = oMovimiento.Numero;
                    lstClaves.Add(oClave);
                }
            }

            if (lstClaves.Count == 0)
                return;

            List<dClavesComprobante>? lstClavesConFoja = fojaDeRepartoBLL.GetFojaIDByClavesComprobante(lstClaves);

            if (lstClavesConFoja == null || lstClavesConFoja.Count == 0)
                return;

            lstClavesConFoja = lstClavesConFoja.Where(x => x.EntidadID > 0).ToList();

            foreach (cMovimientoDeCliente oMovimiento in lstComprobantes)
            {
                dClavesComprobante? oClaveAux = lstClavesConFoja.FirstOrDefault(x => x.EmpresaID == oMovimiento.EmpresaID &&
                                                                                    x.ComprobanteID == oMovimiento.ComprobanteID &&
                                                                                    x.PuntoDeVentaID == oMovimiento.PuntoDeVentaID &&
                                                                                    x.Serie == oMovimiento.Serie &&
                                                                                    x.Numero == oMovimiento.Numero);
                if (oClaveAux != null)
                    oMovimiento.FojaID = oClaveAux.EntidadID;
            }
        }

        /// <summary>
        /// Endpoint para el log según el subtipo (la V2 hacía este switch solo en GetItem; el GetList siempre logueaba "Facturas")
        /// </summary>
        private static string DevolverEndpoint(string? subTipo, string accion)
        {
            switch (subTipo)
            {
                case SubTiposComprobantes.sFactura: return Endpoint.Ventas.Comprobantes.Facturas(accion);
                case SubTiposComprobantes.sNPE: return Endpoint.Ventas.Comprobantes.Pedidos(accion);
                case SubTiposComprobantes.sNCR: return Endpoint.Ventas.Comprobantes.NotasDeCredito(accion);
                case SubTiposComprobantes.sPresupuesto: return Endpoint.Ventas.Comprobantes.Presupuestos(accion);
                case SubTiposComprobantes.sDEV: return Endpoint.Ventas.Comprobantes.Devolucion(accion);
                case SubTiposComprobantes.sNDB: return Endpoint.Ventas.Comprobantes.NotasDeDebito(accion);
                case SubTiposComprobantes.sRecibo: return Endpoint.Ventas.Comprobantes.Cobros(accion);
                case SubTiposComprobantes.sRemito: return Endpoint.Ventas.Comprobantes.Remitos(accion);
                default: return Endpoint.Ventas.Comprobantes.Pendientes();   // varios subtipos juntos (GetComprobantesPendientes)
            }
        }

        #endregion
    }
}

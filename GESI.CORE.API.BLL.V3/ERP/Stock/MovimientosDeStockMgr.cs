using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Stock;
using GESI.CORE.V3.BLL;
using GESI.ERP.Core.V3.BO.Dinamicos;
using GESI.ERP.Stock.V3.BLL;
using GESI.ERP.Stock.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Stock
{
    public interface IMovimientosDeStockServicio
    {
        cResponseMovimientoDeStock GetList(int comprobanteID = 0,
                                           int puntoDeVentaID = 0,
                                           int almacenID = 0,
                                           DateTime? fechaDesde = null,
                                           DateTime? fechaHasta = null,
                                           int sucursalID = 0,
                                           int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                           int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                           int claseDeComprobanteID = 0);

        cResponseMovimientoDeStock GetItem(List<dClavesComprobante>? lstClaves,
                                           int claseDeComprobanteID = 0,
                                           int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                           int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber);

        cResponseMovimientoDeStock Save(cMovimientoDeStock? oMovimientoStock, int claseDeComprobanteID);
    }

    /// <summary>
    /// Migrado de la V2 (Stock.BLL.cMovimientosDeStockMgr estático con _MIAPISessionMgr estático).
    /// Lo usan los controllers de comprobantes de stock; cada uno le pasa su clase de comprobante.
    /// </summary>
    internal sealed class MovimientosDeStockServicio(
        IMovimientosDeStockBLL movimientosDeStockBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IMovimientosDeStockServicio
    {
        /// <summary>
        /// Devuelve los comprobantes de stock que cumplen los filtros
        /// </summary>
        public cResponseMovimientoDeStock GetList(int comprobanteID = 0,
                                                  int puntoDeVentaID = 0,
                                                  int almacenID = 0,
                                                  DateTime? fechaDesde = null,
                                                  DateTime? fechaHasta = null,
                                                  int sucursalID = 0,
                                                  int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                  int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                  int claseDeComprobanteID = 0)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseMovimientoDeStock oRespuesta = new cResponseMovimientoDeStock();

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            string strEndpoint = DevolverEndpoint(claseDeComprobanteID, "GetList");

            try
            {
                List<cMovimientoDeStock>? lstMovimientosDeStock = movimientosDeStockBLL.GetList(puntoDeVentaID,
                                                                                                comprobanteID,
                                                                                                almacenID,
                                                                                                fechaDesde,
                                                                                                fechaHasta,
                                                                                                sucursalID,
                                                                                                claseDeComprobanteID);

                if (lstMovimientosDeStock == null || lstMovimientosDeStock.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron comprobantes de stock");

                int intCantidadElementos = lstMovimientosDeStock.Count;

                List<cMovimientoDeStock> nuevosplit = lstMovimientosDeStock.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                // NUEVO respecto de la V2: página fuera de rango responde "Paginación Excedida" como el resto de la API
                // (la V2 devolvía success = false sin ningún error ni paginación)
                if (nuevosplit.Count == 0)
                    throw new cIntervalException(new cPaginacion(intCantidadElementos, (int)Math.Ceiling((double)intCantidadElementos / pageSize), pageNumber, pageSize, pageNumber));

                oRespuesta.success = true;
                oRespuesta.movimientosDeStock = nuevosplit;
                oRespuesta.paginacion = new cPaginacion(intCantidadElementos, (int)Math.Ceiling((double)intCantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success MovimientosDeStock GetList ClaseDeComprobante:" + claseDeComprobanteID,
                            strEndpoint, stopwatch, intCantidadElementos, pageNumber, pageSize);

                return oRespuesta;
            }
            catch (cIntervalException cIntervalo)
            {
                oRespuesta.paginacion = cIntervalo._paginacion;
                oRespuesta.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);
                oRespuesta.success = true;
                return oRespuesta;
            }
            catch (cDatosNoEncontradosException mensaje)
            {
                LoguearInfo("Not Found MovimientosDeStock GetList", strEndpoint, stopwatch, 0, pageNumber, pageSize);

                // Igual que la V2: 200 con success = true y el error adentro
                oRespuesta.error = new cError((int)cCodigosError.cDatosNoEncontrados, mensaje.Message, ApiErrors.errDatosNoEncontrados);
                oRespuesta.success = true;
                return oRespuesta;
            }
        }

        /// <summary>
        /// Devuelve los comprobantes de stock pedidos por clave
        /// </summary>
        public cResponseMovimientoDeStock GetItem(List<dClavesComprobante>? lstClaves,
                                                  int claseDeComprobanteID = 0,
                                                  int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                  int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseMovimientoDeStock oRespuesta = new cResponseMovimientoDeStock();

            // NUEVO respecto de la V2: sin claves devuelve 400 (la V2 devolvía 200 con el error adentro)
            if (lstClaves == null || lstClaves.Count == 0)
                throw new cCodigoNoDetalladoException("No se adjuntaron claves de comprobantes");   // → 400

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            string strEndpoint = DevolverEndpoint(claseDeComprobanteID, "GetItem");

            try
            {
                // Igual que la V2: todas las claves se buscan en la empresa de la sesión
                lstClaves.ForEach(x => x.EmpresaID = Sesion.EmpresaID);

                List<cMovimientoDeStock>? lstMovimientos = movimientosDeStockBLL.GetItem(lstClaves, claseDeComprobanteID);

                if (lstMovimientos == null || lstMovimientos.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron movimientos de stock");

                int intCantidadElementos = lstMovimientos.Count;

                List<cMovimientoDeStock> nuevosplit = lstMovimientos.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                oRespuesta.paginacion = new cPaginacion(intCantidadElementos, (int)Math.Ceiling((double)intCantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                // NUEVO respecto de la V2: página fuera de rango informa "Paginación Excedida" (igual que el GetItem de Ventas);
                // la V2 devolvía success = false sin error ni paginación
                if (nuevosplit.Count == 0)
                {
                    oRespuesta.error = new cError((int)cCodigosError.cPaginacionExcedida, "Intervalo de Paginacion Excedido", ApiErrors.errPaginacionExcedida);
                    return oRespuesta;
                }

                oRespuesta.success = true;
                oRespuesta.movimientosDeStock = nuevosplit;

                // NUEVO respecto de la V2: el log usa el endpoint de la clase de comprobante (la V2 logueaba todo como "Ingresos")
                LoguearInfo("Success MovimientosDeStock GetItem ClaseDeComprobante:" + claseDeComprobanteID,
                            strEndpoint, stopwatch, intCantidadElementos, pageNumber, pageSize);

                return oRespuesta;
            }
            catch (cDatosNoEncontradosException cdatex)
            {
                // Igual que la V2: 200 con success = false y el error adentro
                oRespuesta.error = new cError((int)cCodigosError.cDatosNoEncontrados, cdatex.Message, ApiErrors.errDatosNoEncontrados);
                return oRespuesta;
            }
        }

        /// <summary>
        /// Graba un comprobante de stock. Si el Core lo rechaza, devuelve success = false con los motivos en el error
        /// (el controller decide el status, igual que la V2).
        /// </summary>
        public cResponseMovimientoDeStock Save(cMovimientoDeStock? oMovimientoStock, int claseDeComprobanteID)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseMovimientoDeStock oRespuesta = new cResponseMovimientoDeStock();

            // NUEVO respecto de la V2: sin comprobante devuelve 400 (la V2 fallaba con error 500)
            if (oMovimientoStock == null)
                throw new cCodigoNoDetalladoException("No se detalló el comprobante de stock a grabar");   // → 400

            // NUEVO respecto de la V2: se fuerza la empresa de la sesión (la V2 grababa la EmpresaID que viniera en el body)
            oMovimientoStock.EmpresaID = (short)Sesion.EmpresaID;

            (cMovimientoDeStock? oMovimientoStockGrabado, Dictionary<string, (string, string)>? dicc) = movimientosDeStockBLL.Save(oMovimientoStock, claseDeComprobanteID);

            if (oMovimientoStockGrabado != null)
            {
                oRespuesta.movimientosDeStock = new List<cMovimientoDeStock>();
                oRespuesta.movimientosDeStock.Add(oMovimientoStockGrabado);
                oRespuesta.success = true;
                oRespuesta.paginacion = new cPaginacion();
                oRespuesta.paginacion.PaginaSolicitada = 1;
                oRespuesta.paginacion.tamañoPagina = 1;
                oRespuesta.paginacion.totalPaginas = 1;
                oRespuesta.paginacion.paginaActual = 1;
            }
            else
            {
                if (dicc?.Count > 0)
                {
                    string strErrores = "";
                    foreach (KeyValuePair<string, (string, string)> oErrores in dicc)
                    {
                        strErrores = strErrores + " | " + oErrores.Value.Item1;
                    }
                    oRespuesta.error = new cError((int)cCodigosError.cErrorInternoAplicacion, strErrores, ApiErrors.errInternalServerError);
                }
            }

            LoguearInfo("MovimientosDeStock Create ClaseDeComprobante:" + claseDeComprobanteID + " Resultado:" + oRespuesta.success,
                        DevolverEndpoint(claseDeComprobanteID, "Create"), stopwatch, 1, 1, 1);

            return oRespuesta;
        }

        #region Auxiliares

        /// <summary>
        /// Endpoint para el log según la clase de comprobante
        /// </summary>
        private static string DevolverEndpoint(int claseDeComprobanteID, string accion)
        {
            switch (claseDeComprobanteID)
            {
                case (int)ClaseDeComprobante.Productos_Ajustes: return Endpoint.Stock.Ajustes(accion);
                case (int)ClaseDeComprobante.Productos_EgresoDeStock: return Endpoint.Stock.Egresos(accion);
                case (int)ClaseDeComprobante.Productos_TransferenciaDeStock: return Endpoint.Stock.Transferencias(accion);
                case (int)ClaseDeComprobante.Productos_TomaDeInventario: return Endpoint.Stock.TomaDeInventario(accion);
                default: return Endpoint.Stock.Ingresos(accion);
            }
        }

        #endregion
    }
}

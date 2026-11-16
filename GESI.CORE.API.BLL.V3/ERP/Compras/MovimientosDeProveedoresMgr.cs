using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Compras;
using GESI.CORE.V3.BLL;
using GESI.ERP.Compras.V3.BLL;
using GESI.ERP.Compras.V3.BO;
using GESI.ERP.Core.V3.BO.Dinamicos;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Compras
{
    public interface IMovimientosDeProveedoresServicio
    {
        cResponseMovimientoProveedores GetList(dParametrosConsultaComprobantesCompra? parametros,
                                               int claseDeComprobanteID,
                                               int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                               int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize);

        cResponseMovimientoProveedores GetItem(List<dClavesComprobante>? lstClaves,
                                               int claseDeComprobanteID,
                                               bool incluirDetalle = false,
                                               bool incluirRelacionesItemsCompras = false,
                                               bool incluirPagosProveedores = false,
                                               bool incluirMediosDePago = false,
                                               bool incluirVencimientos = false,
                                               int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                               int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize);

        List<cResponseSaveMovimientosProveedores> Create(List<cMovimientoProveedor>? lstMovimientosProveedores);
    }

    /// <summary>
    /// Migrado de la V2 (Compras.BLL.MovimientosDeProveedoresMgr estático con _MIAPISessionMgr estático).
    /// Lo usan todos los controllers de comprobantes de compra; cada uno le pasa su clase de comprobante.
    /// </summary>
    internal sealed class MovimientosDeProveedoresServicio(
        IMovimientosProveedoresBLL movimientosProveedoresBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IMovimientosDeProveedoresServicio
    {
        // Bloqueo compartido por todos los requests del proceso (el servicio es Scoped, por eso es static)
        private static readonly object _lockOrdenesDeCompra = new object();

        /// <summary>
        /// Devuelve los comprobantes de compra que cumplen los filtros
        /// </summary>
        public cResponseMovimientoProveedores GetList(dParametrosConsultaComprobantesCompra? parametros,
                                                      int claseDeComprobanteID,
                                                      int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                      int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseMovimientoProveedores oRespuesta = new cResponseMovimientoProveedores();

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            // Igual que la V2: sin body se buscan los comprobantes registrados desde hoy
            if (parametros == null)
            {
                parametros = new dParametrosConsultaComprobantesCompra();
                parametros.datosOperacion = new dParametroConsultaComprobantesDatosOperacion();
                parametros.datosOperacion.FechaRegistracionDesde = DateTime.Now;
            }

            if (parametros.datosOperacion == null)
                parametros.datosOperacion = new dParametroConsultaComprobantesDatosOperacion();

            if (parametros.datosProveedores == null)
                parametros.datosProveedores = new dParametroConsultaComprobantesDatosProveedores();

            // La clase de comprobante la decide el controller, no el body
            parametros.datosOperacion.ClaseDeComprobanteID = claseDeComprobanteID;

            string strEndpoint = DevolverEndpoint(claseDeComprobanteID, "GetList");

            try
            {
                List<cMovimientoProveedor>? lstMovimientosProveedores = movimientosProveedoresBLL.GetList(parametros.datosOperacion, parametros.datosProveedores);

                if (lstMovimientosProveedores == null || lstMovimientosProveedores.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron comprobantes");

                int intCantidadElementos = lstMovimientosProveedores.Count;

                List<cMovimientoProveedor> lstMovimientosAux = lstMovimientosProveedores.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstMovimientosAux.Count == 0)
                    throw new cIntervalException(new cPaginacion(intCantidadElementos, (int)Math.Ceiling((double)intCantidadElementos / pageSize), pageNumber, pageSize, pageNumber));

                oRespuesta.success = true;
                oRespuesta.paginacion = new cPaginacion(intCantidadElementos, (int)Math.Ceiling((double)intCantidadElementos / pageSize), pageNumber, pageSize, pageNumber);
                oRespuesta.movimientosDeProveedores = lstMovimientosAux;

                LoguearInfo("Success MovimientosProveedores GetList ClaseDeComprobante:" + claseDeComprobanteID,
                            strEndpoint, stopwatch, intCantidadElementos, pageNumber, pageSize);

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
        /// Devuelve los comprobantes pedidos por clave, con el detalle que se pida.
        /// Igual que la V2: devuelve todos los comprobantes encontrados (no pagina); la paginación solo informa los totales.
        /// </summary>
        public cResponseMovimientoProveedores GetItem(List<dClavesComprobante>? lstClaves,
                                                      int claseDeComprobanteID,
                                                      bool incluirDetalle = false,
                                                      bool incluirRelacionesItemsCompras = false,
                                                      bool incluirPagosProveedores = false,
                                                      bool incluirMediosDePago = false,
                                                      bool incluirVencimientos = false,
                                                      int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                      int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseMovimientoProveedores oRespuesta = new cResponseMovimientoProveedores();

            // NUEVO respecto de la V2: sin claves devuelve 400 (la V2 devolvía 200 con el error adentro)
            if (lstClaves == null || lstClaves.Count == 0)
                throw new cCodigoNoDetalladoException("No se detallaron las claves");   // → 400

            string strEndpoint = DevolverEndpoint(claseDeComprobanteID, "GetItem");

            try
            {
                // Igual que la V2: todas las claves se buscan en la empresa de la sesión
                lstClaves.ForEach(x => x.EmpresaID = Sesion.EmpresaID);

                List<cMovimientoProveedor>? lstMovimientosAux = movimientosProveedoresBLL.GetItem(lstClaves,
                                                                                                  incluirDetalle ? 1 : 0,
                                                                                                  incluirRelacionesItemsCompras ? 1 : 0,
                                                                                                  incluirPagosProveedores ? 1 : 0,
                                                                                                  incluirMediosDePago ? 1 : 0,
                                                                                                  0,
                                                                                                  claseDeComprobanteID,
                                                                                                  incluirVencimientos ? 1 : 0);

                if (lstMovimientosAux == null || lstMovimientosAux.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron comprobantes");

                int intCantidadElementos = lstMovimientosAux.Count;

                oRespuesta.success = true;
                oRespuesta.paginacion = new cPaginacion(intCantidadElementos, (int)Math.Ceiling((double)intCantidadElementos / pageSize), pageNumber, pageSize, pageNumber);
                oRespuesta.movimientosDeProveedores = lstMovimientosAux;

                // La V2 además metía en el log el JSON de las claves; acá se loguean solo los parámetros
                LoguearInfo("Success MovimientosDeProveedores GetItem ClaseDeComprobante:" + claseDeComprobanteID + " | Detalle:" + incluirDetalle + " | Relaciones:" + incluirRelacionesItemsCompras + " | MedioDePago:" + incluirMediosDePago,
                            strEndpoint, stopwatch, intCantidadElementos, pageNumber, pageSize);

                return oRespuesta;
            }
            catch (cDatosNoEncontradosException oDatosNoEncontrados)
            {
                // Igual que la V2: 200 con success = true y el error adentro
                oRespuesta.success = true;
                oRespuesta.error = new cError((int)cCodigosError.cDatosNoEncontrados, oDatosNoEncontrados.Message, ApiErrors.errDatosNoEncontrados);
                return oRespuesta;
            }
        }

        /// <summary>
        /// Graba comprobantes de compra (en la V2 solo lo usa OrdenesDeCompra/Create).
        /// Devuelve un resultado por comprobante: los grabados con success = true, los rechazados con el motivo en mensaje.
        /// </summary>
        public List<cResponseSaveMovimientosProveedores> Create(List<cMovimientoProveedor>? lstMovimientosProveedores)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<cResponseSaveMovimientosProveedores> lstSaveProveedores = new List<cResponseSaveMovimientosProveedores>();

            // NUEVO respecto de la V2: sin comprobantes devuelve 400 (la V2 devolvía 200 con el aviso en "notas")
            if (lstMovimientosProveedores == null || lstMovimientosProveedores.Count == 0)
                throw new cCodigoNoDetalladoException("No se encontraron movimientos de proveedores a grabar");   // → 400

            // NUEVO respecto de la V2: se fuerza la empresa de la sesión (la V2 grababa la EmpresaID que viniera en el body)
            lstMovimientosProveedores.ForEach(x => x.EmpresaID = (short)Sesion.EmpresaID);

            List<cMovimientoProveedor>? lstMovimientosImportados;
            Dictionary<string, (string, string)>? lstErrores;

            // NUEVO respecto de la V2: un solo Save a la vez, igual que pedidos y cobros en Ventas,
            // para que dos requests simultáneos no tomen el mismo número de comprobante
            lock (_lockOrdenesDeCompra)
            {
                (lstMovimientosImportados, lstErrores) = movimientosProveedoresBLL.Save(lstMovimientosProveedores, true);
            }

            if (lstMovimientosImportados?.Count > 0)
            {
                foreach (cMovimientoProveedor oMovimiento in lstMovimientosImportados)
                {
                    lstSaveProveedores.Add(new cResponseSaveMovimientosProveedores(oMovimiento, true, ""));
                }
            }

            // FORMATO DE LA CLAVE DEL ERROR - [IndiceCodigoError;ComprobanteID;Serie;PuntoDeVenta;Numero;CodigoDeImportacion]
            if (lstErrores?.Count > 0)
            {
                foreach (KeyValuePair<string, (string, string)> oErrores in lstErrores)
                {
                    List<string> listaStrings = oErrores.Key.Split(';').ToList();

                    string strnumeroComprobante = listaStrings[4];
                    if (listaStrings[4].IndexOf("-") != -1)   // SI VIENE CON UN GUION LO SEPARO PARA TOMAR SOLAMENTE EL NUMERO DE COMPROBANTE
                        strnumeroComprobante = listaStrings[4].Split('-').First();

                    cResponseSaveMovimientosProveedores oRespuestaComprobanteConError = new cResponseSaveMovimientosProveedores();
                    oRespuestaComprobanteConError.serie = listaStrings[2];
                    oRespuestaComprobanteConError.puntoDeVentaID = Convert.ToInt32(listaStrings[3]);
                    oRespuestaComprobanteConError.codigoDeImportacion = listaStrings[5];
                    oRespuestaComprobanteConError.comprobanteID = Convert.ToInt32(listaStrings[1]);
                    oRespuestaComprobanteConError.numero = Convert.ToInt32(strnumeroComprobante);
                    oRespuestaComprobanteConError.mensaje = oErrores.Value.Item1 + " | " + oErrores.Value.Item2;
                    lstSaveProveedores.Add(oRespuestaComprobanteConError);
                }
            }

            LoguearInfo("Success MovimientosProveedores Create", Endpoint.Compras.OrdenesDeCompra("Create"),
                        stopwatch, lstMovimientosProveedores.Count, 1, lstMovimientosProveedores.Count);

            return lstSaveProveedores;
        }

        #region Auxiliares

        /// <summary>
        /// Endpoint para el log según la clase de comprobante (la V2 logueaba todos los GetList como "Ventas/Facturas")
        /// </summary>
        private static string DevolverEndpoint(int claseDeComprobanteID, string accion)
        {
            switch (claseDeComprobanteID)
            {
                case (int)ClaseDeComprobante.Compras_Factura: return Endpoint.Compras.Facturas(accion);
                case (int)ClaseDeComprobante.Compras_NotaDeCredito: return Endpoint.Compras.NotasDeCredito(accion);
                case (int)ClaseDeComprobante.Compras_NotaDeDebito: return Endpoint.Compras.NotasDeDebito(accion);
                case (int)ClaseDeComprobante.Compras_NotaDePedido: return Endpoint.Compras.OrdenesDeCompra(accion);
                case (int)ClaseDeComprobante.Compras_OrdenDePago: return Endpoint.Compras.OrdenesDePago(accion);
                default: return Endpoint.Compras.Remitos(accion);
            }
        }

        #endregion
    }
}

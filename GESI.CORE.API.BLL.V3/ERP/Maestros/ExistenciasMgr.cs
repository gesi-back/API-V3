using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IExistenciasServicio
    {
        cResponseExistencias GetList(string codigos = "",
                                     int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                     int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                     string fechahasta = "",
                                     string? sucursales = "",
                                     string? almacenes = "",
                                     string? estado = null,
                                     string incluirProductosSinExistencias = "S",
                                     string incluirExistenciasLotes = "N",
                                     string? parametros = "");

        cResponseExistencias GetListConModificacionesDesdeFecha(string fecha = "",
                                                                int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                                string? almacenes = "",
                                                                string? sucursales = "",
                                                                string parametros = "",
                                                                string incluirExistenciasLotes = "N");
    }

    /// <summary>
    /// Migrado de la V2:
    ///  - GetList viene de Stock.BLL.Existenciasv2Mgr (el que usaba GetExistencias).
    ///  - GetListConModificacionesDesdeFecha viene del ExistenciasMgr estático.
    /// La V2 tenía locks alrededor de las llamadas al Core porque compartía un SessionManager estático;
    /// en la V3 cada request tiene su propia sesión, así que no hacen falta.
    /// </summary>
    internal sealed class ExistenciasServicio(
        IExistenciasBLL existenciasBLL,
        IProductosBLL productosBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IExistenciasServicio
    {
        /// <summary>
        /// Devuelve las existencias de los productos pedidos (o de todos) por almacén.
        /// La paginación es sobre las filas de existencias (producto + almacén), igual que la V2.
        /// </summary>
        public cResponseExistencias GetList(string codigos = "",
                                            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                            string fechahasta = "",
                                            string? sucursales = "",
                                            string? almacenes = "",
                                            string? estado = null,
                                            string incluirProductosSinExistencias = "S",
                                            string incluirExistenciasLotes = "N",
                                            string? parametros = "")
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseExistencias oRespuesta = new cResponseExistencias();

            if (sucursales?.Length == 0)
                sucursales = null;

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            try
            {
                if (codigos?.Length > 0)   // VIENEN CODIGOS EN LA SOLICITUD
                {
                    // NUEVO respecto de la V2: la V2 acá paginaba los códigos y después volvía a paginar las filas del resultado,
                    // así que con pageNumber > 1 no devolvía nada. Ahora se piden todos los códigos y se pagina una sola vez
                    // (sobre las filas), igual que cuando no vienen códigos.
                    codigos = string.Join(",", codigos.Split(',').Where(x => x != ""));
                }
                else   // NO VIENEN CODIGOS: TODOS LOS PRODUCTOS (de las sucursales / estado pedidos)
                {
                    (List<string>? lstCodigos, string? strMensaje) = productosBLL.GetSearchResults(strSucursales: sucursales!, strEstado: estado!);

                    if (lstCodigos == null || lstCodigos.Count == 0)
                        throw new cIntervalException(new cPaginacion(0, 0, pageNumber, pageSize, 0));   // Igual que la V2

                    codigos = string.Join(",", lstCodigos);
                }

                #region REVISO ALMACENES
                // Igual que la V2: si no se piden almacenes se usan los de la habilitación (si tiene)
                string? AlmacenesIDs = null;
                if (almacenes?.Length > 0)
                    AlmacenesIDs = almacenes;
                else if (Sesion.Almacenes != null)
                    AlmacenesIDs = string.Join(",", Sesion.Almacenes);
                #endregion

                DateTime? datFechaHasta = null;
                if (fechahasta?.Length > 0)
                    datFechaHasta = DateTime.Parse(fechahasta);   // NUEVO respecto de la V2: fecha inválida → 400 (la V2 devolvía 500)

                List<dExistenciaProducto>? lstExistenciasProductos = existenciasBLL.GetExistenciaProductos(codigos,
                                                                                                         AlmacenesIDs!,
                                                                                                         datFechaHasta,
                                                                                                         sucursales!,
                                                                                                         blnAgregarEnResultadosAlmacenesSinExistencias: incluirProductosSinExistencias == "S",
                                                                                                         blnIncluirStockLotes: incluirExistenciasLotes.Equals("S"));

                if (lstExistenciasProductos == null)
                    lstExistenciasProductos = new List<dExistenciaProducto>();

                int intcantidadElementos = lstExistenciasProductos.Count;

                List<dExistenciaProducto> lstExistenciasProductosFinal = lstExistenciasProductos.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                // Igual que la V2: sin filas en la página (o sin existencias) responde "Paginacion extendida"
                if (lstExistenciasProductosFinal.Count == 0)
                    throw new cIntervalException(new cPaginacion(intcantidadElementos, (int)Math.Ceiling((double)intcantidadElementos / pageSize), pageNumber, pageSize, pageNumber));

                oRespuesta.existencias = lstExistenciasProductosFinal;
                oRespuesta.success = true;
                oRespuesta.paginacion = new cPaginacion(intcantidadElementos, (int)Math.Ceiling((double)intcantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success Existencias GetList " + (parametros ?? ""), Endpoint.Maestros.Productos("GetExistencias"),
                            stopwatch, intcantidadElementos, pageNumber, pageSize);

                return oRespuesta;
            }
            catch (cIntervalException oIntervalo)
            {
                oRespuesta.paginacion = oIntervalo._paginacion;
                oRespuesta.success = true;
                oRespuesta.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginacion extendida", ApiErrors.errPaginacionExcedida);
                return oRespuesta;
            }
        }

        /// <summary>
        /// Devuelve las existencias de los productos cuyo stock cambió desde una fecha
        /// </summary>
        public cResponseExistencias GetListConModificacionesDesdeFecha(string fecha = "",
                                                                       int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                       int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                                       string? almacenes = "",
                                                                       string? sucursales = "",
                                                                       string parametros = "",
                                                                       string incluirExistenciasLotes = "N")
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseExistencias oResponse = new cResponseExistencias();

            if (fecha == null || fecha.Length == 0)
                throw new cCodigoNoDetalladoException("No se detalló la fecha en la solicitud");   // → 400

            DateTime oFecha = DateTime.Parse(fecha);   // NUEVO respecto de la V2: fecha inválida → 400 (la V2 devolvía 500)

            if (string.IsNullOrEmpty(almacenes))
                almacenes = null;

            if (string.IsNullOrEmpty(sucursales))
                sucursales = null;

            // NUEVO respecto de la V2: se respeta el tamaño máximo de página de la habilitación
            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            List<string>? lstCodigos = existenciasBLL.GetExistenciaProductosConVariacionesDesdeFecha(oFecha);

            // Igual que la V2: sin productos con cambios → 404
            if (lstCodigos == null || lstCodigos.Count == 0)
                throw new cDatosNoEncontradosException("No se encontraron productos con cambios de existencias desde la fecha");

            try
            {
                // Igual que la V2: acá se paginan los códigos (no las filas)
                string codigos = ApiHelper.DevolverCodigosSeparadosPorComa(lstCodigos, pageNumber, pageSize);

                List<dExistenciaProducto>? lstExistencias = existenciasBLL.GetExistenciaProductos(codigos,
                                                                                                strAlmacenIDs: almacenes!,
                                                                                                strSucursales: sucursales!,
                                                                                                blnIncluirStockLotes: incluirExistenciasLotes.Equals("S"));

                oResponse.success = true;
                oResponse.paginacion = new cPaginacion(lstCodigos.Count, (int)Math.Ceiling((double)lstCodigos.Count / pageSize), pageNumber, pageSize, pageNumber);
                oResponse.existencias = lstExistencias;

                LoguearInfo("Success Existencias GetListConModificacionesDesdeFecha " + parametros, Endpoint.Maestros.Productos("GetExistenciasConModificacionesDesdeFecha"),
                            stopwatch, lstExistencias?.Count ?? 0, pageNumber, pageSize);

                return oResponse;
            }
            catch (cIntervalException oIntervalo)
            {
                oResponse.paginacion = oIntervalo._paginacion;
                oResponse.success = true;
                oResponse.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);
                return oResponse;
            }
        }
    }
}

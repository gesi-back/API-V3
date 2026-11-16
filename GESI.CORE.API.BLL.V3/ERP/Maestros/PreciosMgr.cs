using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.CORE.V3.BLL;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IPreciosServicio
    {
        cResponsePrecios GetPrecios(string codigos = "",
                                    int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                    int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                    string fechamodificaciones = "",
                                    string? parametros = "");

        cResponseHistorialDePrecios GetHistorialDePrecios(string? productoID,
                                                          int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                          int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize);
    }

    /// <summary>
    /// Migrado de la V2 (Precios.BLL.PreciosMgr estático con _MiAPISessionMgr estático).
    /// Lo usa ProductosController en GetPrecios y GetHistorialDePrecios.
    /// </summary>
    internal sealed class PreciosServicio(
        IPreciosBLL preciosBLL,
        IHistorialDePreciosBLL historialDePreciosBLL,
        ICanalesDeVentaBLL canalesDeVentaBLL,
        IUtilidadesBLL utilidadesBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IPreciosServicio
    {
        /// <summary>
        /// Devuelve los precios de los productos pedidos (o de los modificados desde una fecha) en los canales de venta del usuario
        /// </summary>
        public cResponsePrecios GetPrecios(string codigos = "",
                                           int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                           int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                           string fechamodificaciones = "",
                                           string? parametros = "")
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponsePrecios oRespuesta = new cResponsePrecios();

            if (string.IsNullOrEmpty(codigos) && string.IsNullOrEmpty(fechamodificaciones))
                throw new cCodigoNoDetalladoException("No se especifico codigos a buscar");   // → 400

            List<string> lstCodigos = (codigos ?? "").Split(',').ToList();

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            // Igual que la V2: los precios salen solo para los canales de venta habilitados al usuario
            List<int> CanalesDeVenta = canalesDeVentaBLL.GetCanalesDeVentaPorUsuario(Sesion.EmpresaID, Sesion.Usuario) ?? new List<int>();

            try
            {
                if (fechamodificaciones?.Length > 0)
                {
                    // NUEVO respecto de la V2: fecha inválida → 400 (la V2 devolvía 500)
                    DateTime oFecha = DateTime.Parse(fechamodificaciones);

                    List<(string, string)>? lstCodigosModificados = utilidadesBLL.GetRegistrosModificadosDesdeFecha(Sesion.EmpresaID,
                                                                                                                    ((int)LogCambios.tPreciosProductos).ToString(),
                                                                                                                    oFecha);

                    if (lstCodigosModificados == null || lstCodigosModificados.Count == 0)
                        throw new cDatosNoEncontradosException("No se encontraron productos modificados desde fecha");

                    lstCodigos = new List<string>();
                    foreach ((string, string) oPrecio in lstCodigosModificados)
                    {
                        lstCodigos.Add(oPrecio.Item2);
                    }
                }

                // Pagina los códigos y trae los precios de esa página
                List<dResultadoConsultaPrecio>? lstPreciosProductos = preciosBLL.GetList(ApiHelper.DevolverCodigosSeparadosPorComa(lstCodigos, pageNumber, pageSize),
                                                                                       CanalesDeVenta.ToArray());

                if (lstPreciosProductos?.Count > 0)
                {
                    // Igual que la V2: si vienen más filas que el tamaño de página (un producto en varios canales) se recortan
                    if (lstPreciosProductos.Count > pageSize)
                        lstPreciosProductos = lstPreciosProductos.Take(pageSize).ToList();

                    oRespuesta.precios = lstPreciosProductos;
                    oRespuesta.paginacion = new cPaginacion(lstCodigos.Count, (int)Math.Ceiling((double)lstCodigos.Count / pageSize), pageNumber, pageSize, pageNumber);
                    oRespuesta.success = true;
                }
                else
                {
                    // Igual que la V2: sin precios responde success = true, sin lista y con paginaActual = 0
                    oRespuesta.paginacion = new cPaginacion(lstCodigos.Count, (int)Math.Ceiling((double)lstCodigos.Count / pageSize), pageNumber, pageSize, 0);
                    oRespuesta.success = true;
                }

                LoguearInfo("Success Precios GetList " + (parametros ?? ""), Endpoint.Maestros.Productos("GetPrecios"),
                            stopwatch, lstPreciosProductos?.Count ?? 0, pageNumber, pageSize);

                return oRespuesta;
            }
            catch (cIntervalException oIntervalo)
            {
                oRespuesta.paginacion = oIntervalo._paginacion;
                oRespuesta.success = true;
                oRespuesta.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);
                return oRespuesta;
            }
            catch (cDatosNoEncontradosException NotFound)
            {
                // Igual que la V2: 200 con success = false y el error adentro
                LoguearInfo("Not Found Precios GetList " + (parametros ?? ""), Endpoint.Maestros.Productos("GetPrecios"), stopwatch, 0, pageNumber, 0);

                oRespuesta.error = new cError((int)cCodigosError.cDatosNoEncontrados, NotFound.Message, ApiErrors.errDatosNoEncontrados);
                return oRespuesta;
            }
        }

        /// <summary>
        /// Devuelve el historial de precios de un producto
        /// </summary>
        public cResponseHistorialDePrecios GetHistorialDePrecios(string? productoID,
                                                                 int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                 int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseHistorialDePrecios oRespuesta = new cResponseHistorialDePrecios();

            if (productoID == null || productoID.Length == 0)
                throw new cCodigoNoDetalladoException("No se especifico un ProductoID");   // → 400

            // NUEVO respecto de la V2: se respeta el tamaño máximo de página de la habilitación
            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            List<cHistorialPrecio>? historialPrecios = historialDePreciosBLL.GetList(productoID);

            // Igual que la V2: sin historial → 404
            if (historialPrecios == null || historialPrecios.Count == 0)
            {
                LoguearInfo("Not Found HistorialPrecios GetItem", Endpoint.Maestros.Productos("GetHistorialDePrecios"), stopwatch, 0, pageNumber, 0);
                throw new cDatosNoEncontradosException("No se encontraron Precios Historicos");
            }

            int cantidadElementos = historialPrecios.Count;

            historialPrecios = historialPrecios.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            if (historialPrecios.Count == 0)
            {
                oRespuesta.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, 0);
                oRespuesta.success = true;
                oRespuesta.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);
                return oRespuesta;
            }

            oRespuesta.historialPrecios = historialPrecios;
            oRespuesta.success = true;
            oRespuesta.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

            LoguearInfo("Success PreciosHistoricos GetList", Endpoint.Maestros.Productos("GetHistorialDePrecios"),
                        stopwatch, cantidadElementos, pageNumber, pageSize);

            return oRespuesta;
        }
    }
}

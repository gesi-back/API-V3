using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.CORE;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BO.Dinamicos;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API_con_Core_V3.Controllers
{
    [Route("api/Maestros/[controller]")]
    [ApiController]
    public class ProductosController(IProductosServicio productoServicio,
                                     IPreciosServicio PreciosMgr,
                                     IExistenciasServicio ExistenciasMgr) : ApiControllerBase
    {
        [HttpGet("GetList")]
        public ActionResult<cResponseProductos> Get(int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                    int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                    string costos = "N",
                                    string imagenes = "N",
                                    string fechamodificaciones = "",
                                    string stock = "N",
                                    string publicaecommerce = "T",
                                    string aplicaciones = "N",
                                    string fechamodificacionesimagenes = "",
                                    string precios = "S",
                                    string sucursales = "",
                                    string almacenes = "",
                                    string fechamodificacionesprecios = "",
                                    string codigosadicionales = "N",
                                    string definicionesporalmacen = "N",
                                    string insumos = "N",
                                    string impuestos = "N",
                                    string cuentas = "N",
                                    string elementos = "N",
                                    string canalesDeVenta = "",
                                    string subproductos = "N",
                                    string soloproductospadre = "N",
                                    string rubros = "",
                                    string subrubros = "",
                                    string subsubrubros = "",
                                    string filtroArticulos1 = "",
                                    string filtroArticulos2 = "",
                                    string filtroArticulos3 = "",
                                    string? ordenarpor = null,
                                    string? direccion = "ASC",
                                    string incluirExistenciasEnCero = "N",
                                    string incluirExistenciasLotes = "N",
                                    string incluirSucursalesHabilitadas = "N",
                                    string proveedorID = "",
                                    string incluirLotesDelProducto = "N",
                                    string codigos = "",
                                    string? estado = null)
        => Responder(() => productoServicio.GetList(pageNumber,
                                                                            pageSize,
                                                                            costos,
                                                                            imagenes,
                                                                            fechamodificaciones,
                                                                            stock,
                                                                            publicaecommerce,
                                                                            aplicaciones,
                                                                            fechamodificacionesimagenes,
                                                                            precios, fechamodificacionesprecios: fechamodificacionesprecios, codigosadicionales: codigosadicionales, definicionesporalmacen: definicionesporalmacen,
                                                                            insumos: insumos,
                                                                            impuestos: impuestos, cuentas: cuentas, elementos: elementos, canalesDeVenta: canalesDeVenta, subproductos: subproductos, soloproductospadre: soloproductospadre, rubros: rubros, subrubros: subrubros, subsubrubros: subsubrubros, filtroArticulos1: filtroArticulos1, filtroArticulos2: filtroArticulos2, filtroArticulos3: filtroArticulos3, ordenarPor: ordenarpor, direccion: direccion, incluirExistenciaAlmacenesEnCero: incluirExistenciasEnCero, incluirExistenciasLotes: incluirExistenciasLotes, incluirSucursalesHabilitadas: incluirSucursalesHabilitadas, proveedorID: proveedorID, parametros: this.HttpContext.Request.QueryString.ToString(), incluirLotesDelProducto: incluirLotesDelProducto, codigos: codigos, estado: estado));

        [HttpGet("GetSearchResults")]
        public ActionResult<cResponseProductos> GetSearchResults(string expresion = "",
                                            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                            string costos = "N",
                                            string categoriasafiltrar = "",
                                            string imagenes = "N",
                                            string stock = "N",
                                            string publicaecommerce = "T",
                                            string aplicaciones = "N",
                                            string codigosadicionales = "N",
                                            string definicionesporalmacen = "N",
                                            string insumos = "N",
                                            string impuestos = "N",
                                            string cuentas = "N",
                                            string elementos = "N",
                                            string soloproductospadre = "N",
                                            string rubros = "",
                                            string subrubros = "",
                                            string subsubrubros = "",
                                            string filtroArticulos1 = "",
                                            string filtroArticulos2 = "",
                                            string filtroArticulos3 = "",
                                            string? ordenarPor = null,
                                            string? direccion = null,
                                            string? estado = null,
                                            string? incluirSucursalesHabilitadas = "N",
                                            string proveedorID = "",
                                            string incluirLotesDelProducto = "N")
            => Responder(() => productoServicio.GetSearchResults(expresion,
                                                                                    pageNumber,
                                                                                    pageSize,
                                                                                    costos,
                                                                                    categoriasafiltrar,
                                                                                    imagenes,
                                                                                    stock,
                                                                                    publicaecommerce,
                                                                                    aplicaciones, definicionesporalmacen: definicionesporalmacen,
                                                                                    insumos: insumos,
                                                                                    impuestos: impuestos, cuentas: cuentas, elementos: elementos, soloproductospadre: soloproductospadre, rubros: rubros, subrubros: subrubros, subsubrubros: subsubrubros, filtroArticulos1: filtroArticulos1, filtroArticulos2: filtroArticulos2, filtroArticulos3: filtroArticulos3, ordenarPor: ordenarPor, direccion: direccion, estado: estado, proveedorID: proveedorID, parametros: this.HttpContext.Request.QueryString.ToString(), token: "", incluirLotesDelProducto: incluirLotesDelProducto));

        [HttpGet("GetItem")]
        public ActionResult<cResponseProducto> GetItem(string productoID = "",
                                    int canalDeVentaID = 0,
                                    string costos = "N",
                                    string imagenes = "N",
                                    string stock = "N",
                                    string insumos = "N",
                                    string impuestos = "N",
                                    string cuentas = "N",
                                    string elementos = "N",
                                    string subproductos = "N",
                                    string incluirExistenciasEnCero = "N",
                                    string incluirExistenciasLotes = "N",
                                    string incluirSucursalesHabilitadas = "N")
            => Responder(() =>  productoServicio.GetItem(productoID,
                                                                            canalDeVentaID,
                                                                            costos,
                                                                            imagenes,
                                                                            stock,
                                                                            insumos: insumos,
                                                                            impuestos: impuestos,
                                                                            cuentas: cuentas,
                                                                            subproductos: subproductos, strIncluirExistenciasEnCero: incluirExistenciasEnCero, incluirExistenciasLotes: incluirExistenciasLotes));



        [HttpGet("GetHistorialDeCostos")]
        [EnableCors("MyCorsPolicy")]
        [Produces("application/json")]
        public ActionResult<cResponseHistorialCostos> GetHistorialCostos(int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                string? productoID = "",
                                                DateTime? fechaDesde = null,
                                                DateTime? fechaHasta = null)
            => Responder(() => productoServicio.HistorialCostosGet(pageNumber, pageSize, productoID));

        /// <summary>
        /// Obtiene los precios de los productos en los canales de venta del usuario
        /// </summary>
        /// <param name="codigos">Codigos de producto separados por coma</param>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="fechamodificaciones">Devuelve los productos con precios modificados desde esta fecha (en lugar de codigos)</param>
        [HttpGet("GetPrecios")]
        public ActionResult<cResponsePrecios> GetPrecios(string codigos = "",
                                                         int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                         int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                         string fechamodificaciones = "")
            => Responder(() => PreciosMgr.GetPrecios(codigos, pageNumber, pageSize, fechamodificaciones, parametros: HttpContext.Request.QueryString.ToString()));

        /// <summary>
        /// Obtiene el historial de precios de un producto
        /// </summary>
        [HttpGet("GetHistorialDePrecios")]
        public ActionResult<cResponseHistorialDePrecios> GetHistorialPrecios(int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                             int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                                             string? productoID = "")
            => Responder(() => PreciosMgr.GetHistorialDePrecios(productoID, pageNumber, pageSize));

        /// <summary>
        /// Obtiene las existencias de los productos por almacén
        /// </summary>
        /// <param name="codigos">Codigos de producto separados por coma (vacío = todos)</param>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de filas por pagina</param>
        /// <param name="fechahasta">Existencias a una fecha</param>
        /// <param name="sucursales">Filtra por sucursales separadas por coma</param>
        /// <param name="almacenes">Filtra por almacenes separados por coma (vacío = los de la habilitación)</param>
        /// <param name="estado">Estado de los productos (solo cuando no vienen codigos)</param>
        /// <param name="incluirProductosSinExistencias">S/N</param>
        /// <param name="incluirExistenciasLotes">S/N</param>
        [HttpGet("GetExistencias")]
        public ActionResult<cResponseExistencias> GetExistencias(string codigos = "",
                                                                 int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                 int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                                 string fechahasta = "",
                                                                 string sucursales = "",
                                                                 string almacenes = "",
                                                                 string? estado = null,
                                                                 string incluirProductosSinExistencias = "N",
                                                                 string incluirExistenciasLotes = "N")
            => Responder(() => ExistenciasMgr.GetList(codigos,
                                                      pageNumber,
                                                      pageSize,
                                                      fechahasta,
                                                      sucursales,
                                                      almacenes,
                                                      estado,
                                                      incluirProductosSinExistencias,
                                                      incluirExistenciasLotes,
                                                      parametros: HttpContext.Request.QueryString.ToString()));

        /// <summary>
        /// Obtiene las existencias de los productos cuyo stock cambió desde una fecha
        /// </summary>
        [HttpGet("GetExistenciasConModificacionesDesdeFecha")]
        public ActionResult<cResponseExistencias> GetExistenciasConModificacionesDesdeFecha(string fechamodificaciones = "",
                                                                                            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                                            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                                                            string? almacenes = "",
                                                                                            string? sucursales = "",
                                                                                            string? incluirExistenciasLotes = "N")
            => Responder(() => ExistenciasMgr.GetListConModificacionesDesdeFecha(fechamodificaciones,
                                                                                 pageNumber,
                                                                                 pageSize,
                                                                                 almacenes,
                                                                                 sucursales,
                                                                                 parametros: HttpContext.Request.QueryString.ToString(),
                                                                                 incluirExistenciasLotes: incluirExistenciasLotes ?? "N"));

        /// <summary>
        /// Crea productos en el ERP (máximo 10 por llamada).
        /// Igual que la V2: devuelve un resultado por producto; lista vacía o más de 10 → 200 con el error en el único ítem.
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<List<cResponseProductosSave>> Create([FromBody] List<GESI.ERP.Core.V3.BO.cProducto>? lstProductos)
            => ResponderLista(() => productoServicio.Save(lstProductos!, true),
                              error => new cResponseProductosSave(false, "", "", error.message ?? "", "", 0, 0) { error = error });

        /// <summary>
        /// Actualiza los datos de productos en el ERP (máximo 10 por llamada)
        /// </summary>
        [HttpPut("Update")]
        public ActionResult<List<cResponseProductosSave>> Update([FromBody] List<GESI.ERP.Core.V3.BO.cProducto>? lstProductos)
            => ResponderLista(() => productoServicio.Save(lstProductos!, false),
                              error => new cResponseProductosSave(false, "", "", error.message ?? "", "", 0, 0) { error = error });

        /// <summary>
        /// Obtiene las configuraciones de los productos (descripciones de los filtros, etc.)
        /// </summary>
        /// <param name="seccionID">Codigo de seccion</param>
        /// <param name="itemID">Codigo de item</param>
        [HttpGet("Configuraciones/GetList")]
        public ActionResult<cConfiguraciones> GetConfiguraciones(string seccionID = "", string itemID = "")
            => Responder(() => productoServicio.GetConfiguraciones(seccionID, itemID));

        /// <summary>
        /// Actualiza los costos de los productos por proveedor
        /// </summary>
        /// <param name="costos">Costos agrupados por proveedor</param>
        /// <param name="agregarfaltantes">Si el producto no tiene asignado el proveedor, lo agrega</param>
        [HttpPut("Costos/Update")]
        public ActionResult<List<dActualizacionCostoProductoProveedorResponse>> UpdateCostos([FromBody] List<cActualizacionCosto>? costos, bool agregarfaltantes = false)
            => ResponderLista(() => productoServicio.ActualizacionCostosPorProveedor(costos, agregarfaltantes),
                              error => new dActualizacionCostoProductoProveedorResponse { message = error.message });


        /// <summary>
        /// Crea productos en el ERP (máximo 10 por llamada).
        /// Igual que la V2: devuelve un resultado por producto; lista vacía o más de 10 → 200 con el error en el único ítem.
        /// </summary>
        [HttpPost("update-partial")]
        public ActionResult<List<cResponseProductosSave>> UpdatePartial([FromBody] List<GESI.ERP.Core.V3.BO.cProducto>? lstProductos)
            => ResponderLista(() => productoServicio.SavePartial(lstProductos),
                              error => new cResponseProductosSave(false, "", "", error.message ?? "", "", 0, 0) { error = error });

    }
}

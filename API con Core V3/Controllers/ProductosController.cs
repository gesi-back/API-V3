using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BO.V3;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API_con_Core_V3.Controllers
{
    [Route("api/Maestros/[controller]")]
    [ApiController]
    public class ProductosController(IProductosServicio productoServicio) : ApiControllerBase
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



    }
}

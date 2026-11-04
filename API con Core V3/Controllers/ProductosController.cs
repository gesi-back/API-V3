using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BO.V3;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API_con_Core_V3.Controllers
{
    [Route("api/Maestros/[controller]")]
    [ApiController]
    public class ProductosController(IProductosServicio productoServicio) : ControllerBase
    {
        [HttpGet("GetList")]
        public ActionResult<cResponseCanalesDeVenta> Get(int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
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
                                    string? incluirExistenciasEnCero = "N",
                                    string? incluirExistenciasLotes = "N",
                                    string? incluirSucursalesHabilitadas = "N",
                                    string proveedorID = "",
                                    string? incluirLotesDelProducto = "N",
                                    string codigos = "",
                                    string? estado = null)
        => Ok(productoServicio.GetList(pageNumber,
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
    }
}

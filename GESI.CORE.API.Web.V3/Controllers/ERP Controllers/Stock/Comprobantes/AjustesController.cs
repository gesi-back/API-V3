using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Stock;
using GESI.CORE.API.BO.V3.ERP.Stock;
using GESI.CORE.V3.BLL;
using GESI.ERP.Core.V3.BO.Dinamicos;
using GESI.ERP.Stock.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de ajustes de stock
    /// </summary>
    [ApiController]
    [Route("api/Stock/Comprobantes/[controller]")]
    [Produces("application/json")]
    public class AjustesController(IMovimientosDeStockServicio MovimientosDeStockMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene un listado de ajustes de stock
        /// </summary>
        /// <param name="comprobanteID">Filtra por codigo de comprobante</param>
        /// <param name="puntoDeVentaID">Filtra por punto de venta</param>
        /// <param name="almacenID">Filtra por almacen</param>
        /// <param name="fechaDesde">Filtra los comprobantes desde una fecha</param>
        /// <param name="fechaHasta">Filtra los comprobantes hasta una fecha</param>
        /// <param name="sucursalID">Filtra los comprobantes por sucursal</param>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseMovimientoDeStock> GetList(int comprobanteID = 0,
                                                                int puntoDeVentaID = 0,
                                                                int almacenID = 0,
                                                                DateTime? fechaDesde = null,
                                                                DateTime? fechaHasta = null,
                                                                int sucursalID = 0,
                                                                int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeStockMgr.GetList(comprobanteID,
                                                             puntoDeVentaID,
                                                             almacenID,
                                                             fechaDesde,
                                                             fechaHasta,
                                                             sucursalID,
                                                             pageNumber,
                                                             pageSize,
                                                             (int)ClaseDeComprobante.Productos_Ajustes));

        /// <summary>
        /// Obtiene ajustes de stock por clave
        /// </summary>
        [HttpPost("GetItem")]
        public ActionResult<cResponseMovimientoDeStock> GetItem([FromBody] List<dClavesComprobante>? claves,
                                                                int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                                int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber)
            => Responder(() => MovimientosDeStockMgr.GetItem(claves, (int)ClaseDeComprobante.Productos_Ajustes, pageSize, pageNumber));

        /// <summary>
        /// Graba un comprobante de ajustes de stock
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<cResponseMovimientoDeStock> Create([FromBody] cMovimientoDeStock? oMovimiento)
        {
            ActionResult<cResponseMovimientoDeStock> oResultado = Responder(() => MovimientosDeStockMgr.Save(oMovimiento, (int)ClaseDeComprobante.Productos_Ajustes));

            // Igual que la V2: si el Core rechazó el comprobante responde 400 con los motivos
            OkObjectResult? oOk = oResultado.Result as OkObjectResult;
            cResponseMovimientoDeStock? oRespuesta = oOk?.Value as cResponseMovimientoDeStock;

            if (oRespuesta != null && !oRespuesta.success && oRespuesta.error != null)
                return BadRequest(oRespuesta);

            return oResultado;
        }
    }
}

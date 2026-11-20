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
    /// Controlador de transferencias de stock
    /// </summary>
    [ApiController]
    [Route("api/Stock/Comprobantes/[controller]")]
    [Produces("application/json")]
    public class TransferenciasController(IMovimientosDeStockServicio MovimientosDeStockMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene un listado de transferencias de stock
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
                                                             (int)ClaseDeComprobante.Productos_TransferenciaDeStock));

        /// <summary>
        /// Obtiene transferencias de stock por clave
        /// </summary>
        [HttpPost("GetItem")]
        public ActionResult<cResponseMovimientoDeStock> GetItem([FromBody] List<dClavesComprobante>? claves,
                                                                int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                                int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber)
            => Responder(() => MovimientosDeStockMgr.GetItem(claves, (int)ClaseDeComprobante.Productos_TransferenciaDeStock, pageSize, pageNumber));

        /// <summary>
        /// Graba un comprobante de transferencias de stock.
        /// Igual que la V2: si el Core lo rechaza responde 200 con success = false y los motivos en el error.
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<cResponseMovimientoDeStock> Create([FromBody] cMovimientoDeStock? oMovimiento)
            => Responder(() => MovimientosDeStockMgr.Save(oMovimiento, (int)ClaseDeComprobante.Productos_TransferenciaDeStock));
    }
}

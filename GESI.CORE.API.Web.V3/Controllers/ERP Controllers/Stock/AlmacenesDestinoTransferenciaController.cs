using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Stock;
using GESI.CORE.API.BO.V3.ERP.Stock;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de almacenes destino de transferencias de stock
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class AlmacenesDestinoTransferenciaController(IAlmacenesDestinoTransferenciaServicio AlmacenesDestinoTransferenciaMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene los almacenes a los que se puede transferir desde un almacén
        /// </summary>
        /// <param name="almacenID">Almacen de origen (obligatorio)</param>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseAlmacenesDestintoTransferencia> GetListAlmacenes(int almacenID = 0,
                                                                                      int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                                      int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => AlmacenesDestinoTransferenciaMgr.GetList(pageNumber, pageSize, (short)almacenID));
    }
}

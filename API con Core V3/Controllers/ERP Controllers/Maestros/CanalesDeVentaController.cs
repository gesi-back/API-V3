using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.CORE.V3.BLL;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    [ApiController]
    [Route("api/Maestros/[controller]")]
    public class CanalesDeVentaController(ICanalesDeVentaServicio canalesDeVenta) : ApiControllerBase
    {

        [HttpGet("GetList")]
        public ActionResult<cResponseCanalesDeVenta> Get(
        int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
        int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
        => Responder(() => canalesDeVenta.GetList(pageNumber, pageSize));

        /// <summary>
        /// Obtiene un canal de venta
        /// </summary>
        /// <param name="canalDeVentaID">Codigo de canal de venta</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseCanalDeVenta> GetItem(int canalDeVentaID = 0)
            => Responder(() => canalesDeVenta.GetItem(canalDeVentaID));
    }
}

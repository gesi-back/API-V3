using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BO.V3;
using GESI.CORE.V3.BLL;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    [ApiController]
    [Route("api/Maestros/[controller]")]
    public class CanalesDeVentaController(ICanalesDeVentaServicio canalesDeVenta) : ControllerBase
    {

        [HttpGet("GetList")]
        public ActionResult<cResponseCanalesDeVenta> Get(
        int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
        int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
        => Ok(canalesDeVenta.GetList(pageNumber, pageSize));
    }
}

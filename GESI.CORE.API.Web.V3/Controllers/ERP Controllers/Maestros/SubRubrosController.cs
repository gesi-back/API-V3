using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de subrubros
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class SubRubrosController(ISubRubrosServicio subRubros) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de subrubros de la empresa
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseSubRubros> GetList(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => subRubros.GetList(pageNumber, pageSize));
    }
}

using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BO.V3;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de rubros
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class RubrosController(IRubrosServicio rubros) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de rubros de la empresa
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseRubros> GetList(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => rubros.GetList(pageNumber, pageSize));
    }
}

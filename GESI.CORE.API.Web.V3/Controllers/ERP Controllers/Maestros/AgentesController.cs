using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de agentes
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    public class AgentesController(IAgentesServicio agentes) : ApiControllerBase
    {
        /// <summary>
        /// Permite obtener el listado de agentes cargados dentro del ERP
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="vendedorID">Filtra por vendedor (0 = todos)</param>
        /// <param name="agenteID">Filtra por agente (0 = todos)</param>
        [HttpGet("GetList")]
        [Produces("application/json")]
        public ActionResult<cResponseAgentes> GetList(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
            int vendedorID = 0,
            int agenteID = 0)
            => Responder(() => agentes.GetList(pageNumber, pageSize, vendedorID, agenteID));
    }
}

using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de repartidores
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class RepartidoresController(IRepartidoresServicio repartidores) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de repartidores
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseRepartidores> GetList(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => repartidores.GetList(pageNumber, pageSize));

        /// <summary>
        /// Obtiene un repartidor
        /// </summary>
        /// <param name="repartidorID">Codigo de repartidor</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseRepartidor> GetItem(int repartidorID = 0)
            => Responder(() => repartidores.GetItem(repartidorID));
    }
}

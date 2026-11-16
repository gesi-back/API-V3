using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de grupos de productos
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class GruposDeProductosController(IGruposDeProductosServicio grupos) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de grupos de productos
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseGrupoDeArticulos> GetList(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => grupos.GetList(pageNumber, pageSize));

        /// <summary>
        /// Obtiene un grupo de productos
        /// </summary>
        /// <param name="grupoArticulosID">Codigo de grupo de productos</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseGrupoDeArticulo> GetItem(int grupoArticulosID = 0)
            => Responder(() => grupos.GetItem(grupoArticulosID));
    }
}

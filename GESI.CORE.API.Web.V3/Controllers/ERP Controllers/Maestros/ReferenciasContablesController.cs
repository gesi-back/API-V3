using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de referencias contables
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class ReferenciasContablesController(IReferenciasContablesServicio referencias) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de referencias contables
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseReferenciasContables> GetList(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => referencias.GetList(pageNumber, pageSize));

        /// <summary>
        /// Obtiene una referencia contable
        /// </summary>
        /// <param name="referenciaContableID">Codigo de referencia contable</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseReferenciaContable> GetItem(int referenciaContableID = 0)
            => Responder(() => referencias.GetItem(referenciaContableID));
    }
}

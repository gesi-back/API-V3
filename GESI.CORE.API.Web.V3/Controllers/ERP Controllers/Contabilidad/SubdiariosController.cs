using GESI.Contabilidad.V3.BO;
using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Contabilidad;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Contabilidad;
using GESI.ERP.Core.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de subdiarios contables
    /// </summary>
    [ApiController]
    [Route("api/Maestros/Contabilidad/[controller]")]
    [Produces("application/json")]
    public class SubdiariosController(ISubdiariosServicio SubdiariosMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene los subdiarios de un ejercicio
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="ejercicioID">Codigo de ejercicio</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseSubdiarios> GetList(int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                         int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                         int ejercicioID = 0)
            => Responder(() => SubdiariosMgr.GetList(pageNumber, pageSize, ejercicioID));

        /// <summary>
        /// Crea subdiarios
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<List<cResponseSubdiarioSave>> Create([FromBody] List<cSubdiario>? oSubdiarios = null)
            => ResponderLista(() => SubdiariosMgr.Save(oSubdiarios),
                              error => new cResponseSubdiarioSave(null, false, error.message));

        /// <summary>
        /// Actualiza subdiarios
        /// </summary>
        [HttpPut("Update")]
        public ActionResult<List<cResponseSubdiarioSave>> Update([FromBody] List<cSubdiario>? oSubdiarios = null)
            => ResponderLista(() => SubdiariosMgr.Save(oSubdiarios),
                              error => new cResponseSubdiarioSave(null, false, error.message));

        /// <summary>
        /// Elimina subdiarios de un ejercicio
        /// </summary>
        /// <param name="ejercicioID">Codigo de ejercicio</param>
        /// <param name="codigos">Codigos de subdiario separados por coma</param>
        [HttpDelete("Delete")]
        public ActionResult<cResponse> Delete(int ejercicioID = 0, string? codigos = "")
            => Responder(() => SubdiariosMgr.Delete(ejercicioID, codigos));
    }
}

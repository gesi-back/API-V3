using GESI.Contabilidad.V3.BO;
using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Contabilidad;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Contabilidad;
using GESI.ERP.Core.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de ejercicios contables
    /// </summary>
    [ApiController]
    [Route("api/Maestros/Contabilidad/[controller]")]
    [Produces("application/json")]
    public class EjerciciosController(IEjerciciosServicio EjerciciosMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de ejercicios contables
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseEjercicios> GetList(int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                         int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => EjerciciosMgr.GetList(pageNumber, pageSize));

        /// <summary>
        /// Crea ejercicios contables
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<List<cResponseEjerciciosSave>> Create([FromBody] List<cEjercicio>? oEjercicios = null)
            => ResponderLista(() => EjerciciosMgr.Save(oEjercicios),
                              error => new cResponseEjerciciosSave(null, false, error.message));

        /// <summary>
        /// Actualiza ejercicios contables
        /// </summary>
        [HttpPut("Update")]
        public ActionResult<List<cResponseEjerciciosSave>> Update([FromBody] List<cEjercicio>? oEjercicios = null)
            => ResponderLista(() => EjerciciosMgr.Save(oEjercicios),
                              error => new cResponseEjerciciosSave(null, false, error.message));

        /// <summary>
        /// Elimina un ejercicio contable
        /// </summary>
        /// <param name="ejercicioID">Codigo de ejercicio</param>
        [HttpDelete("Delete")]
        public ActionResult<cResponse> Delete(int ejercicioID = 0)
            => Responder(() => EjerciciosMgr.Delete(ejercicioID));
    }
}

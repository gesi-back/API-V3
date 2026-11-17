using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Contabilidad;
using GESI.CORE.API.BO.V3.ERP.Contabilidad;
using GESI.Contabilidad.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de asientos contables
    /// </summary>
    [ApiController]
    [Route("api/Maestros/Contabilidad/[controller]")]
    [Produces("application/json")]
    public class AsientosController(IAsientosServicio AsientosMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene los asientos de un ejercicio
        /// </summary>
        /// <param name="ejercicioID">Codigo de ejercicio</param>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseAsientos> GetList(int ejercicioID = 0,
                                                       int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                       int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => AsientosMgr.GetList(ejercicioID, pageNumber, pageSize));

        /// <summary>
        /// Graba un asiento contable con su detalle
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<cResponseSaveAsiento> Create([FromBody] cAsiento? oAsiento)
            => Responder(() => AsientosMgr.Save(oAsiento));
    }
}

using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Contabilidad;
using GESI.CORE.API.BO.V3.ERP.Contabilidad;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de asientos contables.
    /// PENDIENTE: Create de la V2 no se migró porque el Core V3 ya no tiene Contable.AsientosManager.Save.
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
    }
}

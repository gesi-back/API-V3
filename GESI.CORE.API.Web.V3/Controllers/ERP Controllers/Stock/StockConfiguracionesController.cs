using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.CORE;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de configuraciones de stock
    /// </summary>
    [ApiController]
    [Route("api/Stock/Configuraciones")]
    [Produces("application/json")]
    public class StockConfiguracionesController(IConfiguracionesServicio ConfiguracionesMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene las configuraciones de stock de los comprobantes de la empresa
        /// </summary>
        /// <param name="seccionID">Filtra por seccion</param>
        /// <param name="itemID">Filtra por item</param>
        [HttpGet("GetList")]
        public ActionResult<cConfiguraciones> Get(string seccionID = "", string itemID = "")
            => Responder(() => ConfiguracionesMgr.GetConfiguraciones("stock", seccionID, itemID));
    }
}

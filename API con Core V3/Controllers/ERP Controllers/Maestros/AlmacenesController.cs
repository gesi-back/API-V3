using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de almacenes
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class AlmacenesController(IAlmacenesServicio almacenes) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de almacenes del ERP (filtrado por la sucursal de la sesión, salvo casa central)
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="ControlDeTransferencias">Filtra por almacenes con control temporal de transferencias</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseAlmacenes> GetList(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
            bool? ControlDeTransferencias = null)
            => Responder(() => almacenes.GetList(pageNumber, pageSize, ControlDeTransferencias));

        /// <summary>
        /// Obtiene un almacen del ERP
        /// </summary>
        /// <param name="almacenID">Codigo de almacen</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseAlmacen> GetItem(int almacenID = 0)
            => Responder(() => almacenes.GetItem(almacenID));
    }
}

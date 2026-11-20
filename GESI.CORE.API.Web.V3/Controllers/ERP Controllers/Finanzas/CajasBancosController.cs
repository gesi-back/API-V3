using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Finanzas;
using GESI.CORE.API.BO.V3.ERP.Finanzas;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de cajas / bancos
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class CajasBancosController(ICajasBancosServicio CajaBancoMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de cajas / bancos
        /// </summary>
        /// <param name="sucursalID">Filtra por sucursal (0 = todas)</param>
        /// <param name="estado">Estado de la caja / banco (A = activas)</param>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseCajasBancos> GetList(int sucursalID = 0,
                                                          string estado = "A",
                                                          int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                          int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => CajaBancoMgr.GetList(sucursalID, estado, pageNumber, pageSize));

        /// <summary>
        /// Obtiene una caja / banco
        /// </summary>
        /// <param name="cajabancoID">Codigo de caja / banco</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseCajaBanco> GetItem(int cajabancoID = 0)
            => Responder(() => CajaBancoMgr.GetItem(cajabancoID));
    }
}

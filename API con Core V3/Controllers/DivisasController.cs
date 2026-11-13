using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BO.V3;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de divisas
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class DivisasController(IDivisasServicio divisas) : ApiControllerBase
    {
        /// <summary>
        /// Permite obtener el listado de divisas del ERP con sus cotizaciones
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="divisaID">Filtra por divisa (0 = todas)</param>
        /// <param name="fechaDesde">Fecha desde de las cotizaciones</param>
        /// <param name="fechaHasta">Fecha hasta de las cotizaciones</param>
        /// <param name="ultimaCotizacion">Si es true devuelve solo la ultima cotizacion de cada divisa</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseDivisas> GetList(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
            int divisaID = 0,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            bool ultimaCotizacion = true)
            => Responder(() => divisas.GetList(pageNumber, pageSize, divisaID, fechaDesde, fechaHasta, ultimaCotizacion));
    }
}

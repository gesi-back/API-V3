using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de estados de comprobante de venta
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class EstadosDeComprobanteController(IEstadosDeComprobanteServicio estados) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de estados de comprobante de venta
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="estadosIDs">Se acepta por compatibilidad con la V2, pero (igual que en la V2) no filtra</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseEstadosComprobantes> GetList(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
            string estadosIDs = "")
            => Responder(() => estados.GetList(pageNumber, pageSize));
    }
}

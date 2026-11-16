using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de puntos de venta
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class PuntosDeVentaController(IPuntosDeVentaServicio puntosDeVenta) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de puntos de venta
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponsePuntosDeVenta> GetList(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => puntosDeVenta.GetList(pageNumber, pageSize));

        /// <summary>
        /// Obtiene un punto de venta
        /// </summary>
        /// <param name="puntoDeVentaID">Codigo de punto de venta</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponsePuntoDeVenta> GetItem(int puntoDeVentaID = 0)
            => Responder(() => puntosDeVenta.GetItem(puntoDeVentaID));
    }
}

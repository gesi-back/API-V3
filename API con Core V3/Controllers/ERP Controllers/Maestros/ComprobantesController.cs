using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador del maestro de comprobantes (tipos de comprobante del ERP)
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class ComprobantesController(IComprobantesServicio comprobantes) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de comprobantes activos de la sucursal
        /// </summary>
        /// <param name="comprobanteID">Filtra por comprobante (0 = todos)</param>
        /// <param name="claseDeComprobanteID">Filtra por clase de comprobante (0 = todas)</param>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseComprobantes> GetList(
            int comprobanteID = 0,
            int claseDeComprobanteID = 0,
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => comprobantes.GetList(comprobanteID, claseDeComprobanteID, pageNumber, pageSize));

        /// <summary>
        /// Obtiene un comprobante
        /// </summary>
        /// <param name="comprobanteID">Codigo de comprobante</param>
        /// <param name="claseDeComprobanteID">Clase de comprobante (0 = cualquiera)</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseComprobante> GetItem(int comprobanteID = 0, int claseDeComprobanteID = 0)
            => Responder(() => comprobantes.GetItem(comprobanteID, claseDeComprobanteID));

        /// <summary>
        /// Crea un comprobante en el ERP
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<cResponseSaveComprobante> Create([FromBody] cComprobante? oComprobante)
            => Responder(() => comprobantes.Create(oComprobante));
    }
}

using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Finanzas;
using GESI.CORE.API.BO.V3.ERP.Finanzas;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de medios de pago
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class MediosDePagoController(IMediosDePagoServicio MediosDePagoMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de medios de pago
        /// </summary>
        /// <param name="tipo">Filtra por tipo de medio de pago</param>
        /// <param name="estado">Filtra por estado</param>
        /// <param name="divisaID">Se recibe por compatibilidad con la V2, que tampoco lo usaba (el Core no filtra por divisa)</param>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseMediosDePagos> GetList(string tipo = "",
                                                            int estado = 0,
                                                            int divisaID = 1,
                                                            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MediosDePagoMgr.GetList(tipo, estado, pageNumber, pageSize));

        /// <summary>
        /// Obtiene un medio de pago
        /// </summary>
        /// <param name="medioDePagoID">Codigo de medio de pago</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseMedioDePago> GetItem(int medioDePagoID = 0)
            => Responder(() => MediosDePagoMgr.GetItem(medioDePagoID));
    }
}

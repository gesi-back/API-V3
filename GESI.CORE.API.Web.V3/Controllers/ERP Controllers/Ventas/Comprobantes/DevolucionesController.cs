using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Ventas;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.ERP.Ventas.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de devoluciones de venta.
    /// </summary>
    [ApiController]
    [Route("api/Ventas/Comprobantes/[controller]")]
    [Produces("application/json")]
    public class DevolucionesController(IMovimientosDeClientesServicio MovimientosDeClientesMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene un listado de devoluciones
        /// </summary>
        [HttpPost("GetList")]
        public ActionResult<cResponseMovimientoDeClientes> GetList([FromBody] dParametrosConsultaComprobantes? parametros,
                                                                   int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                   int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeClientesMgr.GetList(parametros?.datosOperacion ?? new dParametrosConsultaDeComprobantesDatosDeLaOperacion(),
                                                                parametros?.datosCliente ?? new dParametrosConsultaDeComprobantesDatosDelCliente(),
                                                                SubTiposComprobantes.sDEV,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize));

        /// <summary>
        /// Crea devoluciones en el ERP
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<List<cResponseSaveMovimientosClientes>> Create([FromBody] List<cMovimientoDeCliente>? lstMovimientos)
            => ResponderLista(() => MovimientosDeClientesMgr.SaveComprobanteDeVenta(lstMovimientos, SubTiposComprobantes.sDEV, true),
                              error => new cResponseSaveMovimientosClientes { success = false, mensaje = error.message });
    }
}

using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Ventas;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.ERP.Ventas.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de presupuestos de venta.
    /// PENDIENTE: Getpdf (etapa 3).
    /// </summary>
    [ApiController]
    [Route("api/Ventas/Comprobantes/[controller]")]
    [Produces("application/json")]
    public class PresupuestosController(IMovimientosDeClientesServicio MovimientosDeClientesMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene presupuestos por clave
        /// </summary>
        [HttpPost("GetItem")]
        public ActionResult<cResponseMovimientoDeClientes> GetItem([FromBody] List<dClaveComprobanteDeVenta>? claves,
                                                                   string incluirDetalleDeVenta = "N",
                                                                   string incluirDetalleDeImpuesto = "N",
                                                                   string incluirRelacionesItems = "N",
                                                                   int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                   int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeClientesMgr.GetItem(claves,
                                                                SubTiposComprobantes.sPresupuesto,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize,
                                                                blIncluirDetalleDeVenta: incluirDetalleDeVenta,
                                                                blIncluirDetalleImpuestos: incluirDetalleDeImpuesto,
                                                                blIncluirRelacionesItems: incluirRelacionesItems));

        /// <summary>
        /// Obtiene un listado de presupuestos
        /// </summary>
        [HttpPost("GetList")]
        public ActionResult<cResponseMovimientoDeClientes> GetList([FromBody] dParametrosConsultaComprobantes? parametros,
                                                                   int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                   int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeClientesMgr.GetList(parametros?.datosOperacion ?? new dParametrosConsultaDeComprobantesDatosDeLaOperacion(),
                                                                parametros?.datosCliente,
                                                                SubTiposComprobantes.sPresupuesto,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize));

        /// <summary>
        /// Crea presupuestos en el ERP
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<List<cResponseSaveMovimientosClientes>> Create([FromBody] List<cMovimientoDeCliente>? body)
            => ResponderLista(() => MovimientosDeClientesMgr.SaveComprobanteDeVenta(body, SubTiposComprobantes.sPresupuesto, true),
                              error => new cResponseSaveMovimientosClientes { success = false, mensaje = error.message });
    }
}

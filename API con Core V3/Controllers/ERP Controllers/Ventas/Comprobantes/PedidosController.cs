using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Ventas;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.ERP.Ventas.V3.BO;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de pedidos de venta.
    /// PENDIENTE: Getpdf y GetPdf (etapa 3).
    /// </summary>
    [ApiController]
    [Route("api/Ventas/Comprobantes/[controller]")]
    [Produces("application/json")]
    public class PedidosController(IMovimientosDeClientesServicio MovimientosDeClientesMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene pedidos por clave
        /// </summary>
        [HttpPost("GetItem")]
        public ActionResult<cResponseMovimientoDeClientes> GetItem([FromBody] List<dClaveComprobanteDeVenta>? claves,
                                                                   string incluirDetalleDeVenta = "N",
                                                                   string incluirDetalleDeImpuesto = "N",
                                                                   string incluirRelacionesItems = "N",
                                                                   int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                   int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeClientesMgr.GetItem(claves,
                                                                SubTiposComprobantes.sNPE,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize,
                                                                blIncluirDetalleDeVenta: incluirDetalleDeVenta,
                                                                blIncluirDetalleImpuestos: incluirDetalleDeImpuesto,
                                                                blIncluirRelacionesItems: incluirRelacionesItems));

        /// <summary>
        /// Obtiene un listado de pedidos
        /// </summary>
        [HttpPost("GetList")]
        public ActionResult<cResponseMovimientoDeClientes> GetList([FromBody] dParametrosConsultaComprobantes? parametros,
                                                                   int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                   int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeClientesMgr.GetList(parametros?.datosOperacion ?? new dParametrosConsultaDeComprobantesDatosDeLaOperacion(),
                                                                parametros?.datosCliente,
                                                                SubTiposComprobantes.sNPE,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize));

        /// <summary>
        /// Obtiene un listado de pedidos (versión GET de la V2, con body)
        /// </summary>
        [HttpGet("v2/GetList")]
        public ActionResult<cResponseMovimientoDeClientes> GetListGet([FromBody] dParametrosConsultaComprobantes? parametros,
                                                                      int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                      int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeClientesMgr.GetList(parametros?.datosOperacion ?? new dParametrosConsultaDeComprobantesDatosDeLaOperacion(),
                                                                parametros?.datosCliente,
                                                                SubTiposComprobantes.sNPE,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize));

        /// <summary>
        /// Crea pedidos en el ERP. Acepta una lista de cMovimientoDeCliente o, igual que la V2, el formato de la API vieja (cComprobanteDePedidoAPI)
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<List<cResponseSaveMovimientosClientes>> Create([FromBody] JsonElement body)
            => ResponderLista(() => MovimientosDeClientesMgr.SaveComprobanteDeVenta(MovimientosDeClientesMgr.LeerComprobantesDePedido(body.GetRawText()),
                                                                                    SubTiposComprobantes.sNPE,
                                                                                    true),
                              error => new cResponseSaveMovimientosClientes { success = false, mensaje = error.message });
    }
}

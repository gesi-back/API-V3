using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Ventas;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.ERP.Ventas.V3.BO;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de comprobantes de cobro (recibos)
    /// </summary>
    [ApiController]
    [Route("api/Ventas/Comprobantes/[controller]")]
    [Produces("application/json")]
    public class CobrosController(IMovimientosDeClientesServicio MovimientosDeClientesMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene cobros por clave
        /// </summary>
        [HttpPost("GetItem")]
        public ActionResult<cResponseMovimientoDeClientes> GetItem([FromBody] List<dClaveComprobanteDeVenta>? claves,
                                                                   int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                   int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                                   string incluirDetalleDeAplicacion = "N",
                                                                   string incluirDetalleDeImpuestos = "N",
                                                                   string incluirDetalleDeMedioDePago = "N")
            => Responder(() => MovimientosDeClientesMgr.GetItem(claves,
                                                                SubTiposComprobantes.sRecibo,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize,
                                                                blIncluirRelacionesItems: incluirDetalleDeAplicacion,
                                                                blIncluirDetalleImpuestos: incluirDetalleDeImpuestos,
                                                                blIncluirDetalleDeMedioDePago: incluirDetalleDeMedioDePago));

        /// <summary>
        /// Obtiene un listado de cobros
        /// </summary>
        [HttpPost("GetList")]
        public ActionResult<cResponseMovimientoDeClientes> GetList([FromBody] dParametrosConsultaComprobantes? parametros,
                                                                   int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                   int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeClientesMgr.GetList(parametros?.datosOperacion ?? new dParametrosConsultaDeComprobantesDatosDeLaOperacion(),
                                                                parametros?.datosCliente ?? new dParametrosConsultaDeComprobantesDatosDelCliente(),
                                                                SubTiposComprobantes.sRecibo,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize));

        /// <summary>
        /// Crea cobros en el ERP con sus valores y aplicaciones. Acepta una lista de cComprobanteCobro o, igual que la V2, el formato de la API vieja (cComprobanteCobroAPI)
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<List<cResponseSaveMovimientosClientes>> Create([FromBody] JsonElement body)
            => ResponderLista(() => MovimientosDeClientesMgr.SaveComprobanteDeCobro(MovimientosDeClientesMgr.LeerComprobantesDeCobro(body.GetRawText())),
                              error => new cResponseSaveMovimientosClientes { success = false, mensaje = error.message });
    }
}

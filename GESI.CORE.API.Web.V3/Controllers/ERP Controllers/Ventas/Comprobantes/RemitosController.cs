using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Ventas;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.ERP.Ventas.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de remitos de venta.
    /// </summary>
    [ApiController]
    [Route("api/Ventas/Comprobantes/[controller]")]
    [Produces("application/json")]
    public class RemitosController(IMovimientosDeClientesServicio MovimientosDeClientesMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene remitos por clave
        /// </summary>
        [HttpPost("GetItem")]
        public ActionResult<cResponseMovimientoDeClientes> GetItem([FromBody] List<dClaveComprobanteDeVenta>? claves,
                                                                   string incluirDetalleDeVenta = "N",
                                                                   string incluirDetalleDeImpuesto = "N",
                                                                   string incluirRelacionesItems = "N",
                                                                   int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                   int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeClientesMgr.GetItem(claves,
                                                                SubTiposComprobantes.sRemito,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize,
                                                                blIncluirDetalleDeVenta: incluirDetalleDeVenta,
                                                                blIncluirDetalleImpuestos: incluirDetalleDeImpuesto,
                                                                blIncluirRelacionesItems: incluirRelacionesItems));

        /// <summary>
        /// Obtiene un listado de remitos
        /// </summary>
        [HttpPost("GetList")]
        public ActionResult<cResponseMovimientoDeClientes> GetList([FromBody] dParametrosConsultaComprobantes? parametros,
                                                                   int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                   int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeClientesMgr.GetList(parametros?.datosOperacion ?? new dParametrosConsultaDeComprobantesDatosDeLaOperacion(),
                                                                parametros?.datosCliente,
                                                                SubTiposComprobantes.sRemito,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize));

        /// <summary>
        /// Devuelve el link al PDF de los remitos del ERP (máximo 5 por llamada). Los PDF se buscan en el FTP de la empresa.
        /// Igual que la V2: GET con las claves en el body y un resultado por comprobante.
        /// </summary>
        [HttpPost("Getpdf")]
        public async Task<ActionResult<List<cPDFComprobante>>> Getpdf([FromBody] List<GESI.ERP.Core.V3.BO.Dinamicos.dClavesComprobante>? lstClaves)
            => await ResponderListaAsync(() => MovimientosDeClientesMgr.DevolverJSONPDF(lstClaves, SubTiposComprobantes.sRemito),
                                         error => new cPDFComprobante { success = false, error = error });
    }
}
using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Ventas;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.ERP.Ventas.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de notas de credito de venta.
    /// </summary>
    [ApiController]
    [Route("api/Ventas/Comprobantes/[controller]")]
    [Produces("application/json")]
    public class NotasDeCreditoController(IMovimientosDeClientesServicio MovimientosDeClientesMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene un listado de notas de credito
        /// </summary>
        [HttpPost("GetList")]
        public ActionResult<cResponseMovimientoDeClientes> GetList([FromBody] dParametrosConsultaComprobantes? parametros,
                                                                   int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                   int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeClientesMgr.GetList(parametros?.datosOperacion ?? new dParametrosConsultaDeComprobantesDatosDeLaOperacion(),
                                                                parametros?.datosCliente,
                                                                SubTiposComprobantes.sNCR,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize));

        /// <summary>
        /// Obtiene notas de credito por clave
        /// </summary>
        [HttpPost("GetItem")]
        public ActionResult<cResponseMovimientoDeClientes> GetItem([FromBody] List<dClaveComprobanteDeVenta>? claves,
                                                                   string incluirDetalleDeVenta = "N",
                                                                   string incluirDetalleDeImpuesto = "N",
                                                                   string incluirRelacionesItems = "N",
                                                                   string incluirDetalleDeMedioDePago = "N",
                                                                   int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                   int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeClientesMgr.GetItem(claves,
                                                                SubTiposComprobantes.sNCR,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize,
                                                                blIncluirDetalleDeVenta: incluirDetalleDeVenta,
                                                                blIncluirDetalleImpuestos: incluirDetalleDeImpuesto,
                                                                blIncluirRelacionesItems: incluirRelacionesItems,
                                                                blIncluirDetalleDeMedioDePago: incluirDetalleDeMedioDePago));

        /// <summary>
        /// Aplica saldos de notas de credito / recibos a facturas
        /// </summary>
        [HttpPost("AplicarSaldo/Save")]
        public ActionResult<List<cResponseSaveRelaciones>> SaveSaldo([FromBody] List<cRelacionComprobantes>? claves)
            => ResponderLista(() => MovimientosDeClientesMgr.SaveAplicacionesSaldos(claves),
                              error => new cResponseSaveRelaciones { success = false, error = error });

        /// <summary>
        /// Devuelve el link al PDF de las notas de crédito del ERP (máximo 5 por llamada). Los PDF se buscan en el FTP de la empresa.
        /// Igual que la V2: GET con las claves en el body y un resultado por comprobante.
        /// </summary>
        [HttpPost("Getpdf")]
        public async Task<ActionResult<List<cPDFComprobante>>> Getpdf([FromBody] List<GESI.ERP.Core.V3.BO.Dinamicos.dClavesComprobante>? lstClaves)
            => await ResponderListaAsync(() => MovimientosDeClientesMgr.DevolverJSONPDF(lstClaves, SubTiposComprobantes.sNCR),
                                         error => new cPDFComprobante { success = false, error = error });
    }
}
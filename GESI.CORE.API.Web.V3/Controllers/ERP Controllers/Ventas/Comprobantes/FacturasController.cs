using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Ventas;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.ERP.Ventas.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de facturas de venta.
    /// </summary>
    [ApiController]
    [Route("api/Ventas/Comprobantes/[controller]")]
    [Produces("application/json")]
    public class FacturasController(IMovimientosDeClientesServicio MovimientosDeClientesMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene facturas por clave
        /// </summary>
        [HttpPost("GetItem")]
        public ActionResult<cResponseMovimientoDeClientes> GetItem([FromBody] List<dClaveComprobanteDeVenta>? claves,
                                                                   string incluirDetalleDeVenta = "N",
                                                                   string incluirDetalleDeImpuesto = "N",
                                                                   string incluirRelacionesItems = "N",
                                                                   string incluirDetalleDeMedioDePago = "N",
                                                                   string incluirDetalleDeVencimientos = "N",
                                                                   int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                   int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeClientesMgr.GetItem(claves,
                                                                SubTiposComprobantes.sFactura,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize,
                                                                blIncluirDetalleDeVenta: incluirDetalleDeVenta,
                                                                blIncluirDetalleImpuestos: incluirDetalleDeImpuesto,
                                                                blIncluirRelacionesItems: incluirRelacionesItems,
                                                                blIncluirDetalleDeMedioDePago: incluirDetalleDeMedioDePago,
                                                                blincluirDetalleDeVencimientos: incluirDetalleDeVencimientos));

        /// <summary>
        /// Obtiene un listado de facturas
        /// </summary>
        [HttpPost("GetList")]
        public ActionResult<cResponseMovimientoDeClientes> GetList([FromBody] dParametrosConsultaComprobantes? parametros,
                                                                   int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                   int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeClientesMgr.GetList(parametros?.datosOperacion ?? new dParametrosConsultaDeComprobantesDatosDeLaOperacion(),
                                                                parametros?.datosCliente ?? new dParametrosConsultaDeComprobantesDatosDelCliente(),
                                                                SubTiposComprobantes.sFactura,
                                                                pageNumber: pageNumber,
                                                                pageSize: pageSize));

        /// <summary>
        /// Cambia el estado de facturas
        /// </summary>
        [HttpPut("SetearEstado")]
        public ActionResult<cResponse> SetState([FromBody] List<dClaveComprobanteDeVenta>? lstClaves, int estadoID = 0)
            => Responder(() => MovimientosDeClientesMgr.SetearEstadoComprobante(lstClaves, estadoID));

        /// <summary>
        /// Devuelve el link al PDF de las facturas del ERP (máximo 5 por llamada). Los PDF se buscan en el FTP de la empresa.
        /// Igual que la V2: GET con las claves en el body y un resultado por comprobante.
        /// </summary>
        [HttpPost("Getpdf")]
        public async Task<ActionResult<List<cPDFComprobante>>> Getpdf([FromBody] List<GESI.ERP.Core.V3.BO.Dinamicos.dClavesComprobante>? lstClaves)
            => await ResponderListaAsync(() => MovimientosDeClientesMgr.DevolverJSONPDF(lstClaves, SubTiposComprobantes.sFactura),
                                         error => new cPDFComprobante { success = false, error = error });
    }
}
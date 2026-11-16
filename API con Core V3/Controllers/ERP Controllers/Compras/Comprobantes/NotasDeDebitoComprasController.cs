using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Compras;
using GESI.CORE.API.BO.V3.ERP.Compras;
using GESI.CORE.V3.BLL;
using GESI.ERP.Core.V3.BO.Dinamicos;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de notas de debito de compra.
    /// La clase se llama NotasDeDebitoComprasController para no chocar con el de Ventas; la ruta es la misma de la V2.
    /// </summary>
    [ApiController]
    [Route("api/Compras/Comprobantes/NotasDeDebito")]
    [Produces("application/json")]
    public class NotasDeDebitoComprasController(IMovimientosDeProveedoresServicio MovimientosDeProveedoresMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene un listado de notas de debito de compra
        /// </summary>
        [HttpPost("GetList")]
        public ActionResult<cResponseMovimientoProveedores> GetList([FromBody] dParametrosConsultaComprobantesCompra? parametros,
                                                                    int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                    int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeProveedoresMgr.GetList(parametros,
                                                                   (int)ClaseDeComprobante.Compras_NotaDeDebito,
                                                                   pageNumber,
                                                                   pageSize));

        /// <summary>
        /// Obtiene notas de debito de compra por clave, con su detalle
        /// </summary>
        [HttpPost("GetItem")]
        public ActionResult<cResponseMovimientoProveedores> GetItem([FromBody] List<dClavesComprobante>? claves,
                                                                    string incluirDetalle = "N",
                                                                    string incluirRelacionesItems = "N",
                                                                    string incluirPagosProveedores = "N",
                                                                    string incluirMediosDePago = "N",
                                                                    int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                    int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                                    string incluirVencimientos = "N")
            => Responder(() => MovimientosDeProveedoresMgr.GetItem(claves,
                                                                   (int)ClaseDeComprobante.Compras_NotaDeDebito,
                                                                   incluirDetalle.Equals("S"),
                                                                   incluirRelacionesItems.Equals("S"),
                                                                   incluirPagosProveedores.Equals("S"),
                                                                   incluirMediosDePago.Equals("S"),
                                                                   incluirVencimientos.Equals("S"),
                                                                   pageNumber,
                                                                   pageSize));
    }
}

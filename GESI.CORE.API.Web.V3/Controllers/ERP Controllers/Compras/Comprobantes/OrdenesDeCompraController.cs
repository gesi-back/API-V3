using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Compras;
using GESI.CORE.API.BO.V3.ERP.Compras;
using GESI.CORE.V3.BLL;
using GESI.ERP.Core.V3.BO.Dinamicos;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de ordenes de compra
    /// </summary>
    [ApiController]
    [Route("api/Compras/Comprobantes/OrdenesDeCompra")]
    [Produces("application/json")]
    public class OrdenesDeCompraController(IMovimientosDeProveedoresServicio MovimientosDeProveedoresMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene un listado de ordenes de compra
        /// </summary>
        [HttpPost("GetList")]
        public ActionResult<cResponseMovimientoProveedores> GetList([FromBody] dParametrosConsultaComprobantesCompra? parametros,
                                                                    int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                    int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeProveedoresMgr.GetList(parametros,
                                                                   (int)ClaseDeComprobante.Compras_NotaDePedido,
                                                                   pageNumber,
                                                                   pageSize));

        /// <summary>
        /// Obtiene ordenes de compra por clave, con su detalle.
        /// Igual que la V2: este endpoint no tiene incluirVencimientos y la paginación informada usa los valores por defecto.
        /// </summary>
        [HttpPost("GetItem")]
        public ActionResult<cResponseMovimientoProveedores> GetItem([FromBody] List<dClavesComprobante>? claves,
                                                                    string incluirDetalle = "N",
                                                                    string incluirRelacionesItems = "N",
                                                                    string incluirPagosProveedores = "N",
                                                                    string incluirMediosDePago = "N",
                                                                    int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                    int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => MovimientosDeProveedoresMgr.GetItem(claves,
                                                                   (int)ClaseDeComprobante.Compras_NotaDePedido,
                                                                   incluirDetalle.Equals("S"),
                                                                   incluirRelacionesItems.Equals("S"),
                                                                   incluirPagosProveedores.Equals("S"),
                                                                   incluirMediosDePago.Equals("S")));

        /// <summary>
        /// Crea ordenes de compra en el ERP
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<List<cResponseSaveMovimientosProveedores>> Create([FromBody] List<GESI.ERP.Compras.V3.BO.cMovimientoProveedor>? ordenesDeCompra)
            => ResponderLista(() => MovimientosDeProveedoresMgr.Create(ordenesDeCompra),
                              error => new cResponseSaveMovimientosProveedores { success = false, mensaje = error.message });
    }
}

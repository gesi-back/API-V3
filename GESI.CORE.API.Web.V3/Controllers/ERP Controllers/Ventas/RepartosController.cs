using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Ventas;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.ERP.Ventas.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de fojas de reparto
    /// </summary>
    [ApiController]
    [Route("api/Ventas/[controller]")]
    [Produces("application/json")]
    public class RepartosController(IRepartosServicio RepartosMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene un listado de fojas de reparto
        /// </summary>
        /// <param name="repartidorID">Filtra por repartidor</param>
        /// <param name="zonaID">Filtra por zona</param>
        /// <param name="fechaDesde">Filtra las fojas desde una fecha</param>
        /// <param name="estado">A = abiertas, C = cerradas</param>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseFojasDeRepartos> GetList(int repartidorID = 0,
                                                              int zonaID = 0,
                                                              string? fechaDesde = null,
                                                              string? estado = null,
                                                              int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                              int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => RepartosMgr.GetList(repartidorID, zonaID, fechaDesde, estado, pageNumber, pageSize));

        /// <summary>
        /// Obtiene una foja de reparto con sus comprobantes asignados
        /// </summary>
        /// <param name="empresaID">Opcional; si viene tiene que ser la empresa de la sesión</param>
        /// <param name="fojaID">Codigo de foja</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseFojaDeReparto> GetFojaReparto(int empresaID = 0, int fojaID = 0)
            => Responder(() => RepartosMgr.ObtenerFojaReparto(empresaID, fojaID));

        /// <summary>
        /// Indica si un producto está en la foja
        /// </summary>
        [HttpGet("GetProducto")]
        public ActionResult<cResponseGetProductoEncontradoFoja> GetProducto(int empresaID = 0, int fojaID = 0, string productoID = "")
            => Responder(() => RepartosMgr.GetProductoFoja(empresaID, fojaID, productoID));

        /// <summary>
        /// Obtiene los comprobantes que no están asignados a ninguna foja
        /// </summary>
        [HttpGet("GetComprobantesNoAsignados")]
        public ActionResult<cResponseMovimientoDeClientes> GetComprobantesNoAsignados(string fechadesde = "",
                                                                                      string fechahasta = "",
                                                                                      int vendedorID = 0,
                                                                                      int repartidorID = 0,
                                                                                      int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                                      int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => RepartosMgr.GetComprobantesNoAsignados(fechadesde, fechahasta, vendedorID, repartidorID, pageNumber, pageSize));

        /// <summary>
        /// Compara los productos y cantidades cargados contra la foja
        /// </summary>
        [HttpPost("CompararProductos")]
        public ActionResult<cResponseCompararProductos> CompararProductos(int empresaID = 0,
                                                                          int fojaID = 0,
                                                                          [FromBody] Dictionary<string, decimal>? dicProductosYCantidades = null)
            => Responder(() => RepartosMgr.CompararProductos(empresaID, fojaID, dicProductosYCantidades));

        /// <summary>
        /// Cierra la carga de la foja con los productos controlados y un comentario
        /// </summary>
        [HttpPost("CerrarCargaFoja")]
        public ActionResult<cResponseCompararProductos> CerrarCargaFoja(int empresaID = 0, int fojaID = 0, [FromBody] cCierreFoja? oCierreFoja = null)
            => Responder(() => RepartosMgr.CerrarCargaFoja(empresaID, fojaID, oCierreFoja));

        /// <summary>
        /// Crea fojas de reparto asignándoles comprobantes
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<cResponseFojasDeRepartosSave> CreateFojaReparto([FromBody] List<cFojaDeReparto>? listaFojas)
            => Responder(() => RepartosMgr.Create(listaFojas));
    }
}

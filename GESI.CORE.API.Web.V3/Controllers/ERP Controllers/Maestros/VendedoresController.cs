using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de vendedores
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class VendedoresController(IVendedoresServicio vendedores) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene un listado de vendedores dentro del ERP
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseVendedores> GetList(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => vendedores.GetList(pageNumber, pageSize));

        /// <summary>
        /// Obtiene un vendedor del ERP
        /// </summary>
        /// <param name="vendedorID">Codigo de vendedor</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseVendedor> GetItem(int vendedorID = 0)
            => Responder(() => vendedores.GetItem(vendedorID));

        /// <summary>
        /// Obtiene el listado de descuentos de vendedores por aplicacion
        /// </summary>
        /// <param name="canalDeVentaID">Filtra por canal de venta (0 = todos)</param>
        /// <param name="vendedorID">Filtra por vendedor (0 = todos)</param>
        /// <param name="aplicacionID">Filtra por aplicacion (0 = todas)</param>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetDescuentosXAplicacion")]
        public ActionResult<cResponseDescuentosVendedoresXAplicacion> GetListDescuentos(
            int canalDeVentaID = 0,
            int vendedorID = 0,
            int aplicacionID = 0,
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => vendedores.GetListDescuentos(canalDeVentaID, aplicacionID, vendedorID, pageNumber, pageSize));
    }
}

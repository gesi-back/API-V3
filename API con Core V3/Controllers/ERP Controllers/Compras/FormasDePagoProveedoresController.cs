using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Compras;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de formas de pago de proveedores
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class FormasDePagoProveedoresController(IFormasDePagoProveedoresServicio FormasDePagoProveedoresMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de formas de pago de proveedores
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="ordenarPor">Campo por el que se ordena la lista</param>
        /// <param name="direccion">Tipo de ordenamiento (asc, desc)</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseFormasDePago> GetList(int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                           int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                           string? ordenarPor = null,
                                                           string? direccion = null)
            => Responder(() => FormasDePagoProveedoresMgr.GetList(pageNumber, pageSize, ordenarPor, direccion));

        /// <summary>
        /// Obtiene una forma de pago de proveedores
        /// </summary>
        /// <param name="formaDePagoID">Codigo de forma de pago</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseFormaDePago> GetItem(int formaDePagoID = 0)
            => Responder(() => FormasDePagoProveedoresMgr.GetItem(formaDePagoID));
    }
}

using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de proveedores
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class ProveedoresController(IProveedoresServicio ProveedoresMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene un listado de proveedores del ERP
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseProveedores> GetList(int pageNumber = 1, int pageSize = 10)
            => Responder(() => ProveedoresMgr.GetList(pageNumber, pageSize));

        /// <summary>
        /// Obtiene un proveedor del ERP
        /// </summary>
        /// <param name="proveedorid">Codigo de proveedor</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseProveedor> GetItem(int proveedorid = 0)
            => Responder(() => ProveedoresMgr.GetItem(proveedorid));
    }
}

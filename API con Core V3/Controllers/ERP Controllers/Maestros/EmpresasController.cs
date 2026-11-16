using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de empresas
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class EmpresasController(IEmpresasServicio empresas) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene las empresas habilitadas para el usuario
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="datosCompletos">S = todos los datos de la empresa; N (predeterminado) = solo IDs y descripciones</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseEmpresas> GetEmpresas(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
            string datosCompletos = "N")
            => Responder(() => empresas.GetList(pageNumber, pageSize, datosCompletos));
    }
}

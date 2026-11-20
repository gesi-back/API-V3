using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de tipos de operacion
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class TiposDeOperacionController(ITiposDeOperacionServicio tiposDeOperacion) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene el listado de tipos de operacion del ERP
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="ordenarPor">Define el campo por el que se ordena la lista</param>
        /// <param name="direccion">Define el tipo de ordenamiento de la lista</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseTiposDeOperacion> GetList(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
            string? ordenarPor = null,
            string? direccion = null)
            => Responder(() => tiposDeOperacion.GetList(pageNumber, pageSize, ordenarPor, direccion));

        /// <summary>
        /// Obtiene un tipo de operacion del ERP
        /// </summary>
        /// <param name="tipoDeOperacionID">Codigo de tipo de operacion</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseTipoDeOperacion> GetItem(int tipoDeOperacionID = 0)
            => Responder(() => tiposDeOperacion.GetItem(tipoDeOperacionID));

        /// <summary>
        /// Crea uno o varios tipos de operacion dentro del ERP
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<List<cResponseTipoDeOperacionSave>> Create([FromBody] List<cTipoDeOperacion>? oTipoOperacion = null)
            => ResponderLista(() => tiposDeOperacion.Save(oTipoOperacion, alta: true),
                              error => new cResponseTipoDeOperacionSave(null, false, error.message));

        /// <summary>
        /// Actualiza uno o varios tipos de operacion dentro del ERP
        /// </summary>
        [HttpPut("Update")]
        public ActionResult<List<cResponseTipoDeOperacionSave>> Update([FromBody] List<cTipoDeOperacion>? oTipoOperacion = null)
            => ResponderLista(() => tiposDeOperacion.Save(oTipoOperacion, alta: false),
                              error => new cResponseTipoDeOperacionSave(null, false, error.message));

        /// <summary>
        /// Elimina uno o varios tipos de operacion dentro del ERP
        /// </summary>
        /// <param name="codigos">Codigos de tipos de operacion separados por coma</param>
        [HttpDelete("Delete")]
        public ActionResult<cResponse> Delete(string? codigos = "")
            => Responder(() => tiposDeOperacion.Delete(codigos));
    }
}

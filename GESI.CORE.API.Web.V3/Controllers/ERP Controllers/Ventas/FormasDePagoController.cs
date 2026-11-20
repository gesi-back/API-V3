using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Ventas;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.ERP.Core.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de formas de pago
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class FormasDePagoController(IFormasDePagoServicio formasDePago) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene la lista de formas de pago del ERP
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="ordenarPor">Define el campo por el que se ordena la lista</param>
        /// <param name="direccion">Define el tipo de ordenamiento de la lista</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseFormasDePago> GetList(
            int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
            int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
            string? ordenarPor = null,
            string? direccion = null)
            => Responder(() => formasDePago.GetList(pageNumber, pageSize, ordenarPor, direccion));

        /// <summary>
        /// Obtiene una forma de pago del ERP
        /// </summary>
        /// <param name="formaDePagoID">Codigo de forma de pago</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseFormaDePago> GetItem(int formaDePagoID = 0)
            => Responder(() => formasDePago.GetItem(formaDePagoID));

        /// <summary>
        /// Crea una o varias formas de pago dentro del ERP
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<List<cResponseFormaDePagoSave>> Create(List<cFormaDePago>? oFormaDePago = null)
            => ResponderLista(() => formasDePago.Save(oFormaDePago, alta: true),
                              error => new cResponseFormaDePagoSave(null, false, error.message));

        /// <summary>
        /// Actualiza una o varias formas de pago dentro del ERP
        /// </summary>
        [HttpPut("Update")]
        public ActionResult<List<cResponseFormaDePagoSave>> Update(List<cFormaDePago>? oFormaDePago = null)
            => ResponderLista(() => formasDePago.Save(oFormaDePago, alta: false),
                              error => new cResponseFormaDePagoSave(null, false, error.message));

        /// <summary>
        /// Elimina una o varias formas de pago dentro del ERP
        /// </summary>
        /// <param name="codigos">Codigos de formas de pago separados por coma</param>
        [HttpDelete("Delete")]
        public ActionResult<cResponse> Delete(string? codigos)
            => Responder(() => formasDePago.Delete(codigos));
    }
}

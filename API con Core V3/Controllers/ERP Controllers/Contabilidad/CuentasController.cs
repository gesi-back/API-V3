using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Contabilidad;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Contabilidad;
using GESI.Contabilidad.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de cuentas contables
    /// </summary>
    [ApiController]
    [Route("api/Maestros/Contabilidad/[controller]")]
    [Produces("application/json")]
    public class CuentasController(ICuentasServicio CuentasMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene las cuentas contables de un ejercicio
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="ejercicioID">Codigo de ejercicio</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseCuentas> GetList(int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                      int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                      int ejercicioID = 0)
            => Responder(() => CuentasMgr.GetList(ejercicioID, pageNumber, pageSize));

        /// <summary>
        /// Elimina una cuenta contable
        /// </summary>
        /// <param name="ejercicioID">Codigo de ejercicio</param>
        /// <param name="cuentaID">Codigo de cuenta</param>
        [HttpDelete("Delete")]
        public ActionResult<cResponse> Delete(int ejercicioID = 0, string? cuentaID = "")
            => Responder(() => CuentasMgr.Delete(ejercicioID, cuentaID));

        /// <summary>
        /// Crea una cuenta contable
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<cResponseCuenta> Save([FromBody] cCuenta? oCuenta)
            => Responder(() => CuentasMgr.Save(oCuenta, true));

        /// <summary>
        /// Modifica una cuenta contable (requiere CuentaID)
        /// </summary>
        [HttpPut("Update")]
        public ActionResult<cResponseCuenta> Update([FromBody] cCuenta? oCuenta)
            => Responder(() => CuentasMgr.Save(oCuenta, false));
    }
}

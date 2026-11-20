using GESI.Contabilidad.V3.BO;
using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Contabilidad;
using GESI.CORE.API.BO.V3.ERP.Contabilidad;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GESI.CORE.API.Web.V3.Controllers.ERP_Controllers.Contabilidad
{
    [Route("api/Maestros/Contabilidad/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ReportesController(IReportesContables reportesContablesBLL) : ApiControllerBase
    {
        /// <summary>
        /// Libro diario del ejercicio
        /// </summary>
        /// <param name="ejercicioID"></param>
        /// <param name="desde"></param>
        /// <param name="hasta"></param>
        /// <returns></returns>
        [HttpGet("LibroDiario")]
        public ActionResult<cResponseLibroDiario> GetLibroDiario(int ejercicioID = 0, DateTime? desde = null,DateTime? hasta = null)
           => Responder(() => reportesContablesBLL.GetLibroDiario(ejercicioID,desde,hasta));

        /// <summary>
        /// Mayores de las cuentas contables dentro del ejercicio
        /// </summary>
        /// <param name="ejercicioID"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="desde"></param>
        /// <param name="hasta"></param>
        /// <param name="cuentasIDs"></param>
        /// <returns></returns>
        [HttpGet("Mayores")]
        public ActionResult<cResponseMayores> GetMayores(int ejercicioID = 0,int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber, int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize, DateTime? desde = null, DateTime? hasta = null,string? cuentasIDs = null)
           => Responder(() => reportesContablesBLL.GetMayor(ejercicioID,pageNumber,pageSize,desde,hasta,cuentasIDs));

        /// <summary>
        /// Balance de sumas y saldos del ejercicio
        /// </summary>
        /// <param name="ejercicioID">Ejercicio contable (obligatorio)</param>
        /// <param name="desde">Fecha desde. Vacío = inicio del ejercicio</param>
        /// <param name="hasta">Fecha hasta. Vacío = fin del ejercicio</param>
        /// <param name="incluirCuentasTotalizadoras">"S" incluye las cuentas no imputables (totalizadoras). Por defecto "N"</param>
        /// <param name="asientosExcluidos">Asientos que no se tienen en cuenta, separados por coma. Ej: 1,25</param>
        /// <param name="asientoDeAperturaID">Asiento de apertura del ejercicio</param>
        [HttpGet("BalanceDeSumasYSaldos")]
        public ActionResult<cResponseBalanceDeSumasYSaldos> GetBalanceDeSumasYSaldos(int ejercicioID = 0, DateTime? desde = null, DateTime? hasta = null, string incluirCuentasTotalizadoras = "N", string? asientosExcluidos = null, int? asientoDeAperturaID = null)
           => Responder(() => reportesContablesBLL.GetBalanceDeSumasYSaldos(ejercicioID, desde, hasta, incluirCuentasTotalizadoras, asientosExcluidos, asientoDeAperturaID));

        /// <summary>
        /// Balance general del ejercicio, agrupado por rubros
        /// </summary>
        /// <param name="ejercicioID">Ejercicio contable (obligatorio)</param>
        /// <param name="hasta">Fecha de corte. Vacío = fin del ejercicio</param>
        /// <param name="cuentasRaiz">Cuentas raíz de los rubros a mostrar, separadas por coma. Vacío = todas</param>
        /// <param name="nivelMaximo">Nivel máximo de cuentas a mostrar. Vacío = todos</param>
        /// <param name="ocultarCuentasSinSaldo">"N" muestra también las cuentas sin saldo. Por defecto "S"</param>
        /// <param name="asientosExcluidos">Asientos que no se tienen en cuenta, separados por coma. Ej: 1,25</param>
        [HttpGet("BalanceGeneral")]
        public ActionResult<cResponseBalanceGeneral> GetBalanceGeneral(int ejercicioID = 0, DateTime? hasta = null, string? cuentasRaiz = null, int? nivelMaximo = null, string ocultarCuentasSinSaldo = "S", string? asientosExcluidos = null)
           => Responder(() => reportesContablesBLL.GetBalanceGeneral(ejercicioID, hasta, cuentasRaiz, nivelMaximo, ocultarCuentasSinSaldo, asientosExcluidos));
    }
}

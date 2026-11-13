using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BO.V3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Runtime.CompilerServices;

namespace API_con_Core_V3.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Ejecuta la acción y, si falla, devuelve el mismo tipo de respuesta con el error cargado.
        /// </summary>
        protected ActionResult<T> Responder<T>(Func<T> accion, [CallerMemberName] string endpoint = "")
            where T : cResponse, new()
        {
            try
            {
                return Ok(accion());
            }
            catch (Exception ex)
            {
                var (status, error) = ArmarError(ex, endpoint);
                return StatusCode((int)status, new T { success = false, error = error });
            }
        }

        /// <summary>Versión async, para servicios que devuelven Task.</summary>
        protected async Task<ActionResult<T>> ResponderAsync<T>(Func<Task<T>> accion, [CallerMemberName] string endpoint = "")
            where T : cResponse, new()
        {
            try
            {
                return Ok(await accion());
            }
            catch (Exception ex)
            {
                var (status, error) = ArmarError(ex, endpoint);
                return StatusCode((int)status, new T { success = false, error = error });
            }
        }

        /// <summary>
        /// Para endpoints que devuelven una lista de resultados por ítem (ej. Create/Update en lote).
        /// Si falla todo el request, devuelve una lista con un único ítem armado por <paramref name="itemDeError"/>.
        /// </summary>
        protected ActionResult<List<TItem>> ResponderLista<TItem>(Func<List<TItem>> accion,
                                                                  Func<cError, TItem> itemDeError,
                                                                  [CallerMemberName] string endpoint = "")
        {
            try
            {
                return Ok(accion());
            }
            catch (Exception ex)
            {
                var (status, error) = ArmarError(ex, endpoint);
                return StatusCode((int)status, new List<TItem> { itemDeError(error) });
            }
        }

        /// <summary>
        /// Traduce la excepción a status HTTP + cError y la loguea.
        /// </summary>
        private (HttpStatusCode status, cError error) ArmarError(Exception ex, string endpoint)
        {
            // Las BLL envuelven todo en ApplicationException: el error real está en InnerException
            var real = ex is ApplicationException { InnerException: not null } ? ex.InnerException! : ex;

            var (status, codigo, apiError) = real switch
            {
                UnauthorizedAccessException
                    => (HttpStatusCode.Forbidden, (int)cCodigosError.cPermisoDenegadoCostos, ApiErrors.errNoAutorizado),
                cCodigoNoDetalladoException
                    => (HttpStatusCode.BadRequest, (int)cCodigosError.cCodigoDetalladoEnLaSolicitud, ApiErrors.errParametroInvalido),
                cFormatoInvalidoException
                    => (HttpStatusCode.BadRequest, (int)cCodigosError.cErrorConversionDato, ApiErrors.errParametroInvalido),
                ArgumentException or FormatException
                    => (HttpStatusCode.BadRequest, (int)cCodigosError.cModeloNoValido, ApiErrors.errJsonInvalido),
                cDatosNoEncontradosException or KeyNotFoundException
                    => (HttpStatusCode.NotFound, (int)cCodigosError.cDatosNoEncontrados, ApiErrors.errDatosNoEncontrados),
                SqlException
                    => (HttpStatusCode.InternalServerError, (int)cCodigosError.cErrorInternoAplicacion, ApiErrors.errInternalServerError),
                _ => (HttpStatusCode.InternalServerError, (int)cCodigosError.cErrorInternoAplicacion, ApiErrors.errInternalServerError),
            };

            var esDesarrollo = HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment();
            var mensaje = status == HttpStatusCode.InternalServerError && !esDesarrollo
                ? "Ocurrió un error interno al procesar la solicitud."   // en producción no mostramos el detalle técnico
                : real.Message;

            var sesion = HttpContext.RequestServices.GetRequiredService<IApiSessionManager>();
            HttpContext.RequestServices.GetRequiredService<IApiLogger>().LoguearSucesosAPI(
                $"{endpoint}: {real.Message}", tTiposDeError.tError,
                sesion.Iniciada ? sesion.Usuario : "", Request.Path.Value,
                sesion.Iniciada ? sesion.EmpresaID : 0, sesion.Iniciada ? sesion.SucursalID : 0,
                codigoErrorInterno: codigo,
                parametros: Request.QueryString.ToString(),
                IPRemoteDesktop: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                IPPublica: Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "",
                Agent: Request.Headers.UserAgent.ToString());

            return (status, new cError(codigo, mensaje, apiError));
        }
    }
}

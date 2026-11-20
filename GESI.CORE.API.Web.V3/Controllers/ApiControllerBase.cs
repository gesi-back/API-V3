using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Runtime.CompilerServices;

namespace GESI.CORE.API.Web.V3.Controllers
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

        /// <summary>Versión async de ResponderLista, para servicios que devuelven Task (ej. los PDF).</summary>
        protected async Task<ActionResult<List<TItem>>> ResponderListaAsync<TItem>(Func<Task<List<TItem>>> accion,
                                                                               Func<cError, TItem> itemDeError,
                                                                               [CallerMemberName] string endpoint = "")
        {
            try
            {
                return Ok(await accion());
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
            var apiLogger = HttpContext.RequestServices.GetRequiredService<IApiLogger>();

            string usuario = sesion.Iniciada ? sesion.Usuario : "";
            int empresaID = sesion.Iniciada ? sesion.EmpresaID : 0;
            string ruta = Request.Path.Value ?? endpoint;
            string parametros = Request.QueryString.ToString();

            apiLogger.LoguearSucesosAPI(
                $"{endpoint}: {real.Message}", tTiposDeError.tError,
                usuario, ruta,
                empresaID, sesion.Iniciada ? sesion.SucursalID : 0,
                codigoErrorInterno: codigo,
                parametros: parametros,
                IPRemoteDesktop: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                IPPublica: Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "",
                Agent: Request.Headers.UserAgent.ToString());

            // Solo los errores 500 (errores internos, no los de datos del cliente) avisan por notificación remota, como la V2.
            // No se espera el envío: la respuesta al cliente no se demora aunque el servidor de notificaciones tarde o esté caído.
            // Igual que la V2: los errores de permisos (mensaje con "10100") no avisan aunque lleguen como error genérico.
            if (status == HttpStatusCode.InternalServerError && !real.Message.Contains("10100"))
            {
                string titulo = "Error API EmpresaID:" + empresaID + " Fecha:" + DateTime.Now;
                string mensajeNotificacion = real.Message + " Endpoint:" + ruta + " Parametros: " + parametros + " Usuario: " + usuario;

                _ = Task.Run(() => EnviarNotificacionRemotaAsync(titulo, mensajeNotificacion, usuario, ruta, empresaID, apiLogger));
            }

            return (status, new cError(codigo, mensaje, apiError));
        }

        /// <summary>
        /// Envía el aviso de error al servicio de notificaciones (Url y SharedSecret salen del archivo de secretos).
        /// Recibe el logger ya resuelto porque corre después de que el request terminó: no puede usar HttpContext.
        /// Nunca lanza excepciones: si algo falla, solo queda en el log.
        /// </summary>
        private static async Task EnviarNotificacionRemotaAsync(string titulo, string mensaje,
                                                                string usuario, string endpoint, int empresaID,
                                                                IApiLogger apiLogger)
        {
            string? ruta = null;

            try
            {
                ruta = RutaSecrets.Value;
                string? url = null;
                string? sharedSecret = null;

                // Las claves secretas del Core solo existen en Windows (la API corre en IIS).
                // En otra plataforma quedan vacías y se registra en el log, igual que si faltaran.
                if (OperatingSystem.IsWindows())
                {
                    url = GESI.CORE.V3.SEC.Seguridad.UserSecretGet("Url", ruta);
                    sharedSecret = GESI.CORE.V3.SEC.Seguridad.UserSecretGet("SharedSecret", ruta);
                }

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(sharedSecret))
                {
                    apiLogger.LoguearSucesosAPI(
                        "No se encontraron claves secretas. Ruta: " + (ruta ?? GESI.CORE.V3.SEC.Seguridad.UserSecretsRutaPorDefecto) +
                        " | Usuario: " + Environment.UserName +
                        " | Url: " + (string.IsNullOrEmpty(url) ? "NO" : "SI") +
                        " | SharedSecret: " + (string.IsNullOrEmpty(sharedSecret) ? "NO" : "SI"),
                        tTiposDeError.tInformativo, usuario, endpoint, empresaID, 0);
                    return;
                }

                await GESI.CORE.V3.Utilidades.NotificacionRemota
                       .SendAsync(url, sharedSecret, titulo, mensaje, new Dictionary<string, object?>())
                       .ConfigureAwait(false);

                // Igual que la V2: queda registrado que el aviso salió
                apiLogger.LoguearSucesosAPI("Notificación remota enviada a: " + url,
                    tTiposDeError.tInformativo, usuario, endpoint, empresaID, 0);
            }
            catch (Exception exw)
            {
                apiLogger.LoguearSucesosAPI("Error al enviar notificación remota: " + exw.Message +
                    " | Ruta secretos: " + (ruta ?? GESI.CORE.V3.SEC.Seguridad.UserSecretsRutaPorDefecto),
                    tTiposDeError.tInformativo, usuario, endpoint, empresaID, 0);
            }
        }

        private static readonly Lazy<string?> RutaSecrets = new(() =>
        {
            try
            {
                return new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build()
                    .GetSection("AppSettings")["RutaSecrets"];
            }
            catch
            {
                return null;   // usa la ruta por defecto de UserSecretGet
            }
        });
    }
}

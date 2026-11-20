using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace GESI.CORE.API.Web.V3.Middleware
{
    /// <summary>
    /// Límite de solicitudes por minuto por IP (reemplaza al CustomIpRateLimitMiddleware de la V2).
    /// Usa el rate limiting que trae ASP.NET Core, sin paquetes externos.
    ///
    /// Se configura en appsettings.json:
    ///   "RateLimit": { "SolicitudesPorMinuto": 1000 }
    /// Con 0 o sin la sección, no hay límite. Si se cambia el valor hay que reiniciar la API.
    ///
    /// Al excederse responde 429 con un cResponse y el cError, igual que la V2.
    /// </summary>
    public static class LimiteDeSolicitudes
    {
        public const string Seccion = "RateLimit";

        /// <summary>
        /// Registra el límite leyendo la configuración. En Program.cs: builder.Services.AddLimiteDeSolicitudes(builder.Configuration)
        /// </summary>
        public static IServiceCollection AddLimiteDeSolicitudes(this IServiceCollection services, IConfiguration config)
        {
            int solicitudesPorMinuto = config.GetValue<int>(Seccion + ":SolicitudesPorMinuto");

            services.AddRateLimiter(options =>
            {
                // Sin límite configurado no se agrega ninguna restricción
                if (solicitudesPorMinuto > 0)
                {
                    // Una ventana de 1 minuto por cada IP: cuenta las solicitudes y al minuto vuelve a cero
                    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "sin-ip",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = solicitudesPorMinuto,
                                Window = TimeSpan.FromMinutes(1),
                                QueueLimit = 0   // las que exceden se rechazan, no quedan esperando
                            }));
                }

                options.OnRejected = (contexto, cancellationToken) => ResponderLimiteExcedido(contexto.HttpContext, solicitudesPorMinuto, cancellationToken);
            });

            return services;
        }

        /// <summary>
        /// Respuesta al exceder el límite: 429 + cResponse con el cError (mismo formato que la V2)
        /// </summary>
        private static async ValueTask ResponderLimiteExcedido(HttpContext httpContext, int solicitudesPorMinuto, CancellationToken cancellationToken)
        {
            string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            // Queda registrado en el log de la API para poder ver quién está llegando al límite
            IApiLogger apiLogger = httpContext.RequestServices.GetRequiredService<IApiLogger>();
            apiLogger.LoguearSucesosAPI("Se excedió el límite de " + solicitudesPorMinuto + " solicitudes por minuto",
                                        tTiposDeError.tError, "", httpContext.Request.Path.Value, 0, 0,
                                        codigoErrorInterno: StatusCodes.Status429TooManyRequests,
                                        IPRemoteDesktop: ip,
                                        IPPublica: httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "",
                                        Agent: httpContext.Request.Headers.UserAgent.ToString());

            cResponse oRespuesta = new cResponse();
            oRespuesta.error = new cError(StatusCodes.Status429TooManyRequests,
                                          "Se excedió el límite de " + solicitudesPorMinuto + " solicitudes por minuto.",
                                          ApiErrors.errRateLimitExceeded);

            httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            httpContext.Response.Headers.RetryAfter = "60";   // segundos que conviene esperar antes de reintentar
            await httpContext.Response.WriteAsJsonAsync(oRespuesta, cancellationToken);
        }
    }
}

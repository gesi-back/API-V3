using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.V3.BLL;
using GESI.CORE.V3.BO;
using GESI.ERP.Core.V3.BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;

namespace API_con_Core_V3.Middleware
{
    public sealed class ERPSessionMiddleware(RequestDelegate next)
    {
        private const string HeaderEmpresa = "empresaID";
        private const string HeaderSucursal = "sucursalID";

        public async Task InvokeAsync(HttpContext context, IApiSessionManager sesion,
                                      IAccesoApiServicio acceso, IApiLogger apiLogger)
        {

            if(!context.Request.Headers.ContainsKey("GrupoESI"))
            {
                await ResponderError(context, HttpStatusCode.BadRequest, 0,
                   $"No se encontró el encabezado GrupoESI en la solicitud");
                return;
            }

            // Login, RefreshToken y cualquier [AllowAnonymous]: sin sesión del ERP
            if (context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            {
                await next(context);
                return;
            }

            var user = context.User;

            // Sin token/ApiKey válidos: no armamos sesión; UseAuthorization responde 401
            if (user.Identity?.IsAuthenticated != true)
            {
                await next(context);
                return;
            }

            var req = context.Request;
            var usuario = user.FindFirst("sub")?.Value ?? "";
            var esApiKey = string.Equals(user.Identity.AuthenticationType, "ApiKey", StringComparison.OrdinalIgnoreCase);
            var ipPublica = req.Headers["X-Forwarded-For"].FirstOrDefault() ?? "";
            var agent = req.Headers.UserAgent.ToString();

            // JWT: además de firma y vigencia, el token tiene que estar habilitado en la tabla
            if (!esApiKey)
            {
                var auth = req.Headers.Authorization.ToString();
                var token = auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? auth["Bearer ".Length..].Trim() : "";

                if (!acceso.TokenHabilitado(token))
                {
                    apiLogger.LoguearSucesosAPI("Token no encontrado en la tabla de tokens", tTiposDeError.tError, usuario,
                        req.Path.Value, 0, 0, parametros: req.QueryString.ToString(), IPPublica: ipPublica, Agent: agent);

                    await ResponderError(context, HttpStatusCode.Unauthorized,
                        (int)cCodigosError.cTokenNoEncontrado, "Token no válido en el Request");
                    return;
                }
            }

            // Empresa y sucursal: 1) headers  2) valores por defecto del usuario
            int empresaId = 0, sucursalId = 0;
            
            bool vieneEmpresa = req.Headers.ContainsKey(HeaderEmpresa);
            bool vieneSucursal = req.Headers.ContainsKey(HeaderSucursal);

            cAPIHabilitaciones? oAPIHabilitacion = acceso.ObtenerHabilitacionAPIDelUsuario(usuario);

            if(oAPIHabilitacion == null)
            {
                apiLogger.LoguearSucesosAPI("Usuario no habilitado en Habilitaciones API", tTiposDeError.tError, usuario,
                        req.Path.Value, 0, 0, parametros: req.QueryString.ToString(), IPPublica: ipPublica, Agent: agent);

                await ResponderError(context, HttpStatusCode.BadRequest, 0,
                    "El usuario no se encuentra habilitado para consumir la API REST");
                return;
            }

            empresaId = oAPIHabilitacion.EmpresaID;
            sucursalId = oAPIHabilitacion.SucursalID;

            if (vieneEmpresa || vieneSucursal)
            {
                if (!int.TryParse(req.Headers[HeaderEmpresa], out empresaId) || !int.TryParse(req.Headers[HeaderSucursal], out sucursalId))
                {
                    await ResponderError(context, HttpStatusCode.BadRequest, 0,
                        $"'{HeaderEmpresa}' y '{HeaderSucursal}' deben venir juntos y ser numéricos.");
                    return;
                }
                
            }            
            else
            {
                if (empresaId <= 0)
                {
                    await ResponderError(context, HttpStatusCode.Forbidden, 0,
                        "No se pudo determinar la empresa y sucursal del usuario.");
                    return;
                }
            }

            if (!acceso.UsuarioPuedeOperar(usuario, empresaId, sucursalId))
            {
                await ResponderError(context, HttpStatusCode.Forbidden, 0,
                    $"El usuario no está habilitado para la empresa {empresaId} / sucursal {sucursalId}.");
                return;
            }

            sesion.Iniciar(empresaId, sucursalId, usuario,
                metodoAutenticacion: esApiKey ? "ApiKey" : "Bearer",
                correlationId: context.Items["CorrelationId"] as string,
                remoteIP: context.Connection.RemoteIpAddress?.ToString(),
                forwarded: ipPublica,
                userAgent: agent,oAPIHabilitacion.CategoriasIDs, oAPIHabilitacion.CostosDeProveedor, oAPIHabilitacion.Estados, oAPIHabilitacion.ReleerPreciosAlImportarComprobantes, oAPIHabilitacion.AlmacenesIDs,
                tiposDeClientes: oAPIHabilitacion.TiposDeCliente,
                usuarioExterno: oAPIHabilitacion.UsuarioExterno);

            

            await next(context);
        }

        private static Task ResponderError(HttpContext context, HttpStatusCode status, int codigo, string mensaje)
        {
            context.Response.StatusCode = (int)status;
            var respuesta = new cResponse
            {
                error = new cError(codigo, mensaje, ApiErrors.errInvalidToken)   // ajustar el ApiErrors según el caso
            };
            return context.Response.WriteAsJsonAsync(respuesta);
        }
    }
}

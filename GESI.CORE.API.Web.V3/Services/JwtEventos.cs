using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3;
using GESI.CORE.API.BO.V3.API;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace GESI.CORE.API.Web.V3.Services
{
    public sealed class JwtEventos(IApiLogger apiLogger) : JwtBearerEvents
    {
        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            // Se ejecuta cuando el token vino pero es inválido (vencido, mal firmado, etc.)
            return Task.CompletedTask;
        }

        public override async Task Challenge(JwtBearerChallengeContext context)
        {
            context.HandleResponse();   // evita la respuesta 401 estándar

            bool tokenInvalido = context.AuthenticateFailure is not null;
            string mensaje = tokenInvalido ? "Token invalido. Acceso denegado" : "No se encontro el Token en el Request";
            int codigo = (int)(tokenInvalido ? cCodigosError.cTokenInvalido : cCodigosError.cTokenNoEncontrado);

            var req = context.Request;
            apiLogger.LoguearSucesosAPI(mensaje, tTiposDeError.tError, "", req.Path.Value, 0, 0,
                parametros: req.QueryString.ToString(),
                IPRemoteDesktop: context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                IPPublica: req.Headers["X-Forwarded-For"].FirstOrDefault() ?? "",
                Agent: req.Headers.UserAgent.ToString());

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new cResponse
            {
                error = new cError(codigo, mensaje, ApiErrors.errInvalidToken)
            });
        }

        public override Task TokenValidated(TokenValidatedContext context)
        {
            if (context.SecurityToken is JsonWebToken jwt && context.Principal?.Identity is ClaimsIdentity identity)
                identity.AddClaim(new Claim("access_token", jwt.EncodedToken));
            return Task.CompletedTask;
        }
    }
}

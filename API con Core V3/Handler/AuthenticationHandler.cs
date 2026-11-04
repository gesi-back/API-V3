using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BO.V3;
using GESI.CORE.V3.BLL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace API_con_Core_V3.Handler
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string SchemeName = "ApiKey";
        public const string HeaderName = "GrupoESI-ApiKey";
    }

    public class ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IAPIKeyBLL apiKeyBLL)                                   // ← inyectado desde DI
        : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {            
            if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var apiKeyHeader))
                return AuthenticateResult.NoResult();           // no vino ApiKey: este esquema no aplica

            var apiKey = apiKeyHeader.ToString().Trim();
            if (apiKey.Length == 0)
                return AuthenticateResult.Fail("API Key no encontrada");

            // Ajustar a la firma real: si es sincrónico, sacá el await
            var (esValida, usuarioID) = await apiKeyBLL.ValidarAPIKey(apiKey);

            if (!esValida || string.IsNullOrWhiteSpace(usuarioID))
                return AuthenticateResult.Fail("API Key inválida");

            var identity = new ClaimsIdentity(
                [
                    new Claim("sub", usuarioID),                // el middleware arma la sesión con esto
                new Claim("auth_method", Scheme.Name),
            ],
                authenticationType: Scheme.Name,                // "ApiKey"
                nameType: "sub",
                roleType: "roles");

            return AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var resultado = await HandleAuthenticateOnceSafeAsync();
            var mensaje = resultado.Failure?.Message ?? "API Key no encontrada";

            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            Response.ContentType = "application/json; charset=utf-8";

            var oresp = new cResponseToken
            {
                error = new cError((int)cCodigosError.cTokenInvalido, mensaje, ApiErrors.errInvalidToken)
            };
            await Response.WriteAsync(JsonConvert.SerializeObject(oresp));
        }
    }
}

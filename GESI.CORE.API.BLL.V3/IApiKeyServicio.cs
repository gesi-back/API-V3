using GESI.CORE.V3.BLL;
using System;
using System.Collections.Generic;
using System.Text;

namespace GESI.CORE.API.BLL.V3
{
    public sealed record ResultadoApiKey(bool EsValida, string UsuarioID);

    public interface IApiKeyServicio
    {
        Task<ResultadoApiKey> ValidarAsync(string apiKey, CancellationToken ct = default);
    }

    internal sealed class ApiKeyServicio(IAPIKeyBLL apiKeyBLL) : IApiKeyServicio
    {
        public async Task<ResultadoApiKey> ValidarAsync(string apiKey, CancellationToken ct = default)
        {
            // Por ahora delega en V2; cuando tengas el BLL de V3, se cambia solo acá
            var (esValida, usuario) = await apiKeyBLL.ValidarAPIKey(apiKey);
            return new(esValida, usuario ?? "");
        }
    }
}

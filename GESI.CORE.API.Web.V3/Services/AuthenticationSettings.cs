namespace GESI.CORE.API.Web.V3.Services
{
    public sealed class AuthenticationSettings
    {
        public const string Seccion = "AuthenticationSettings";
        public string SigningKey { get; set; } = "";
        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
        public double HorasExpiracion { get; set; } = 8;
    }
}

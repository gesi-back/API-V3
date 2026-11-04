using GESI.CORE.API.BLL.V3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_con_Core_V3.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthService(ILoginServicio login, IOptions<AuthenticationSettings> opciones) : IAuthService
    {
        private readonly AuthenticationSettings _cfg = opciones.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidateLogin(string? username, string? password)
        {
            //aqui haríamos la validación, de momento simulamos validación login
            bool isCredentialValid = login.ValidateLogin(username, password);

            if (isCredentialValid)
                return true;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fechaActual"></param>
        /// <param name="username"></param>
        /// <param name="tiempoValidez"></param>
        /// <returns></returns>
        
        public string GenerateToken(DateTime fechaActual, string? username, TimeSpan tiempoValidez)
        {
            var desde = fechaActual.ToUniversalTime();
            var hasta = desde.Add(tiempoValidez);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(desde).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("roles", "Cliente"),
            new Claim("roles", "Administrador"),
        };

            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_cfg.SigningKey)),
                SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: _cfg.Issuer,
                audience: _cfg.Audience,
                claims: claims,
                notBefore: desde,
                expires: hasta,
                signingCredentials: credenciales);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }


    }
}

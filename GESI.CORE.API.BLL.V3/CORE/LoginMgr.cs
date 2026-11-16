using GESI.CORE.API.BO.V3.API;
using GESI.CORE.V3.BLL;
using GESI.CORE.V3.BO;
using GESI.ERP.Core.V3.BO;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Authentication;
using System.Text;

namespace GESI.CORE.API.BLL.V3.CORE
{
   public interface ILoginServicio
    {
        cResponseToken? ValidarUsuario(cUserLogin? userLogin);

        bool ValidateLogin(string? username, string? password);

        cResponseToken DevolverRespuestaTokenYActualizarEnBase(string? token,
                        string? usuario,
                        DateTime fechaAActualizar,
                        string? clienteIPAddress,
                        DateTime FechaExpiracion);

        cAPILogin ValidarRefreshToken(string? token, string? refreshToken);

    }

    internal sealed class LoginServicio(IUsuariosBLL usuarios,IAPIHabilitacionesBLL habilitacionesAPI, IAPILoginBLL apilogin) : ILoginServicio
    {
        
        public cResponseToken? ValidarUsuario(cUserLogin? userLogin)
        {

            try
            {
                if (ValidateLogin(userLogin?.Username, userLogin?.Password))
                {                    
                    List<cAPILogin>? lstApiToken = apilogin.GetItem("", userLogin?.Username ?? "");
                    cAPILogin MiAPIToken = new cAPILogin();

                    if (lstApiToken?.Count > 0)
                        lstApiToken = lstApiToken.Where(x => x.Activo == true).OrderByDescending(x => x.FechaYHora).ToList();

                    if (lstApiToken?.Count > 0)
                        MiAPIToken = lstApiToken[0];
                    
                    DateTime deFechaSQL = DateTime.Now;

                    
                    var TiempoExpiracionToken = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["TiempoExpiracion"];
                    
                    var validez = TimeSpan.FromHours(double.Parse(TiempoExpiracionToken ?? "", CultureInfo.InvariantCulture));
                    var fechaExpiracion = DateTime.Now.Add(validez);

                    if (MiAPIToken?.Token?.Length > 0)  // ENCONTRO OBJETO TOKEN
                    {

                        TimeSpan horasDiferencia = deFechaSQL - MiAPIToken.FechaYHora;
                        int intHorasDiferencia = (int)horasDiferencia.TotalHours;
                        decimal tmExpiracion = decimal.Parse(TiempoExpiracionToken ?? "", CultureInfo.InvariantCulture);
                        if (intHorasDiferencia >= tmExpiracion) // EXPIRÓ EL TIEMPO DEL TOKEN . Genera un nuevo Token
                        {
                            return null;
                        }
                        else
                        {
                            if (intHorasDiferencia < 0)  // TIENE UNA FECHA FUTURA. GENERA NUEVO TOKEN
                            {
                                return null;
                            }
                            else    // DEVUELVE EL MISMO TOKEN QUE SE ENCUENTRA EN LA TABLA . VIGENTE
                            {
                                cResponseToken respuestaToken = new cResponseToken();
                                respuestaToken.success = true;
                                respuestaToken.token = MiAPIToken.Token;
                                respuestaToken.refresh_token = MiAPIToken.RefreshToken;
                                respuestaToken.expiracion = MiAPIToken.FechaYHora.AddHours(Convert.ToDouble(TiempoExpiracionToken)).ToString("yyyy-MM-ddTHH:mm:ss");
                                return respuestaToken;
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                    
                }
                else
                {
                    throw new InvalidCredentialException("Usuario y/o Contraseña incorrectos");
                }


            }
            catch (InvalidCredentialException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Valida el Usuario y contraseña
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidateLogin(string? username,
                                            string? password)
        {
            bool Habilitado = false;
            //aqui haríamos la validación, de momento simulamos validación login
            if (username == null)
                username = "";

            if (password == null)
                password = "";


            bool isCredentialValid = usuarios.Login(username, password);

            if (isCredentialValid)
                return true;


            List<cAPIHabilitaciones>? lstHabilitacionesAPI = habilitacionesAPI.GetList(username);

            if (lstHabilitacionesAPI?.Count > 0)
            {
                if (isCredentialValid)
                {
                    Habilitado = true;
                }
            }

            return Habilitado;
        }

        public cResponseToken DevolverRespuestaTokenYActualizarEnBase(string? Token,
                                                                            string? usuario,
                                                                            DateTime FechaAActualizar,
                                                                            string? clientIpAddress,
                                                                            DateTime FechaExpiracion)
        {
            try
            {
                #region Objeto a Grabar
                cAPILogin? ApiLogin = new cAPILogin();
                ApiLogin.FechaYHora = FechaAActualizar;

                if (usuario == null)
                    usuario = "";

                if (Token == null)
                    Token = "";

                ApiLogin.UsuarioID = usuario;
                ApiLogin.Token = Token;
                #endregion


                #region Respuesta Token
                cResponseToken respuestaToken = new cResponseToken();
                respuestaToken.success = true;
                respuestaToken.token = Token;

                Guid nuevoguid = Guid.NewGuid();
                ApiLogin.RefreshToken = nuevoguid.ToString();
                ApiLogin.Activo = true;
                //ApiLogin.DeviceFingerprint = GESI.CORE.V2.SEC.Seguridad.GetDeviceFingerprint();
                respuestaToken.refresh_token = ApiLogin.RefreshToken;
                respuestaToken.expiracion = FechaExpiracion.ToString("yyyy-MM-ddTHH:mm:ss");

                #endregion

                int resultado = apilogin.Save(ApiLogin);

                string strultimosCaracteresToken = Token.Length >= 7
                                    ? Token[^7..]
                                    : Token;                

                return respuestaToken;

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Valida el pedido de RefreshToken (migrado del LoginController de la V2) y devuelve el login guardado.
        /// Pasos de la V2: el token existe, está activo y el refresh_token coincide con el último entregado.
        /// Si algo no se cumple lanza InvalidCredentialException con el mismo mensaje de la V2 (el controller responde 400).
        ///
        /// NUEVO respecto de la V2: no se compara el DeviceFingerprint. La V2 lo comparaba pero nunca lo grababa
        /// (la línea está comentada en el login) y además era la huella del servidor, no del cliente,
        /// así que en la práctica el refresh de la V2 no podía funcionar. El secreto es el refresh_token (GUID).
        /// </summary>
        public cAPILogin ValidarRefreshToken(string? token, string? refreshToken)
        {
            // NUEVO respecto de la V2: la V2 validaba por error el token de su propia respuesta (vacía) en lugar del recibido
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                throw new ArgumentException("No se detallo un refreshtoken / token");

            List<cAPILogin>? lstApiLogins = apilogin.GetItem(token);

            if (lstApiLogins == null || lstApiLogins.Count == 0)
                throw new InvalidCredentialException("No se encontró el token informado.");

            lstApiLogins = lstApiLogins.Where(x => x.Activo == true).ToList();

            if (lstApiLogins.Count == 0)
                throw new InvalidCredentialException("El token y refresh token ya expiró");

            cAPILogin oAPILogin = lstApiLogins[0];

            if (oAPILogin.RefreshToken == null || !oAPILogin.RefreshToken.Equals(refreshToken))
                throw new InvalidCredentialException("No coincide el Refresh Token con el ultimo solicitado");

            return oAPILogin;
        }
    }

}

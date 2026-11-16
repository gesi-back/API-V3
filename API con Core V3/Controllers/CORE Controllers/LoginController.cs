using API_con_Core_V3.Services;
using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.V3.BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Security.Authentication;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API_con_Core_V3.Controllers
{
    [AllowAnonymous]
    [Route("api/Autenticacion/[controller]")]
    [ApiController]
    public class LoginController(ILoginServicio loginService,IAuthService authService, IApiLogger APILogger,IAPIHelper APIHelper) : ControllerBase
    {
        [HttpPost]
        [EnableCors("MyCorsPolicy")]
        [Produces("application/json")]        
        public async Task<IActionResult?> TokenAsync(cUserLogin credenciales)
        {
            cResponseToken rspContenidoRespuesta = new cResponseToken();

            if(!ModelState.IsValid)
            {
                rspContenidoRespuesta.error = new cError(500, "Modelo de JSON no valido");

                return BadRequest(rspContenidoRespuesta);
            }
            else
            {
                try
                {
                    cResponseToken? oResponse = loginService.ValidarUsuario(credenciales);

                    if (oResponse != null)
                    {
                        return Ok(oResponse);
                    }
                    else
                    {
                        HttpContext context = HttpContext;
                        var TiempoExpiracionToken = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["TiempoExpiracion"];
                        var validez = TimeSpan.FromHours(double.Parse(TiempoExpiracionToken ?? "", CultureInfo.InvariantCulture));

                        var fechaExpiracion = DateTime.Now.Add(validez);
                       
                        DateTime deFechaSQL = DateTime.Now; // OBTIENE LA FECHA Y HORA DEL SV SQL

                        var token = authService.GenerateToken(deFechaSQL, credenciales.Username, validez); // GENERA EL TOKEN                                               

                        rspContenidoRespuesta = loginService.DevolverRespuestaTokenYActualizarEnBase(token, credenciales.Username, deFechaSQL, context?.Request?.HttpContext?.Connection?.RemoteIpAddress?.ToString(), fechaExpiracion);  // DEVUELVE TOKEN Y LO GRABA EN LA TABLA API LOGIN TOKENS

                        return Ok(rspContenidoRespuesta);
                    }
                }
                catch (InvalidCredentialException)
                {                    
                    APILogger.LoguearSucesosAPI("Usuario incorrecto", tTiposDeError.tError, credenciales.Username, "api/Autenticacion/Login", 0, 0);
                    rspContenidoRespuesta.error = new cError((int)cCodigosError.cUsuarioIncorrecto, "Usuario y/o contraseña incorrectos", ApiErrors.errUsuarioNoHallado);
                    return Unauthorized(rspContenidoRespuesta);
                }
                catch (Exception e)
                {
                    
                    cResponseToken oRespuestaConError = new cResponseToken();
                    rspContenidoRespuesta.error = new cError((int)cCodigosError.cErrorInternoAlDevolverToken,
                                                                                " Descripcion: " + e.Message, ApiErrors.errInternalServerError);

                   APIHelper.LanzarExcepcion(e, credenciales.Username ?? "", "api/Autenticacion/Login", 0, this.HttpContext.Request.QueryString.ToString());

                    return StatusCode((int)HttpStatusCode.InternalServerError, rspContenidoRespuesta);
                }
            }

        }

        /// <summary>
        /// Genera un token nuevo a partir del token anterior y su refresh_token, sin volver a pedir usuario y contraseña.
        /// Migrado de la V2 (misma ruta, mismo body y mismas respuestas), sin la comparación de DeviceFingerprint.
        /// </summary>
        /// <param name="oRequest">token y refresh_token entregados en el último login / refresh</param>
        [HttpPost("/api/Autenticacion/RefreshToken")]
        [EnableCors("MyCorsPolicy")]
        [Produces("application/json")]
        public IActionResult RefreshToken([FromBody] cResponseToken? oRequest)
        {
            cResponseToken oRespuesta = new cResponseToken();

            try
            {
                GESI.CORE.V3.BO.cAPILogin oAPILogin = loginService.ValidarRefreshToken(oRequest?.token, oRequest?.refresh_token);

                // Igual que el login: validez en horas según AppSettings:TiempoExpiracion
                var TiempoExpiracionToken = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["TiempoExpiracion"];
                var validez = TimeSpan.FromHours(double.Parse(TiempoExpiracionToken ?? "", CultureInfo.InvariantCulture));
                var fechaExpiracion = DateTime.Now.Add(validez);

                DateTime deFechaSQL = DateTime.Now;

                var token = authService.GenerateToken(deFechaSQL, oAPILogin.UsuarioID, validez);   // GENERA EL TOKEN

                // Graba el token nuevo con un refresh_token nuevo (igual que el login).
                // Si el registro del token anterior sigue activo depende de cómo graba el Core (IAPILoginBLL.Save)
                oRespuesta = loginService.DevolverRespuestaTokenYActualizarEnBase(token,
                                                                                oAPILogin.UsuarioID,
                                                                                deFechaSQL,
                                                                                HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString(),
                                                                                fechaExpiracion);

                return Ok(oRespuesta);
            }
            catch (ArgumentException ex)
            {
                // Igual que la V2: falta el token o el refresh_token → 400
                oRespuesta.error = new cError((int)cCodigosError.cCodigoDetalladoEnLaSolicitud, ex.Message, ApiErrors.errInvalidToken);
                return BadRequest(oRespuesta);
            }
            catch (InvalidCredentialException ex)
            {
                // Igual que la V2: token inexistente, inactivo o refresh_token que no coincide → 400
                APILogger.LoguearSucesosAPI("RefreshToken rechazado: " + ex.Message, tTiposDeError.tError, "", "api/Autenticacion/RefreshToken", 0, 0,
                                            IPPublica: HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "",
                                            IPRemoteDesktop: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");

                oRespuesta.error = new cError((int)cCodigosError.cRefreshTokenInvalido, ex.Message, ApiErrors.errInvalidToken);
                return BadRequest(oRespuesta);
            }
            catch (Exception ex)
            {
                // NUEVO respecto de la V2: el detalle técnico queda en el log y no se devuelve al cliente
                APIHelper.LanzarExcepcion(ex, "", "api/Autenticacion/RefreshToken", 0, HttpContext.Request.QueryString.ToString());

                oRespuesta.error = new cError((int)cCodigosError.cErrorInternoAplicacion, "Error al generar nuevamente el token.", ApiErrors.errInternalServerError);
                return StatusCode((int)HttpStatusCode.InternalServerError, oRespuesta);
            }
        }
    }
}

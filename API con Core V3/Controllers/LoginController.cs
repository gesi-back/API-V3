using API_con_Core_V3.Services;
using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BO.V3;
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
    }
}

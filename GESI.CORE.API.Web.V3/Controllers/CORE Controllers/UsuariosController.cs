using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.CORE;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de usuarios (igual que la V2, la ruta es api/Usuarios, no api/Maestros)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsuariosController(IUsuariosServicio UsuariosMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene las opciones mas utilizadas por un usuario
        /// </summary>
        /// <param name="usuarioID">Identificador del usuario</param>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de registros por pagina</param>
        [HttpGet("GetOpcionesUtilizadas")]
        public ActionResult<cResponseOpcionesUtilizadas> GetOpcionesUtilizadasPorUsuario(string? usuarioID = null,
                                                                                        int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                                        int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize)
            => Responder(() => UsuariosMgr.GetOpcionesUtilizadasPorUsuario(usuarioID, pageNumber, pageSize));

        /// <summary>
        /// Valida el nivel de acceso del usuario de la sesión para un permiso del VersCom
        /// </summary>
        /// <param name="accesoID">Codigo de acceso a validar</param>
        [HttpGet("GetNivelDeAcceso")]
        public ActionResult<cResponseNivelAcceso> GetNivelAcceso(int accesoID = 0)
            => Responder(() => UsuariosMgr.GetNivelAcceso(accesoID));
    }
}

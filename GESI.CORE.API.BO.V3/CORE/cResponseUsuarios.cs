using GESI.CORE.API.BO.V3.API;
using GESI.CORE.V3.BO;

namespace GESI.CORE.API.BO.V3.CORE
{
    /// <summary>
    /// Respuesta con las opciones mas utilizadas por un usuario
    /// </summary>
    public class cResponseOpcionesUtilizadas : cResponse
    {
        /// <summary>
        /// Listado de opciones utilizadas
        /// </summary>
        public List<cOpcionUtilizadaPorUsuario>? opcionesUtilizadas { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con el nivel de acceso del usuario para un permiso
    /// </summary>
    public class cResponseNivelAcceso : cResponse
    {
        /// <summary>
        /// Codigo de acceso consultado
        /// </summary>
        public int accesoID { get; set; }

        /// <summary>
        /// Nivel de acceso del usuario
        /// </summary>
        public int nivelDeAcceso { get; set; }

        /// <summary>
        /// Descripcion del nivel de acceso
        /// </summary>
        public string? descripcion { get; set; }
    }
}

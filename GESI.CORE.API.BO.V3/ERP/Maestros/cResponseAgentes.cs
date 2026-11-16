using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Objeto de respuesta de agentes
    /// </summary>
    public class cResponseAgentes : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de agentes
        /// </summary>
        public List<cAgente>? agentes { get; set; }

        /// <summary>
        /// Paginacion
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

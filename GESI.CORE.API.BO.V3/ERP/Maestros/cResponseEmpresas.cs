using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con las empresas habilitadas para el usuario
    /// </summary>
    public class cResponseEmpresas : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }

        /// <summary>
        /// Listado de empresas
        /// </summary>
        public List<cEmpresa>? empresas { get; set; }
    }
}

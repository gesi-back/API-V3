using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con criterios de logistica
    /// </summary>
    public class cResponseCriterioLogistica : cResponse
    {
        /// <summary>
        /// Listado de criterios de logistica
        /// </summary>
        public List<cCriterioLogistica>? criteriosLogistica { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

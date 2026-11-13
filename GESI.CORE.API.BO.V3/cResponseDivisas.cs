using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3
{
    /// <summary>
    /// Respuesta con divisas y sus cotizaciones
    /// </summary>
    public class cResponseDivisas : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }

        /// <summary>
        /// Listado de divisas
        /// </summary>
        public List<cDivisa>? divisas { get; set; }
    }
}

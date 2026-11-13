using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3
{
    /// <summary>
    /// Respuesta de las zonas (cada zona incluye sus subzonas)
    /// </summary>
    public class cResponseZonas : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de zonas
        /// </summary>
        public List<cZona>? zonas { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

using GESI.CORE.API.BO.V3.API;
using GESI.CORE.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con tipos de documento
    /// </summary>
    public class cResponseTipoDocumento : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de tipos de documento
        /// </summary>
        public List<cTipoDeDocumento>? tiposDeDocumento { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

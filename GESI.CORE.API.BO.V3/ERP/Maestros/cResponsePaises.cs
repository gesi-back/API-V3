using GESI.CORE.API.BO.V3.API;
using GESI.CORE.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con paises
    /// </summary>
    public class cResponsePaises : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de paises
        /// </summary>
        public List<cPais>? paises { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con un pais (incluye sus regiones)
    /// </summary>
    public class cResponsePais : cResponse
    {
        /// <summary>
        /// Objeto de pais
        /// </summary>
        public cPais? pais { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

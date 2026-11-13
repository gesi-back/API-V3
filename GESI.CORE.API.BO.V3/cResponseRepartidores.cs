using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3
{
    /// <summary>
    /// Respuesta con repartidores
    /// </summary>
    public class cResponseRepartidores : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de repartidores
        /// </summary>
        public List<cRepartidor>? repartidores { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con un repartidor
    /// </summary>
    public class cResponseRepartidor : cResponse
    {
        /// <summary>
        /// Objeto de repartidor
        /// </summary>
        public cRepartidor? repartidor { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

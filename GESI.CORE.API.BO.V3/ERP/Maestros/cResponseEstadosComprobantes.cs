using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con estados de comprobantes de venta
    /// </summary>
    public class cResponseEstadosComprobantes : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de estados de comprobantes
        /// </summary>
        public List<cEstadoComprobanteDeVenta>? estadosDeComprobantes { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

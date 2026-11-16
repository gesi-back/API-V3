using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con comprobantes
    /// </summary>
    public class cResponseComprobantes : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de comprobantes
        /// </summary>
        public List<cComprobante>? comprobantes { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con un comprobante
    /// </summary>
    public class cResponseComprobante : cResponse
    {
        /// <summary>
        /// Objeto de comprobante
        /// </summary>
        public cComprobante? comprobante { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta al crear un comprobante (comprobante, success, error)
    /// </summary>
    public class cResponseSaveComprobante : cResponse
    {
        /// <summary>
        /// Comprobante grabado
        /// </summary>
        public cComprobante? comprobante { get; set; }
    }
}

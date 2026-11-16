using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con referencias contables
    /// </summary>
    public class cResponseReferenciasContables : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de referencias contables
        /// </summary>
        public List<cReferenciaContable>? referenciasContables { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con una referencia contable
    /// </summary>
    public class cResponseReferenciaContable : cResponse
    {
        /// <summary>
        /// Objeto de referencia contable
        /// </summary>
        public cReferenciaContable? referenciaContable { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

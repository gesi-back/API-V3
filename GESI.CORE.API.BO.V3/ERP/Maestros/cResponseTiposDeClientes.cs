using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con tipos de cliente
    /// </summary>
    public class cResponseTiposDeClientes : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de tipos de cliente
        /// </summary>
        public List<cTipoDeCliente>? tiposDeClientes { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

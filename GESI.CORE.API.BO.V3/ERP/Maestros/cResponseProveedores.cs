using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con proveedores
    /// </summary>
    public class cResponseProveedores : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de proveedores
        /// </summary>
        public List<cProveedor>? proveedores { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con un proveedor
    /// </summary>
    public class cResponseProveedor : cResponse
    {
        /// <summary>
        /// Objeto de proveedor
        /// </summary>
        public cProveedor? proveedor { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

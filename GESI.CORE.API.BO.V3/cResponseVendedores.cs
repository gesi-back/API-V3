using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3
{
    /// <summary>
    /// Respuesta con vendedores
    /// </summary>
    public class cResponseVendedores : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de vendedores
        /// </summary>
        public List<cVendedor>? vendedores { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con un vendedor
    /// </summary>
    public class cResponseVendedor : cResponse
    {
        /// <summary>
        /// Objeto de vendedor
        /// </summary>
        public cVendedor? vendedor { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con los descuentos de vendedores por aplicacion
    /// </summary>
    public class cResponseDescuentosVendedoresXAplicacion : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de descuentos de vendedores
        /// </summary>
        public List<cDescuentoXAplicacionXVendedor>? descuentosVendedores { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con cobradores
    /// </summary>
    public class cResponseCobradores : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de cobradores
        /// </summary>
        public List<cCobrador>? cobradores { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

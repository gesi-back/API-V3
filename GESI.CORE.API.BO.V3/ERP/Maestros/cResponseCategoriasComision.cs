using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con categorias de comision
    /// </summary>
    public class cResponseCategoriasComision : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de categorias de comision
        /// </summary>
        public List<cCategoriaComision>? categoriasComision { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

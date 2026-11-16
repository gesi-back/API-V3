using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con los valores del filtro de articulos 1
    /// </summary>
    public class cResponseFiltroArticulos1 : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado del filtro de articulos 1 (el nombre "filtroarticulos1" se mantiene igual que en la V2)
        /// </summary>
        public List<cFiltroArticulos1ID>? filtroarticulos1 { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con los valores del filtro de articulos 2
    /// </summary>
    public class cResponseFiltroArticulos2 : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado del filtro de articulos 2 (el nombre "filtroarticulos2" se mantiene igual que en la V2)
        /// </summary>
        public List<cFiltroArticulos2ID>? filtroarticulos2 { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con los valores del filtro de articulos 3
    /// </summary>
    public class cResponseFiltroArticulos3 : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado del filtro de articulos 3 (el nombre "filtroarticulos3" se mantiene igual que en la V2)
        /// </summary>
        public List<cFiltroArticulos3ID>? filtroarticulos3 { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

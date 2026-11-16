using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Stock.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Stock
{
    /// <summary>
    /// Respuesta con los almacenes a los que se puede transferir desde un almacén (mismo nombre y JSON que la V2)
    /// </summary>
    public class cResponseAlmacenesDestintoTransferencia : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Almacenes destino habilitados
        /// </summary>
        public List<cAlmacenDestinoTransferencia>? almacenes { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Stock.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Stock
{
    /// <summary>
    /// Respuesta con comprobantes de stock (ingresos, egresos, ajustes, transferencias, toma de inventario)
    /// </summary>
    public class cResponseMovimientoDeStock : cResponse
    {
        /// <summary>
        /// Lista de comprobantes de stock
        /// </summary>
        public List<cMovimientoDeStock>? movimientosDeStock { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

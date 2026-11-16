using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta de canales de venta
    /// </summary>
    public class cResponseCanalesDeVenta : cResponse
    {

        private List<cCanalDeVenta>? _CanalesVenta;
        private cPaginacion? _paginacion;

        /// <summary>
        /// Listado de canales de venta
        /// </summary>
        public List<cCanalDeVenta>? canalesDeVenta { get => _CanalesVenta; set => _CanalesVenta = value; }
        /// <summary>
        /// Resultado paginacion
        /// </summary>
        public cPaginacion? paginacion { get => _paginacion; set => _paginacion = value; }
    }


    /// <summary>
    /// Respuesta con el canal de venta
    /// </summary>
    public class cResponseCanalDeVenta : cResponse
    {

        private cCanalDeVenta? _canalDeVenta;
        private cPaginacion? _paginacion;

        /// <summary>
        /// Resupuesta del canal de venta
        /// </summary>
        public cCanalDeVenta? canalDeVenta { get => _canalDeVenta; set => _canalDeVenta = value; }
        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get => _paginacion; set => _paginacion = value; }
    }
}

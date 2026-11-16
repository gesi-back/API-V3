using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con puntos de venta
    /// </summary>
    public class cResponsePuntosDeVenta : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de puntos de venta
        /// </summary>
        public List<cPuntoDeVenta>? puntosDeVenta { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con un punto de venta
    /// </summary>
    public class cResponsePuntoDeVenta : cResponse
    {
        /// <summary>
        /// Objeto de punto de venta
        /// </summary>
        public cPuntoDeVenta? puntoDeVenta { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

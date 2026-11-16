using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Ventas.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Ventas
{
    /// <summary>
    /// Respuesta con comprobantes de venta (facturas, pedidos, remitos, cobros, etc.)
    /// </summary>
    public class cResponseMovimientoDeClientes : cResponse
    {
        /// <summary>
        /// Listado de comprobantes
        /// </summary>
        public List<cMovimientoDeCliente>? comprobantes { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Body de los GetList de comprobantes de venta
    /// </summary>
    public class dParametrosConsultaComprobantes
    {
        /// <summary>
        /// Filtros de la operacion (fechas, numeros, punto de venta, etc.)
        /// </summary>
        public dParametrosConsultaDeComprobantesDatosDeLaOperacion? datosOperacion { get; set; }

        /// <summary>
        /// Filtros del cliente (vendedor, zona, tipo de cliente, etc.)
        /// </summary>
        public dParametrosConsultaDeComprobantesDatosDelCliente? datosCliente { get; set; }
    }
}

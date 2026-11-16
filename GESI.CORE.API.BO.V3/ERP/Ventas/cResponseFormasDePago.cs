using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Ventas
{
    /// <summary>
    /// Respuesta con las formas de pago
    /// </summary>
    public class cResponseFormasDePago : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de formas de pago
        /// </summary>
        public List<cFormaDePago>? formasDePago { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con la forma de pago
    /// </summary>
    public class cResponseFormaDePago : cResponse
    {
        /// <summary>
        /// Objeto de forma de pago devuelta
        /// </summary>
        public cFormaDePago? formaDePago { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Resultado de grabar una forma de pago (un ítem por cada forma de pago enviada)
    /// </summary>
    public class cResponseFormaDePagoSave
    {
        /// <summary>
        /// Forma de pago grabada (o la enviada, si falló)
        /// </summary>
        public cFormaDePago? formaDePago { get; set; }

        /// <summary>
        /// Indica si se grabó correctamente
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// Mensaje de error o advertencia
        /// </summary>
        public string? mensaje { get; set; }

        /// <summary>
        /// Constructor vacio
        /// </summary>
        public cResponseFormaDePagoSave() { }

        /// <summary>
        /// Constructor con parametros
        /// </summary>
        public cResponseFormaDePagoSave(cFormaDePago? formaDePago, bool success, string? mensaje)
        {
            this.formaDePago = formaDePago;
            this.success = success;
            this.mensaje = mensaje;
        }
    }
}

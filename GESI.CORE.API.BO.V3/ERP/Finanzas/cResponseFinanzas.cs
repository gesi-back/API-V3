using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Finanzas
{
    /// <summary>
    /// Respuesta con cajas / bancos
    /// </summary>
    public class cResponseCajasBancos : cResponse
    {
        /// <summary>
        /// Listado de cajas / bancos
        /// </summary>
        public List<cCajaBanco>? cajasBancos { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con una caja / banco
    /// </summary>
    public class cResponseCajaBanco : cResponse
    {
        /// <summary>
        /// Objeto de caja / banco
        /// </summary>
        public cCajaBanco? cajaBanco { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con medios de pago (valores)
    /// </summary>
    public class cResponseMediosDePagos : cResponse
    {
        /// <summary>
        /// Listado de medios de pago
        /// </summary>
        public List<cValor>? mediosDePago { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con un medio de pago
    /// </summary>
    public class cResponseMedioDePago : cResponse
    {
        /// <summary>
        /// Objeto de medio de pago
        /// </summary>
        public cValor? medioDePago { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

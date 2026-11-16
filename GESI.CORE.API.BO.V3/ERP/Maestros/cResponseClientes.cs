using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Ventas.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con clientes (GetList, GetSearchResults, Create y Update)
    /// </summary>
    public class cResponseClientes : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de clientes
        /// </summary>
        public List<cAPICliente>? clientes { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con un cliente
    /// </summary>
    public class cResponseCliente : cResponse
    {
        /// <summary>
        /// Objeto de cliente
        /// </summary>
        public cAPICliente? cliente { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta de la actualización parcial de clientes (PATCH update-parcial)
    /// </summary>
    public class cResponseSaveClientes : cResponse
    {
        /// <summary>
        /// Paginacion del resultado (la V2 la declaraba pero no la completaba)
        /// </summary>
        public cPaginacion? paginacion { get; set; }

        /// <summary>
        /// Resumen de los clientes grabados
        /// </summary>
        public List<cResponseSaveCliente>? clientes { get; set; }
    }

    /// <summary>
    /// Resumen de un cliente grabado en la actualización parcial
    /// </summary>
    public class cResponseSaveCliente
    {
        /// <summary>
        /// Constructor vacio
        /// </summary>
        public cResponseSaveCliente() { }

        /// <summary>
        /// Arma el resumen a partir del cliente del Core
        /// </summary>
        public cResponseSaveCliente(GESI.ERP.Core.V3.BO.cCliente oCliente)
        {
            ClienteID = oCliente.ClienteID;
            RazonSocial = oCliente.RazonSocial;
            Domicilio = oCliente.Domicilio;
            Telefono = oCliente.Telefono;
            Email = oCliente.Email;
            CanalDeVentaID = oCliente.CanalDeVentaID;
            TipoDeDocumentoID = oCliente.TipoDeDocumentoID;
            NumeroDeDocumento = oCliente.NumeroDeDocumento;
            CondicionIVA = oCliente.CondicionIVA;
        }

        /// <summary>Codigo de cliente</summary>
        public int? ClienteID { get; set; }
        /// <summary>Razon social</summary>
        public string? RazonSocial { get; set; }
        /// <summary>Domicilio</summary>
        public string? Domicilio { get; set; }
        /// <summary>Telefono</summary>
        public string? Telefono { get; set; }
        /// <summary>Correo electronico</summary>
        public string? Email { get; set; }
        /// <summary>Canal de venta</summary>
        public int? CanalDeVentaID { get; set; }
        /// <summary>Tipo de documento</summary>
        public int? TipoDeDocumentoID { get; set; }
        /// <summary>Numero de documento</summary>
        public string? NumeroDeDocumento { get; set; }
        /// <summary>Condicion de IVA</summary>
        public int? CondicionIVA { get; set; }
    }

    /// <summary>
    /// Respuesta con los saldos de clientes
    /// </summary>
    public class cResponseSaldosClientes : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Saldos por cliente
        /// </summary>
        public List<dSaldosCliente>? saldos { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

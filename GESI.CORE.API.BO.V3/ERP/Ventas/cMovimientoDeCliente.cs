using GESI.ERP.Core.V3.BO;
using GESI.ERP.Finanzas.V3.BO;
using GESI.ERP.Ventas.V3.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GESI.CORE.API.BO.V3.ERP.Ventas
{

    /// <summary>
    /// Objeto de comprobante de venta para importaciones / lecturas
    /// </summary>
    public class cMovimientoDeCliente : GESI.ERP.Ventas.V3.BO.cComprobanteVenta
    {
        /// <summary>
        /// Listado de items del comprobante
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public override List<cItemComprobanteDeVenta>? DetalleDeItems { get => base.DetalleDeItems; set => base.DetalleDeItems = value; }
        /// <summary>
        /// Listado de items del comprobante
        /// </summary>
        public List<cDetalleDeVenta>? DetalleItems { get => _DetalleItems; set => _DetalleItems = value; }

        private List<cDetalleDeVenta>? _DetalleItems;


        /// <summary>
        /// Listado de impuestos del comprobante
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public override List<cImpuestosComprobanteDeVenta>? DetalleDeImpuestos{ get => base.DetalleDeImpuestos; set => base.DetalleDeImpuestos = value; }

        /// <summary>
        /// Listado de impuestos del comprobante
        /// </summary>
        public List<cDetalleImpuestos>? DetalleImpuestos { get => _DetalleImpuestos; set => _DetalleImpuestos = value; }

        /// <summary>
        /// Codigo de foja a la que pertenece el comprobante
        /// </summary>
        public int? FojaID { get => fojaID; set => fojaID = value; }

        private List<cDetalleImpuestos>? _DetalleImpuestos;

        private int? fojaID;

        /// <summary>
        /// Constructor vacio
        /// </summary>
        public cMovimientoDeCliente() { }

        /// <summary>
        /// Constructor con objeto padre heredado
        /// </summary>
        /// <param name="padre"></param>
        public cMovimientoDeCliente(GESI.ERP.Ventas.V3.BO.cComprobanteVenta padre)
        {
            EmpresaID = padre.EmpresaID;
            ComprobanteID = padre.ComprobanteID;
            Serie = padre.Serie;
            PuntoDeVentaID = padre.PuntoDeVentaID;
            Numero = padre.Numero;
            Subtipo = padre.Subtipo;
            FechaDeEmision = padre.FechaDeEmision;
            FechaDePrimerVencimiento = padre.FechaDePrimerVencimiento;
            ClienteID = padre.ClienteID;
            AgrupacionID = padre.AgrupacionID;
            ImporteNeto = padre.ImporteNeto;
            ImporteExento = padre.ImporteExento;
            ImporteDescuento = padre.ImporteDescuento;
            ImporteIVAGeneral = padre.ImporteIVAGeneral;
            ImporteIVAAdicional = padre.ImporteIVAAdicional;
            ImporteImpuestos = padre.ImporteImpuestos;
            ImporteTotal = padre.ImporteTotal;
            DescuentoImporte = padre.DescuentoImporte;
            DescuentoPorcentaje = padre.DescuentoPorcentaje;
            Redondeo = padre.Redondeo;
            SaldoPendiente = padre.SaldoPendiente;
            
            RelacionesDeItems = new List<cRelacionItemVenta>();

            if(padre.RelacionesDeItems?.Count > 0)
            {
                foreach(GESI.ERP.Ventas.V3.BO.cRelacionItemVenta oRelacion in padre.RelacionesDeItems)
                {
                    RelacionesDeItems.Add(oRelacion);
                }
            }

                     
            DetalleItems = new List<cDetalleDeVenta>();
            
            AplicacionesADebitos = new List<cRelacionComprobantes>();
            if(padre.AplicacionesADebitos?.Count > 0)
            {
                foreach(GESI.ERP.Ventas.V3.BO.cRelacionComprobantes Aplicacion in padre.AplicacionesADebitos)
                {
                    AplicacionesADebitos.Add(Aplicacion);
                }
            }

            if (padre.DetalleDeItems?.Count > 0)
            {
                foreach (GESI.ERP.Ventas.V3.BO.cItemComprobanteDeVenta oItem in padre.DetalleDeItems)
                {
                    oItem.Serie = padre.Serie;
                    oItem.ComprobanteID = padre.ComprobanteID;
                    oItem.EmpresaID = padre.EmpresaID;   // la V2 asignaba padre.ComprobanteID por error (no se veía porque EmpresaID no se serializa)
                    oItem.PuntoDeVentaID = padre.PuntoDeVentaID;
                    DetalleItems.Add(new cDetalleDeVenta(oItem));
                }
            }

            DetalleImpuestos = new List<cDetalleImpuestos>();

            if (padre.DetalleDeImpuestos?.Count > 0)
            {
                foreach (GESI.ERP.Ventas.V3.BO.cImpuestosComprobanteDeVenta oImpuesto in padre.DetalleDeImpuestos)
                {
                    DetalleImpuestos.Add(new cDetalleImpuestos(oImpuesto));
                }
            }

            DetalleDeValores = new List<cMovimientoDeValor>();

            if (padre.DetalleDeValores?.Count > 0)
            {
                foreach (GESI.ERP.Finanzas.V3.BO.cMovimientoDeValor oValor in padre.DetalleDeValores)
                {
                    DetalleDeValores.Add(oValor);
                }
            }
            

            Notas = padre.Notas;
            RazonSocial = padre.RazonSocial;
            NumeroDeDocumento = padre.NumeroDeDocumento;
            DivisaID = padre.DivisaID;
            Cotizacion = padre.Cotizacion;
            Anulado = padre.Anulado;
            EsUnAcopio = padre.EsUnAcopio;
            VendedorID = padre.VendedorID;
            RepartidorID = padre.RepartidorID;
            FormaDePagoID = padre.FormaDePagoID;
            CanalDeVentaID = padre.CanalDeVentaID;
            SucursalClienteID = padre.SucursalClienteID;
            TipoDeOperacionID = padre.TipoDeOperacionID;
            AlmacenID = padre.AlmacenID;
            ImporteNoGravado = padre.ImporteNoGravado;
            SucursalID = padre.SucursalID;
            CodigoDeImportacion = padre.CodigoDeImportacion;
            Hora = padre.Hora;
            Domicilio = padre.Domicilio;
            Telefono = padre.Telefono;
            Localidad = padre.Localidad;
            Email = padre.Email;

            Vencimientos = new List<cVencimientoComprobanteDeVenta>();

            if (padre.Vencimientos?.Count > 0)
            {
                foreach (GESI.ERP.Ventas.V3.BO.cVencimientoComprobanteDeVenta oVencimiento in padre.Vencimientos)
                {
                    Vencimientos.Add(oVencimiento);
                }
            }

        }

    }

    /// <summary>
    /// Comprobante viejo de la API
    /// </summary>
    public class cComprobanteDeVentaAPI
    {
        private cClienteAPI? _cliente;
        private String? _FechaRegistracion;
        private string? _notas;
        private int _ComprobanteID;


        /// <summary>
        /// Cliente de la operacion
        /// </summary>
        public cClienteAPI? Cliente { get => _cliente; set => _cliente = value; }
        /// <summary>
        /// Fecha de emision del comprobante
        /// </summary>
        public String? FechaRegistracion { get => _FechaRegistracion; set => _FechaRegistracion = value; }    
        /// <summary>
        /// Observaciones del comprobante
        /// </summary>
        public string? Notas { get => _notas; set => _notas = value; }
        /// <summary>
        /// Codigo de comprobanteID
        /// </summary>
        public int ComprobanteID { get => _ComprobanteID; set => _ComprobanteID = value; }


    }

    /// <summary>
    /// Comprobante de pedido de api anterior
    /// </summary>
    public class cComprobanteDePedidoAPI : cComprobanteDeVentaAPI
    {
        private int _PedidoID;
        
        private String? _FechaEntrega;
        private String? _Notas;
        private List<cItemAPI>? _Items;
        private SucursalCliente? _SucursalCliente;
        private String? _TipoPrecio;
        private String? _DescuentoPorcentajes;
        private decimal _DescuentoImporte;
        private String? _DescuentoTexto;
        private int _VendedorID;
        private String? _TipoDeComprobante;
        private String? _Hora;
        private int _FormaIngreso;
        /// <summary>
        /// Codigo de pedido externo
        /// </summary>
        public int PedidoID { get => _PedidoID; set => _PedidoID = value; }
      
        /// <summary>
        /// Fecha de entrega
        /// </summary>
        public String? FechaEntrega { get => _FechaEntrega; set => _FechaEntrega = value; }
        /// <summary>
        /// Observaciones del pedido
        /// </summary>
        public string? Notas { get => _Notas; set => _Notas = value; }

        /// <summary>
        /// Lista de items del comprobante
        /// </summary>
        public List<cItemAPI>? Items { get => _Items; set => _Items = value; }
       
        /// <summary>
        /// Sucursal del cliente 
        /// </summary>
        public SucursalCliente? SucursalCliente { get => _SucursalCliente; set => _SucursalCliente = value; }
        /// <summary>
        /// Tipo de calculo de precio
        /// </summary>
        public string? TipoPrecio { get => _TipoPrecio; set => _TipoPrecio = value; }
        /// <summary>
        /// Porcentaje de descuento del comprobante
        /// </summary>
        public string? DescuentoPorcentajes { get => _DescuentoPorcentajes; set => _DescuentoPorcentajes = value; }
        /// <summary>
        /// Descuento en importe del comprobante
        /// </summary>
        public decimal DescuentoImporte { get => _DescuentoImporte; set => _DescuentoImporte = value; }
        /// <summary>
        /// Descripcion del descuento
        /// </summary>
        public string? DescuentoTexto { get => _DescuentoTexto; set => _DescuentoTexto = value; }
        /// <summary>
        /// Codigo de vendedor
        /// </summary>
        public int VendedorID { get => _VendedorID; set => _VendedorID = value; }
        /// <summary>
        /// Tipo de comprobante 
        /// </summary>
        public string? TipoDeComprobante { get => _TipoDeComprobante; set => _TipoDeComprobante = value; }
        /// <summary>
        /// Hora del comprobante
        /// </summary>
        public string? Hora { get => _Hora; set => _Hora = value; }
        /// <summary>
        /// Codigo de forma de ingreso
        /// </summary>
        public int FormaIngreso { get => _FormaIngreso; set => _FormaIngreso = value; }
    }

    /// <summary>
    /// Sucursal del cliente
    /// </summary>
    public class SucursalCliente
    {
        private int _SucursalID;
        private String? _Descripcion;
        private String? _Domicilio;
        private String? _Localidad;
        private String? _DomicilioEntrega;

        /// <summary>
        /// Codigo de sucursal
        /// </summary>
        public int SucursalID { get => _SucursalID; set => _SucursalID = value; }
        /// <summary>
        /// Descripcion de la sucursal
        /// </summary>
        public string? Descripcion { get => _Descripcion; set => _Descripcion = value; }
        /// <summary>
        /// Domicilio de la sucursal
        /// </summary>
        public string? Domicilio { get => _Domicilio; set => _Domicilio = value; }
        /// <summary>
        /// Localidad de la sucursal
        /// </summary>
        public string? Localidad { get => _Localidad; set => _Localidad = value; }
        /// <summary>
        /// Domicilio de entrega de la sucursal
        /// </summary>
        public string? DomicilioEntrega { get => _DomicilioEntrega; set => _DomicilioEntrega = value; }
    }


    /// <summary>
    /// Comprobante de cobro api vieja
    /// </summary>
    public class cComprobanteCobroAPI : cComprobanteDeVentaAPI
    {
        private int _CobroID;
        private decimal _TotalCobrado;
        private List<cComprobanteAplicadoAPI>? _ComprobantesAplicados;
        private List<cValorAPI>? _Valores;
        private short _CobradorID;

        /// <summary>
        /// Codigo de cobro
        /// </summary>
        public int CobroID { get => _CobroID; set => _CobroID = value; }
        /// <summary>
        /// Importe total cobrado
        /// </summary>
        public decimal TotalCobrado { get => _TotalCobrado; set => _TotalCobrado = value; }
        /// <summary>
        /// Listado de comprobantes a cobrar
        /// </summary>
        public List<cComprobanteAplicadoAPI>? ComprobantesAplicados { get => _ComprobantesAplicados; set => _ComprobantesAplicados = value; }
        /// <summary>
        /// Lista de Medio de pago
        /// </summary>
        public List<cValorAPI>? Valores { get => _Valores; set => _Valores = value; }
        /// <summary>
        /// Codigo de cobrador
        /// </summary>
        public short CobradorID { get => _CobradorID; set => _CobradorID = value; }
    }

    /// <summary>
    /// Comprobante aplicado 
    /// </summary>
    public class cComprobanteAplicadoAPI
    {
        private int _ComprobanteID;
        private int _Numero;
        private string? _Serie;
        private int _PuntoDeVentaID;
        private int _Vto_ComprobanteID;
        private decimal _ImporteAplicado;
        private int _Vto_ItemID;

        /// <summary>
        /// Codigo de comprobante aplicado
        /// </summary>
        public int ComprobanteID { get => _ComprobanteID; set => _ComprobanteID = value; }

        /// <summary>
        /// Serie del comprobante aplicado
        /// </summary>
        public string? Serie { get => _Serie; set => _Serie = value; }

        /// <summary>
        /// Codigo de punto de venta 
        /// </summary>
        public int PuntoDeVentaID { get => _PuntoDeVentaID; set => _PuntoDeVentaID = value; }

        /// <summary>
        /// ComprobanteID del vencimiento
        /// </summary>
        public int Vto_ComprobanteID { get => _Vto_ComprobanteID; set => _Vto_ComprobanteID = value; }
        /// <summary>
        /// Importe aplicado al comprobante
        /// </summary>
        public decimal ImporteAplicado { get => _ImporteAplicado; set => _ImporteAplicado = value; }
        /// <summary>
        /// Numero de comprobante
        /// </summary>
        public int Numero { get => _Numero; set => _Numero = value; }
        /// <summary>
        /// Item del vencimiento
        /// </summary>
        public int Vto_ItemID { get => _Vto_ItemID; set => _Vto_ItemID = value; }
    }

    /// <summary>
    /// Medio de pago del cobro
    /// </summary>
    public class cValorAPI
    {
        private int _CajaBancoID;
        private int _ValorID;
        private string? _NumeroDeValor;
        private decimal _Importe;
        private DateTime _FechaDC;
        private string? _Emisor;
        private int _BancoID;

        /// <summary>
        /// Codigo de caja banco del cobro
        /// </summary>
        public int CajaBancoID { get => _CajaBancoID; set => _CajaBancoID = value; }
        /// <summary>
        /// Codigo del medio de pago
        /// </summary>
        public int ValorID { get => _ValorID; set => _ValorID = value; }
        /// <summary>
        /// Numero de valor utilizado
        /// </summary>
        public string? NumeroDeValor { get => _NumeroDeValor; set => _NumeroDeValor = value; }
        /// <summary>
        /// Importe del medio de pago
        /// </summary>
        public decimal Importe { get => _Importe; set => _Importe = value; }       
        /// <summary>
        /// Emisor del pago
        /// </summary>
        public string? Emisor { get => _Emisor; set => _Emisor = value; }
        /// <summary>
        /// Codigo del banco
        /// </summary>
        public int BancoID { get => _BancoID; set => _BancoID = value; }
        /// <summary>
        /// Fecha del credito
        /// </summary>
        public DateTime FechaDC { get => _FechaDC; set => _FechaDC = value; }
    }

    /// <summary>
    /// Cliente  del comprobante
    /// </summary>
    public class cClienteAPI
    {
        private int _ClienteID;
        private string? _razonSocial;
        private string? _fantasia;
        private string? _domicilio;
        private int _condicionIVA;
        private string? _numeroDeDocumento;
        private int _tipoDocumentoID;
        private string? _notas;
        private string? _telefono;
        private string? _email;

        /// <summary>
        /// Razon Social del cliente
        /// </summary>
        public string? razonSocial { get => _razonSocial; set => _razonSocial = value; }
        /// <summary>
        /// Nombre de fantasia 
        /// </summary>
        public string? fantasia { get => _fantasia; set => _fantasia = value; }
        /// <summary>
        /// Domicilio del cliente
        /// </summary>
        public string? domicilio { get => _domicilio; set => _domicilio = value; }
        /// <summary>
        /// Condicion IVA del cliente
        /// </summary>
        public int condicionIVA { get => _condicionIVA; set => _condicionIVA = value; }
        /// <summary>
        /// Numero de documento del cliente
        /// </summary>
        public string? numeroDeDocumento { get => _numeroDeDocumento; set => _numeroDeDocumento = value; }
        /// <summary>
        /// Codigo de tipo de documento del cliente
        /// </summary>
        public int tipoDocumentoID { get => _tipoDocumentoID; set => _tipoDocumentoID = value; }
        /// <summary>
        /// Observaciones del cliente
        /// </summary>
        public string? notas { get => _notas; set => _notas = value; }
        /// <summary>
        /// Telefono del cliente
        /// </summary>
        public string? telefono { get => _telefono; set => _telefono = value; }
        /// <summary>
        /// correo electronico del cliente
        /// </summary>
        public string? email { get => _email; set => _email = value; }
        /// <summary>
        /// Codigo del cliente
        /// </summary>
        public int ClienteID { get => _ClienteID; set => _ClienteID = value; }
    }


    /// <summary>
    /// Item de comprobante API Vieja
    /// </summary>
    public class cItemAPI
    {
        private string? _ProductoID;
        private decimal _UnidadU1;
        private decimal _UnidadU2;
        private decimal _Precio;
        private decimal _DescuentoImporte;
        private string? _DescuentoPorcentajes;
        private string? _Comentario;
        private string? _TipoDeFacturacion;

        /// <summary>
        /// Codigo de producto
        /// </summary>
        public string? ProductoID { get => _ProductoID; set => _ProductoID = value; }
        /// <summary>
        /// CantidadU1
        /// </summary>
        public decimal UnidadU1 { get => _UnidadU1; set => _UnidadU1 = value; }
        /// <summary>
        /// CantidadU2
        /// </summary>
        public decimal UnidadU2 { get => _UnidadU2; set => _UnidadU2 = value; }
        /// <summary>
        /// Precio del item
        /// </summary>
        public decimal Precio { get => _Precio; set => _Precio = value; }
        /// <summary>
        /// Descuento en importe del item
        /// </summary>
        public decimal DescuentoImporte { get => _DescuentoImporte; set => _DescuentoImporte = value; }
        /// <summary>
        /// Descuento en porcentaje del item
        /// </summary>
        public string? DescuentoPorcentajes { get => _DescuentoPorcentajes; set => _DescuentoPorcentajes = value; }
        /// <summary>
        /// Comentario del item
        /// </summary>
        public string? Comentario { get => _Comentario; set => _Comentario = value; }
        /// <summary>
        /// Tipo de facturacion
        /// </summary>
        public string? TipoDeFacturacion { get => _TipoDeFacturacion; set => _TipoDeFacturacion = value; }
    }
}

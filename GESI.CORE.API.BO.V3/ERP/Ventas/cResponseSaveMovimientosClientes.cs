namespace GESI.CORE.API.BO.V3.ERP.Ventas
{
    /// <summary>
    /// Resultado por comprobante de los Create de comprobantes de venta y de cobro (migrado de la V2, mismo JSON)
    /// </summary>
    public class cResponseSaveMovimientosClientes
    {
        private bool _success;
        private int _numero;
        private int _comprobanteID;
        private int _puntoDeVentaID;
        private string? _notas;
        private string? _notas2;
        private string? _mensaje;
        private string? _serie;
        private string? _subTipo;
        private int _clienteID;
        private string? _razonSocial;
        private string? _codigoDeImportacion;

        public bool success { get => _success; set => _success = value; }
        public int numero { get => _numero; set => _numero = value; }
        public int comprobanteID { get => _comprobanteID; set => _comprobanteID = value; }
        public int puntoDeVentaID { get => _puntoDeVentaID; set => _puntoDeVentaID = value; }
        public string? notas { get => _notas; set => _notas = value; }
        public string? notas2 { get => _notas2; set => _notas2 = value; }
        public string? mensaje { get => _mensaje; set => _mensaje = value; }
        public string? serie { get => _serie; set => _serie = value; }
        public string? subTipo { get => _subTipo; set => _subTipo = value; }
        public int clienteID { get => _clienteID; set => _clienteID = value; }
        public string? razonSocial { get => _razonSocial; set => _razonSocial = value; }
        public string? codigoDeImportacion { get => _codigoDeImportacion; set => _codigoDeImportacion = value; }

        public cResponseSaveMovimientosClientes()
        {

        }

        public cResponseSaveMovimientosClientes(cMovimientoDeCliente oMovimientoCliente, bool _success, string _mensaje)
        {
            success = _success;
            numero = oMovimientoCliente.Numero;
            comprobanteID = oMovimientoCliente.ComprobanteID;
            puntoDeVentaID = oMovimientoCliente.PuntoDeVentaID;
            notas = oMovimientoCliente.Notas;
            mensaje = _mensaje;
            serie = oMovimientoCliente.Serie;
            subTipo = oMovimientoCliente.Subtipo;
        }
    }
}

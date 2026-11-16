using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Compras.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Compras
{
    /// <summary>
    /// Respuesta con comprobantes de compra (facturas, notas de credito/debito, ordenes de compra/pago, remitos)
    /// </summary>
    public class cResponseMovimientoProveedores : cResponse
    {
        /// <summary>
        /// Lista de comprobantes de proveedores
        /// </summary>
        public List<cMovimientoProveedor>? movimientosDeProveedores { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Body de los GetList de comprobantes de compra
    /// </summary>
    public class dParametrosConsultaComprobantesCompra
    {
        /// <summary>
        /// Filtros de la operacion (fechas, numeros, clase de comprobante, etc.)
        /// </summary>
        public dParametroConsultaComprobantesDatosOperacion? datosOperacion { get; set; }

        /// <summary>
        /// Filtros del proveedor
        /// </summary>
        public dParametroConsultaComprobantesDatosProveedores? datosProveedores { get; set; }
    }

    /// <summary>
    /// Resultado por comprobante del Create de ordenes de compra (migrado de la V2, mismo JSON)
    /// </summary>
    public class cResponseSaveMovimientosProveedores
    {
        /// <summary>
        /// Determina si fue exitoso
        /// </summary>
        public bool success { get; set; }
        /// <summary>
        /// Numero de comprobante generado en ERP
        /// </summary>
        public int numero { get; set; }
        /// <summary>
        /// Codigo de comprobante
        /// </summary>
        public int comprobanteID { get; set; }
        /// <summary>
        /// Punto de venta del comprobante generado
        /// </summary>
        public int puntoDeVentaID { get; set; }
        /// <summary>
        /// Observaciones 1 del comprobante
        /// </summary>
        public string? notas { get; set; }
        /// <summary>
        /// Observaciones 2 del comprobante
        /// </summary>
        public string? notas2 { get; set; }
        /// <summary>
        /// Mensaje de advertencia en la importacion
        /// </summary>
        public string? mensaje { get; set; }
        /// <summary>
        /// Serie del comprobante
        /// </summary>
        public string? serie { get; set; }
        /// <summary>
        /// Codigo de proveedor
        /// </summary>
        public int proveedorID { get; set; }
        /// <summary>
        /// Razon social del proveedor
        /// </summary>
        public string? razonSocial { get; set; }
        /// <summary>
        /// Codigo de importacion del comprobante
        /// </summary>
        public string? codigoDeImportacion { get; set; }

        /// <summary>
        /// Constructor generico
        /// </summary>
        public cResponseSaveMovimientosProveedores()
        {

        }

        /// <summary>
        /// Constructor a partir del comprobante grabado
        /// </summary>
        public cResponseSaveMovimientosProveedores(cMovimientoProveedor oMovimientoProveedor, bool _success, string _mensaje)
        {
            success = _success;
            numero = oMovimientoProveedor.Numero;
            comprobanteID = oMovimientoProveedor.ComprobanteID;
            puntoDeVentaID = oMovimientoProveedor.PuntoDeVentaID;
            notas = oMovimientoProveedor.Notas;
            mensaje = _mensaje;
            serie = oMovimientoProveedor.Serie;
            proveedorID = oMovimientoProveedor.ProveedorID;
            codigoDeImportacion = oMovimientoProveedor.CodigoDeImportacion;
        }
    }
}

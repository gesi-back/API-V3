using GESI.CORE.API.BO.V3.API;

namespace GESI.CORE.API.BO.V3.ERP.Ventas
{
    /// <summary>
    /// Foja de reparto con el resultado de la operación (en la V2 se llamaba GESI.CORE.API.BO.cFojaDeReparto; mismo JSON)
    /// </summary>
    public class cFojaDeRepartoAPI : GESI.ERP.Ventas.V3.BO.cFojaDeReparto
    {
        /// <summary>
        /// Determina si la foja se grabó / devolvió correctamente
        /// </summary>
        public bool success { get; set; }

        public cFojaDeRepartoAPI(GESI.ERP.Ventas.V3.BO.cFojaDeReparto foja, bool success)
        {
            this.EmpresaID = foja.EmpresaID;
            this.FojaID = foja.FojaID;
            this.ZonaID = foja.ZonaID;
            this.DescripcionZona = foja.DescripcionZona;
            this.SubZonaID = foja.SubZonaID;
            this.DescripcionSubZona = foja.DescripcionSubZona;
            this.RepartidorID = foja.RepartidorID;
            this.DescripcionRepartidor = foja.DescripcionRepartidor;
            this.Estado = foja.Estado;
            this.Desde = foja.Desde;
            this.Hasta = foja.Hasta;
            this.Comentario = foja.Comentario;
            this.FechaCerrada = foja.FechaCerrada;
            this.ComentarioExpedicion = foja.ComentarioExpedicion;
            this.FechaExpedido = foja.FechaExpedido;
            this.ListaDeComprobantesAsignados = foja.ListaDeComprobantesAsignados;
            this.success = success;
        }
    }

    /// <summary>
    /// Respuesta del GetList de fojas de reparto
    /// </summary>
    public class cResponseFojasDeRepartos : cResponse
    {
        public List<cFojaDeRepartoAPI>? FojasReparto { get; set; }

        public cPaginacion? Paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta del GetItem de una foja de reparto
    /// </summary>
    public class cResponseFojaDeReparto : cResponse
    {
        public GESI.ERP.Ventas.V3.BO.cFojaDeReparto? fojaReparto { get; set; }

        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta del Create de fojas de reparto
    /// </summary>
    public class cResponseFojasDeRepartosSave : cResponse
    {
        public List<cFojaDeRepartoAPI>? fojasReparto { get; set; }

        public cPaginacion? paginacion { get; set; }

        public int FojasProcesadas { get; set; }

        public int FojasImportadas { get; set; }

        public int FojasConErrores { get; set; }
    }

    /// <summary>
    /// Respuesta de GetProducto: si el producto está en la foja
    /// </summary>
    public class cResponseGetProductoEncontradoFoja : cResponse
    {
        public bool productoEncontradoEnFoja { get; set; }

        public string? codigo { get; set; }
    }

    /// <summary>
    /// Respuesta de CompararProductos y CerrarCargaFoja
    /// </summary>
    public class cResponseCompararProductos : cResponse
    {
        public List<cResultadoCompararProducto>? resultado { get; set; }
    }

    /// <summary>
    /// Diferencia de cantidad de un producto contra la foja
    /// </summary>
    public class cResultadoCompararProducto
    {
        public string? productoID { get; set; }

        public decimal unidad1 { get; set; }

        public cResultadoCompararProducto()
        {
        }

        public cResultadoCompararProducto(string productoID, decimal unidad1)
        {
            this.productoID = productoID;
            this.unidad1 = unidad1;
        }
    }

    /// <summary>
    /// Body de CerrarCargaFoja: productos con sus cantidades y comentario
    /// </summary>
    public class cCierreFoja
    {
        public Dictionary<string, decimal>? productos { get; set; }

        public string? comentario { get; set; }
    }
}

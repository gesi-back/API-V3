using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta productos
    /// </summary>
    public class cResponseProductos : cResponse
    {
        private List<cProducto>? _productos;
        private cPaginacion? _paginacion;

        /// <summary>
        /// Listado de productos
        /// </summary>
        public List<cProducto>? productos { get => _productos; set => _productos = value; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get => _paginacion; set => _paginacion = value; }
    }

    /// <summary>
    /// Respuesta de producto
    /// </summary>
    public class cResponseProducto : cResponse
    {
        private cProducto? _producto;
        private cPaginacion? _paginacion;

        /// <summary>
        /// Producto devuelto
        /// </summary>
        public cProducto? producto { get => _producto; set => _producto = value; }
        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get => _paginacion; set => _paginacion = value; }
    }

    /// <summary>
    /// Respuesta de precios
    /// </summary>
    public class cResponsePrecios : cResponse
    {
        private List<dResultadoConsultaPrecio>? _precios;
        private cPaginacion? _paginacion;

        /// <summary>
        /// Listado de precios
        /// </summary>
        public List<dResultadoConsultaPrecio>? precios { get => _precios; set => _precios = value; }
        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get => _paginacion; set => _paginacion = value; }
    }

    public class cResponseExistencias : cResponse
    {
        private List<dExistenciaProducto>? _existencias;
        private cPaginacion? _paginacion;

        public List<dExistenciaProducto>? existencias { get => _existencias; set => _existencias = value; }
        public cPaginacion? paginacion { get => _paginacion; set => _paginacion = value; }
    }

    public class cResponseAplicacionesProducto : cResponse
    {
        private List<cAplicacionesProducto>? _aplicacionProductos;
        private cPaginacion? _paginacion;

        public List<cAplicacionesProducto>? aplicacionProductos { get => _aplicacionProductos; set => _aplicacionProductos = value; }
        public cPaginacion? paginacion { get => _paginacion; set => _paginacion = value; }
    }

    public class cResponseGrupoDeArticulos : cResponse, IRespuestaPaginada
    {
        private List<cGrupoDeArticulos>? _grupoArticulos;
        private cPaginacion? _paginacion;

        public List<cGrupoDeArticulos>? grupoArticulos { get => _grupoArticulos; set => _grupoArticulos = value; }
        public cPaginacion? paginacion { get => _paginacion; set => _paginacion = value; }
    }

    public class cResponseGrupoDeArticulo : cResponse
    {
        private cGrupoDeArticulos? _grupoArticulo;
        private cPaginacion? _paginacion;

        public cGrupoDeArticulos? grupoArticulos { get => _grupoArticulo; set => _grupoArticulo = value; }
        public cPaginacion? paginacion { get => _paginacion; set => _paginacion = value; }
    }

    public class cResponseHistorialCostos :cResponse
    {
        private List<cCostoHistorico>? _historialCostos;
        private cPaginacion? _paginacion;

        public List<cCostoHistorico>? historialCostos { get => _historialCostos; set => _historialCostos = value; }
        public cPaginacion? paginacion { get => _paginacion; set => _paginacion = value; }
    }

    public class cResponseHistorialDePrecios : cResponse
    {
        private List<cHistorialPrecio>? _historialPrecios;
        private cPaginacion? _paginacion;

        public List<cHistorialPrecio>? historialPrecios { get => _historialPrecios; set => _historialPrecios = value; }
        public cPaginacion? paginacion { get => _paginacion; set => _paginacion = value; }
    }

    public class cResponseProductosSave : cResponse
    {       
        private string? _productoID;
        private string? _descripcion;        
        private string? _estado;
        private int _alicuotaIVA;
        private int? _elementoID;

        public string? productoID { get => _productoID; set => _productoID = value; }
        public string? descripcion { get => _descripcion; set => _descripcion = value; }        
        public string? estado { get => _estado; set => _estado = value; }
        public int alicuotaIVA { get => _alicuotaIVA; set => _alicuotaIVA = value; }
        public int? elementoID { get => _elementoID; set => _elementoID = value; }

        public cResponseProductosSave() { }

        public cResponseProductosSave(bool success,
                                        string? productoID, 
                                        string? descripcion, 
                                        string? mensaje, 
                                        string? estado, 
                                        int alicuotaIVA,
                                        int elementoID)
        {
            this.success = success;
            this.productoID = productoID;
            this.descripcion = descripcion;
            if(mensaje != null && mensaje.Length > 0)
            {
                this.error = new cError();
                this.error.message = mensaje;
            }
            this.estado = estado;
            this.alicuotaIVA = alicuotaIVA;
            this.productoID = productoID;
            this.descripcion = descripcion;            
            this.estado = estado;
            this.alicuotaIVA = alicuotaIVA;
            this.elementoID = elementoID;
        }
    }

}

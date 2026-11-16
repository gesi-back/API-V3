using GESI.CORE.API.BO.V3.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Clase de producto
    /// </summary>
    public class cProducto: GESI.ERP.Core.V3.BO.cProducto
    {
        /// <summary>
        /// Codigo de empresa
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        

        public override short EmpresaID { get => base.EmpresaID; set => base.EmpresaID = value; }

        /// <summary>
        /// Array con codigos de categorias
        /// </summary>
        public List<int> ListaDeCategorias { get => _CodigosCategorias; set => _CodigosCategorias = value; }

        private List<int> _CodigosCategorias;

        /// <summary>
        /// List ade categorias
        /// </summary>

        [System.Text.Json.Serialization.JsonIgnore]
        
        public override List<GESI.ERP.Core.V3.BO.cCategoriaXProducto> Categorias { get => base.Categorias; set => base.Categorias = value; }


        /// <summary>
        /// Constructor heredado
        /// </summary>
        /// <param name="padre"></param>
        public cProducto(GESI.ERP.Core.V3.BO.cProducto padre)
        {
            EmpresaID = padre.EmpresaID;
            ProductoID = padre.ProductoID;
            Descripcion = padre.Descripcion;
            DescripcionExtendida = padre.DescripcionExtendida;
            DescripcionReducida = padre.DescripcionReducida;
            AlicuotaIVA = padre.AlicuotaIVA;
            RubroID = padre.RubroID;
            SubRubroID = padre.SubRubroID;
            SubSubRubroID = padre.SubSubRubroID;
            FiltroArticulos1ID = padre.FiltroArticulos1ID;
            FiltroArticulos2ID = padre.FiltroArticulos2ID;
            FiltroArticulos3ID = padre.FiltroArticulos3ID;
            Unidad2XUnidad1 = padre.Unidad2XUnidad1;
            Unidad2XUnidad1Confirmar = padre.Unidad2XUnidad1Confirmar;
            CostosProveedores = padre.CostosProveedores;
            Imagenes = padre.Imagenes;
            Precios = padre.Precios;
            Estado = padre.Estado;
            GrupoArtID = padre.GrupoArtID;
            ListaDeCategorias = new List<int>();
            Existencias = padre.Existencias;
            Largo = padre.Largo;
            Ancho = padre.Ancho;
            Alto = padre.Alto;
            Peso = padre.Peso;
            PublicaECommerce = padre.PublicaECommerce;
            Fraccionable = padre.Fraccionable;
            TipoDeFacturacion = padre.TipoDeFacturacion;
            Aplicaciones = padre.Aplicaciones;
            AdicionalesAlCosto = padre.AdicionalesAlCosto;
            CodigosAdicionales = padre.CodigosAdicionales;
            DefinicionesXAlmacen = padre.DefinicionesXAlmacen;
            Insumos = padre.Insumos;
            Impuestos = padre.Impuestos;
            Cuentas = padre.Cuentas;
            Unidad1 = padre.Unidad1;
            Unidad2 = padre.Unidad2;
            CantidadMinimaAFacturar = padre.CantidadMinimaAFacturar;
            CantidadMaximaAFacturar = padre.CantidadMaximaAFacturar;
            FraccionMinima = padre.FraccionMinima;
            ConceptoIVAID = padre.ConceptoIVAID;
            UnidadIdentificada = padre.UnidadIdentificada;
            Lotes = padre.Lotes;
            Validez = padre.Validez;
            Combinado = padre.Combinado;
            CombinadoCostoDeLosComponentes = padre.CombinadoCostoDeLosComponentes;
            EsEnvase = padre.EsEnvase;
            EsInsumo = padre.EsInsumo;
            AgenteObligatorioVentas = padre.AgenteObligatorioVentas;
            RequiereReferenciaEnEncabezado = padre.RequiereReferenciaEnEncabezado;
            VenderSinExistenciaSiUsuarioAutorizado = padre.VenderSinExistenciaSiUsuarioAutorizado;
            GiftCard = padre.GiftCard;
            NoControlarExistenciasEnRTONoConfirmado = padre.NoControlarExistenciasEnRTONoConfirmado;
            CategoriaComisionID = padre.CategoriaComisionID;
            CriterioLogisticaID = padre.CriterioLogisticaID;
            TipoDeDescuentoID = padre.TipoDeDescuentoID;
            ConceptoID = padre.ConceptoID;
            NoAplicarRecargosFinancieros = padre.NoAplicarRecargosFinancieros;
            ComentariosEnVentas = padre.ComentariosEnVentas;
            UnidadesPorCaja = padre.UnidadesPorCaja;
            MinimoAFacturar = padre.MinimoAFacturar;
            ImprimeCuponVenta = padre.ImprimeCuponVenta;
            FormatoImpresionNPE = padre.FormatoImpresionNPE;
            CantidaddeCopiasCuponVenta = padre.CantidaddeCopiasCuponVenta;
            DiasParaReposicion = padre.DiasParaReposicion;
            TomarCostoProductoID = padre.TomarCostoProductoID;
            TomarCostoProductoAjuste = padre.TomarCostoProductoAjuste;
            Elementos = padre.Elementos;
            SubProductos = padre.SubProductos;
            Embalaje = padre.Embalaje;
            Costo = padre.Costo;
            Rubro = padre.Rubro;
            Subrubro = padre.Subrubro;
            Subsubrubro = padre.Subsubrubro;
            noPermitirDescuento = padre.noPermitirDescuento;
            sucursalesHabilitadas = padre.sucursalesHabilitadas;
            Destacado = padre.Destacado;
            LotesProducto = padre.LotesProducto;
            EsUnSubProductoDe = padre.EsUnSubProductoDe;
        }



       
    }

    
    /// <summary>
    /// Aplicaciones de productos
    /// </summary>
    public class cAplicacionesProducto : GESI.ERP.Core.V3.BO.cAplicacionProducto
    {
        /// <summary>
        /// Codigo de empresa
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        

        public override int EmpresaID { get => base.EmpresaID; set => base.EmpresaID = value; }
        
        /// <summary>
        /// Constructor original
        /// </summary>
        public cAplicacionesProducto()
        { }

        /// <summary>
        /// Constructor heredado
        /// </summary>
        /// <param name="padre"></param>
        public cAplicacionesProducto(GESI.ERP.Core.V3.BO.cAplicacionProducto padre)
        {
            AplicacionID = padre.AplicacionID;
            Descripcion = padre.Descripcion;
        }

    }


    /// <summary>
    /// Objeto de actualizacion de costo
    /// </summary>
    public class cUpdateCostoProducto : GESI.ERP.Core.V3.BO.cCostoProductoProveedor
    {
        private cError? _error;
        private bool _success;

        /// <summary>
        /// Objeto de error
        /// </summary>
        public cError? error { get => _error; set => _error = value; }
        /// <summary>
        /// Confirmacion de success
        /// </summary>
        public bool success { get => _success; set => _success = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class cActualizacionCosto
    {
        private int _proveedorID;

        private List<cUpdateCostoProducto>? _costos;

        /// <summary>
        /// Codigo de proveedor
        /// </summary>

        public int proveedorID { get => _proveedorID; set => _proveedorID = value; }
        /// <summary>
        /// Listado de actualizacion de costos
        /// </summary>
        public List<cUpdateCostoProducto>? costos { get => _costos; set => _costos = value; }
    }


}

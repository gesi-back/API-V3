using GESI.CORE.API.BO.V3.API;
using System.Text.Json.Serialization;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    #region Objetos expuestos (el del Core sin EmpresaID, igual que en la V2)

    /// <summary>
    /// Rubro tal como lo expone la API: igual al del Core pero sin EmpresaID. Equivale al cRubro de la API V2.
    /// </summary>
    public class cRubroAPI : GESI.ERP.Core.V3.BO.cRubro
    {
        /// <summary>
        /// Codigo de empresa (no se serializa)
        /// </summary>
        [JsonIgnore]
        public override int EmpresaID { get => base.EmpresaID; set => base.EmpresaID = value; }

        /// <summary>
        /// Constructor vacio
        /// </summary>
        public cRubroAPI() { }

        /// <summary>
        /// Copia un rubro del Core
        /// </summary>
        public cRubroAPI(GESI.ERP.Core.V3.BO.cRubro padre)
        {
            EmpresaID = padre.EmpresaID;
            RubroID = padre.RubroID;
            Descripcion = padre.Descripcion;
            CategoriaID = padre.CategoriaID;
        }
    }

    /// <summary>
    /// SubRubro tal como lo expone la API: igual al del Core pero sin EmpresaID. Equivale al cSubRubro de la API V2.
    /// </summary>
    public class cSubRubroAPI : GESI.ERP.Core.V3.BO.cSubRubro
    {
        /// <summary>
        /// Codigo de empresa (no se serializa)
        /// </summary>
        [JsonIgnore]
        public override int EmpresaID { get => base.EmpresaID; set => base.EmpresaID = value; }

        /// <summary>
        /// Constructor vacio
        /// </summary>
        public cSubRubroAPI() { }

        /// <summary>
        /// Copia un subrubro del Core
        /// </summary>
        public cSubRubroAPI(GESI.ERP.Core.V3.BO.cSubRubro padre)
        {
            EmpresaID = padre.EmpresaID;
            RubroID = padre.RubroID;
            SubRubroID = padre.SubRubroID;
            Descripcion = padre.Descripcion;
            CategoriaID = padre.CategoriaID;
        }
    }

    /// <summary>
    /// SubSubRubro tal como lo expone la API: igual al del Core pero sin EmpresaID. Equivale al cSubSubRubro de la API V2.
    /// </summary>
    public class cSubSubRubroAPI : GESI.ERP.Core.V3.BO.cSubSubRubro
    {
        /// <summary>
        /// Codigo de empresa (no se serializa)
        /// </summary>
        [JsonIgnore]
        public override int EmpresaID { get => base.EmpresaID; set => base.EmpresaID = value; }

        /// <summary>
        /// Constructor vacio
        /// </summary>
        public cSubSubRubroAPI() { }

        /// <summary>
        /// Copia un subsubrubro del Core
        /// </summary>
        public cSubSubRubroAPI(GESI.ERP.Core.V3.BO.cSubSubRubro padre)
        {
            EmpresaID = padre.EmpresaID;
            RubroID = padre.RubroID;
            SubRubroID = padre.SubRubroID;
            SubSubRubroID = padre.SubSubRubroID;
            Descripcion = padre.Descripcion;
            CategoriaID = padre.CategoriaID;
        }
    }

    #endregion

    #region Respuestas

    /// <summary>
    /// Respuesta con rubros
    /// </summary>
    public class cResponseRubros : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de rubros
        /// </summary>
        public List<cRubroAPI>? rubros { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con subrubros
    /// </summary>
    public class cResponseSubRubros : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de subrubros
        /// </summary>
        public List<cSubRubroAPI>? subRubros { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con subsubrubros
    /// </summary>
    public class cResponseSubSubRubros : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de subsubrubros (el nombre "subsubRubros" se mantiene igual que en la V2)
        /// </summary>
        public List<cSubSubRubroAPI>? subsubRubros { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    #endregion
}

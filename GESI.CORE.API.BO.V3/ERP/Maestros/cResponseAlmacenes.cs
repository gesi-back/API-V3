using GESI.CORE.API.BO.V3.API;
using System.Text.Json.Serialization;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Almacen tal como lo expone la API: igual al del Core pero sin EmpresaID
    /// (la empresa sale de la sesión, no se le devuelve al cliente). Equivale al cAlmacen de la API V2.
    /// </summary>
    public class cAlmacenAPI : GESI.ERP.Core.V3.BO.cAlmacen
    {
        /// <summary>
        /// Codigo de empresa (no se serializa)
        /// </summary>
        [JsonIgnore]
        public override ushort EmpresaID { get => base.EmpresaID; set => base.EmpresaID = value; }

        /// <summary>
        /// Constructor vacio
        /// </summary>
        public cAlmacenAPI() { }

        /// <summary>
        /// Copia un almacen del Core
        /// </summary>
        public cAlmacenAPI(GESI.ERP.Core.V3.BO.cAlmacen padre)
        {
            EmpresaID = padre.EmpresaID;
            AlmacenID = padre.AlmacenID;
            Descripcion = padre.Descripcion;
            Domicilio = padre.Domicilio;
            Localidad = padre.Localidad;
            CP = padre.CP;
            Telefono = padre.Telefono;
            FAX = padre.FAX;
            RegionID = padre.RegionID;
            PaisID = padre.PaisID;
            CorreoElectronico = padre.CorreoElectronico;
            SucursalID = padre.SucursalID;
            AvisoAlRecibirTransferencias = padre.AvisoAlRecibirTransferencias;
            Estado = padre.Estado;
            ControlTemporalDeTransferencias = padre.ControlTemporalDeTransferencias;
            Secciones = padre.Secciones;
        }
    }

    /// <summary>
    /// Respuesta con almacenes
    /// </summary>
    public class cResponseAlmacenes : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de almacenes
        /// </summary>
        public List<cAlmacenAPI>? almacenes { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con un almacen
    /// </summary>
    public class cResponseAlmacen : cResponse
    {
        /// <summary>
        /// Objeto de almacen
        /// </summary>
        public cAlmacenAPI? almacen { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface ISubSubRubrosServicio
    {
        cResponseSubSubRubros GetList(int pageNumber, int pageSize);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.SubSubRubrosMgr estático con _MIAPISessionMgr estático).
    /// Devuelve cSubSubRubroAPI (el subsubrubro del Core sin EmpresaID), igual que la V2.
    /// </summary>
    internal sealed class SubSubRubrosServicio(
        ISubSubRubrosBLL subSubRubrosBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), ISubSubRubrosServicio
    {
        public cResponseSubSubRubros GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseSubSubRubros(),
                       () => subSubRubrosBLL.GetList((uint)Sesion.EmpresaID),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.SubSubRubros("GetList"),
                       nombreLog: "SubSubRubros GetList",
                       mensajeNoEncontrado: "No se encontraron subsubrubros",
                       asignarItems: (r, items) => r.subsubRubros = items.Select(x => new cSubSubRubroAPI(x)).ToList());
    }
}

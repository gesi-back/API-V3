using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IRubrosServicio
    {
        cResponseRubros GetList(int pageNumber, int pageSize);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.RubrosMgr estático con _MIAPISessionMgr estático).
    /// Devuelve cRubroAPI (el rubro del Core sin EmpresaID), igual que la V2.
    /// </summary>
    internal sealed class RubrosServicio(
        IRubrosBLL rubrosBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IRubrosServicio
    {
        public cResponseRubros GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseRubros(),
                       () => rubrosBLL.GetList((uint)Sesion.EmpresaID),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.Rubros("GetList"),
                       nombreLog: "Rubros GetList",
                       mensajeNoEncontrado: "No se encontraron rubros",
                       asignarItems: (r, items) => r.rubros = items.Select(x => new cRubroAPI(x)).ToList());
    }
}

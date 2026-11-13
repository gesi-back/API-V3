using GESI.CORE.API.BO.V3;
using GESI.ERP.Core.V3.BLL;

namespace GESI.CORE.API.BLL.V3
{
    public interface ISubRubrosServicio
    {
        cResponseSubRubros GetList(int pageNumber, int pageSize);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.SubRubrosMgr estático con _MIAPISessionMgr estático).
    /// Devuelve cSubRubroAPI (el subrubro del Core sin EmpresaID), igual que la V2.
    /// </summary>
    internal sealed class SubRubrosServicio(
        ISubRubrosBLL subRubrosBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), ISubRubrosServicio
    {
        public cResponseSubRubros GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseSubRubros(),
                       () => subRubrosBLL.GetList((uint)Sesion.EmpresaID),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.SubRubros("GetList"),
                       nombreLog: "SubRubros GetList",
                       mensajeNoEncontrado: "No se encontraron subrubros",
                       asignarItems: (r, items) => r.subRubros = items.Select(x => new cSubRubroAPI(x)).ToList());
    }
}

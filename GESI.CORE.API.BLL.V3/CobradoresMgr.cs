using GESI.CORE.API.BO.V3;
using GESI.ERP.Core.V3.BLL;

namespace GESI.CORE.API.BLL.V3
{
    public interface ICobradoresServicio
    {
        cResponseCobradores GetList(int pageNumber, int pageSize);
    }

    /// <summary>
    /// Migrado de la V2 (CobradoresMgr estático con _MiApiSessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class CobradoresServicio(
        ICobradoresBLL cobradoresBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), ICobradoresServicio
    {
        public cResponseCobradores GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseCobradores(),
                       () => cobradoresBLL.GetList(),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.Cobradores("GetList"),
                       nombreLog: "Cobradores GetList",
                       mensajeNoEncontrado: "No se encontraron cobradores",
                       asignarItems: (r, items) => r.cobradores = items);
    }
}

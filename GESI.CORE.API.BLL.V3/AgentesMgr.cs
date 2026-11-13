using GESI.CORE.API.BO.V3;
using GESI.ERP.Core.V3.BLL;

namespace GESI.CORE.API.BLL.V3
{
    public interface IAgentesServicio
    {
        cResponseAgentes GetList(int pageNumber, int pageSize, int vendedorID, int agenteID);
    }

    /// <summary>
    /// Migrado de la V2 (AgentesMgr estático con _MIAPISessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class AgentesServicio(
        IAgentesBLL agentesBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IAgentesServicio
    {
        public cResponseAgentes GetList(int pageNumber, int pageSize, int vendedorID, int agenteID)
            => Paginar(new cResponseAgentes(),
                       () => agentesBLL.GetList(vendedorID, agenteID),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.Agentes("GetList"),
                       nombreLog: "Agentes GetList",
                       mensajeNoEncontrado: "No se encontraron agentes",
                       asignarItems: (r, items) => r.agentes = items);
    }
}

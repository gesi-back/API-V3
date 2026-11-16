using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface ITiposDeClienteServicio
    {
        cResponseTiposDeClientes GetList(int pageNumber, int pageSize);
    }

    /// <summary>
    /// Migrado de la V2 (TiposDeClientesMgr estático con _MIAPISessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class TiposDeClienteServicio(
        ITiposDeClienteBLL tiposDeClienteBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), ITiposDeClienteServicio
    {
        public cResponseTiposDeClientes GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseTiposDeClientes(),
                       () => tiposDeClienteBLL.GetList((uint)Sesion.EmpresaID),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.TiposDeCliente("GetList"),
                       nombreLog: "TiposDeClientes GetList",
                       mensajeNoEncontrado: "No se encontraron tipos de cliente",
                       // Igual que la V2: cada tipo sale con la empresa de la sesión
                       asignarItems: (r, items) =>
                       {
                           items.ForEach(t => t.EmpresaID = Sesion.EmpresaID);
                           r.tiposDeClientes = items;
                       });
    }
}

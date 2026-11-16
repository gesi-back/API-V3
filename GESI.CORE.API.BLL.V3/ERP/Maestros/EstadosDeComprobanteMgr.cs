using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IEstadosDeComprobanteServicio
    {
        cResponseEstadosComprobantes GetList(int pageNumber, int pageSize);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.EstadosDeComprobantesMgr estático con _MIAPISessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class EstadosDeComprobanteServicio(
        IEstadosDeComprobantesDeVentaBLL estadosBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IEstadosDeComprobanteServicio
    {
        public cResponseEstadosComprobantes GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseEstadosComprobantes(),
                       () => estadosBLL.GetList(),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.EstadosDeComprobantes("GetList"),
                       nombreLog: "EstadosDeComprobante GetList",
                       mensajeNoEncontrado: "No se encontraron estados de comprobantes",
                       asignarItems: (r, items) => r.estadosDeComprobantes = items);
    }
}

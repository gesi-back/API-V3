using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IFiltroArticulos1Servicio
    {
        cResponseFiltroArticulos1 GetList(int pageNumber, int pageSize);
    }

    public interface IFiltroArticulos2Servicio
    {
        cResponseFiltroArticulos2 GetList(int pageNumber, int pageSize);
    }

    public interface IFiltroArticulos3Servicio
    {
        cResponseFiltroArticulos3 GetList(int pageNumber, int pageSize);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.FiltroArticulos1Mgr estático con _MIAPISessionMgr estático).
    /// </summary>
    internal sealed class FiltroArticulos1Servicio(
        IFiltroArticulos1BLL filtroBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IFiltroArticulos1Servicio
    {
        public cResponseFiltroArticulos1 GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseFiltroArticulos1(),
                       () => filtroBLL.GetList((uint)Sesion.EmpresaID),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.FiltroArticulos1("GetList"),
                       nombreLog: "FiltroArticulos1 GetList",
                       mensajeNoEncontrado: "No se encontraron filtros articulos 1",
                       asignarItems: (r, items) => r.filtroarticulos1 = items);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.FiltroArticulos2Mgr estático con _MIAPISessionMgr estático).
    /// </summary>
    internal sealed class FiltroArticulos2Servicio(
        IFiltroArticulos2BLL filtroBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IFiltroArticulos2Servicio
    {
        public cResponseFiltroArticulos2 GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseFiltroArticulos2(),
                       () => filtroBLL.GetList((uint)Sesion.EmpresaID),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.FiltroArticulos2("GetList"),
                       nombreLog: "FiltroArticulos2 GetList",
                       mensajeNoEncontrado: "No se encontraron filtros articulos 2",
                       asignarItems: (r, items) => r.filtroarticulos2 = items);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.FiltroArticulos3Mgr estático con _MIAPISessionMgr estático).
    /// </summary>
    internal sealed class FiltroArticulos3Servicio(
        IFiltroArticulos3BLL filtroBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IFiltroArticulos3Servicio
    {
        public cResponseFiltroArticulos3 GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseFiltroArticulos3(),
                       () => filtroBLL.GetList((uint)Sesion.EmpresaID),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.FiltroArticulos3("GetList"),
                       nombreLog: "FiltroArticulos3 GetList",
                       mensajeNoEncontrado: "No se encontraron filtros articulos 3",
                       asignarItems: (r, items) => r.filtroarticulos3 = items);
    }
}

using GESI.CORE.API.BO.V3;
using GESI.ERP.Core.V3.BLL;

namespace GESI.CORE.API.BLL.V3
{
    public interface IZonasServicio
    {
        cResponseZonas GetList(int pageNumber, int pageSize);
    }

    /// <summary>
    /// Migrado de la V2 (Repartos.BLL.ZonasMgr estático con _MiApiSessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class ZonasServicio(
        IZonaBLL zonaBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IZonasServicio
    {
        public cResponseZonas GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseZonas(),
                       () => zonaBLL.GetList(),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.Zonas("GetList"),
                       nombreLog: "Zonas GetList",
                       mensajeNoEncontrado: "No se encontraron Zonas",
                       asignarItems: (r, items) => r.zonas = items);
    }
}

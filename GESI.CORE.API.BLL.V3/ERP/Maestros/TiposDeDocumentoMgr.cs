using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.CORE.V3.BLL;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface ITiposDeDocumentoServicio
    {
        cResponseTipoDocumento GetList(int pageNumber, int pageSize);
    }

    /// <summary>
    /// Migrado de la V2 (TiposDeDocumentoMgr estático con _MIAPISessionMgr estático).
    /// Los tipos de documento son del CORE (no del ERP), por eso el BLL es GESI.CORE.V3.BLL.
    /// </summary>
    internal sealed class TiposDeDocumentoServicio(
        ITiposDeDocumentoBLL tiposDeDocumentoBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), ITiposDeDocumentoServicio
    {
        public cResponseTipoDocumento GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseTipoDocumento(),
                       () => tiposDeDocumentoBLL.GetList(),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.TiposDeDocumento("GetList"),
                       nombreLog: "TiposDeDocumento GetList",
                       mensajeNoEncontrado: "No se encontraron tipos de documento",
                       asignarItems: (r, items) => r.tiposDeDocumento = items);
    }
}

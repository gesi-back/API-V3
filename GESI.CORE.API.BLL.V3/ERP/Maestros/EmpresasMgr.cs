using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IEmpresasServicio
    {
        cResponseEmpresas GetList(int pageNumber, int pageSize, string? datosCompletos);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.EmpresasMgr estático con _MiApiSessionMgr estático).
    /// Devuelve las empresas habilitadas para el usuario de la sesión.
    /// </summary>
    internal sealed class EmpresasServicio(
        IEmpresasBLL empresasBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IEmpresasServicio
    {
        public cResponseEmpresas GetList(int pageNumber, int pageSize, string? datosCompletos)
        {
            // "S" (sin importar mayúsculas) → datos completos; cualquier otro valor → solo IDs y descripciones
            bool soloIDsYDescripciones = !string.Equals(datosCompletos?.Trim(), "S", StringComparison.OrdinalIgnoreCase);

            return Paginar(new cResponseEmpresas(),
                           () => empresasBLL.GetListByUsuario(Sesion.Usuario, soloIDsYDescripciones),
                           pageNumber, pageSize,
                           endpoint: Endpoint.Maestros.Empresas("GetList"),
                           nombreLog: "Empresas GetList",
                           mensajeNoEncontrado: "No se encontraron empresas",
                           asignarItems: (r, items) => r.empresas = items);
        }
    }
}

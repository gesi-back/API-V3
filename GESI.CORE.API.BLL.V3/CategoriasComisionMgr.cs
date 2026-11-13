using GESI.CORE.API.BO.V3;
using GESI.ERP.Core.V3.BLL;

namespace GESI.CORE.API.BLL.V3
{
    public interface ICategoriasComisionServicio
    {
        cResponseCategoriasComision GetList(int pageNumber, int pageSize);
    }

    /// <summary>
    /// Migrado de la V2 (Ventas.BLL.CategoriasComisionMgr estático con _MIAPISessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class CategoriasComisionServicio(
        ICategoriaComisionBLL categoriaComisionBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), ICategoriasComisionServicio
    {
        public cResponseCategoriasComision GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseCategoriasComision(),
                       () => categoriaComisionBLL.GetList(),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.CategoriasComision("GetList"),
                       nombreLog: "Categorias Comision GetList",
                       mensajeNoEncontrado: "No se encontraron categorias de comision",
                       asignarItems: (r, items) => r.categoriasComision = items);
    }
}

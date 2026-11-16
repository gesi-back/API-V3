using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IGruposDeProductosServicio
    {
        cResponseGrupoDeArticulos GetList(int pageNumber, int pageSize);
        cResponseGrupoDeArticulo GetItem(int grupoArticulosID);
    }

    /// <summary>
    /// Migrado de la V2 (ProductosMgr.GrupoArticulosGetList / GrupoArticulosGetItem, estáticos).
    /// Usa las respuestas cResponseGrupoDeArticulos / cResponseGrupoDeArticulo que ya existían en cResponseProductos.cs.
    /// </summary>
    internal sealed class GruposDeProductosServicio(
        IGrupoProductosBLL grupoProductosBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IGruposDeProductosServicio
    {
        public cResponseGrupoDeArticulos GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseGrupoDeArticulos(),
                       () => grupoProductosBLL.GetList(),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.GrupoDeProductos("GetList"),
                       nombreLog: "Grupo de Productos GetList",
                       mensajeNoEncontrado: "No se encontraron grupos de productos",
                       asignarItems: (r, items) => r.grupoArticulos = items);

        public cResponseGrupoDeArticulo GetItem(int grupoArticulosID)
        {
            var stopwatch = Stopwatch.StartNew();

            if (grupoArticulosID == 0)
                throw new cCodigoNoDetalladoException("No se especifico un GrupoArticulosID");   // → 400

            var grupo = grupoProductosBLL.GetItem(grupoArticulosID)
                        ?? throw new cDatosNoEncontradosException("No se encontro el grupo de productos " + grupoArticulosID);   // → 404

            LoguearInfo("Success GrupoArticulos GetItem. GrupoArtID: " + grupoArticulosID,
                        Endpoint.Maestros.GrupoDeProductos("GetItem"), stopwatch, 1, 1, 1);

            return new cResponseGrupoDeArticulo
            {
                grupoArticulos = grupo,   // en la V2 la propiedad del ítem también se llama "grupoArticulos"
                success = true,
                paginacion = new cPaginacion(1, 1, 1, 1, 1),
            };
        }
    }
}

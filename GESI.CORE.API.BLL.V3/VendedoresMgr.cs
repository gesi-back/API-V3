using GESI.CORE.API.BO.V3;
using GESI.ERP.Core.V3.BLL;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3
{
    public interface IVendedoresServicio
    {
        cResponseVendedores GetList(int pageNumber, int pageSize);
        cResponseVendedor GetItem(int vendedorID);
        cResponseDescuentosVendedoresXAplicacion GetListDescuentos(int canalDeVentaID, int aplicacionID, int vendedorID, int pageNumber, int pageSize);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.VendedoresMgr estático con _MiApiSessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class VendedoresServicio(
        IVendedoresBLL vendedoresBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IVendedoresServicio
    {
        public cResponseVendedores GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseVendedores(),
                       () => vendedoresBLL.GetList(),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.Vendedores("GetList"),
                       nombreLog: "Vendedores GetList",
                       mensajeNoEncontrado: "No se encontraron vendedores",
                       asignarItems: (r, items) => r.vendedores = items);

        public cResponseVendedor GetItem(int vendedorID)
        {
            var stopwatch = Stopwatch.StartNew();

            if (vendedorID == 0)
                throw new cCodigoNoDetalladoException("Se debe especificar el parametro vendedorID");   // → 400

            var vendedor = vendedoresBLL.GetList(vendedorID)?.FirstOrDefault()
                           ?? throw new cDatosNoEncontradosException("No se encontró el vendedor " + vendedorID);   // → 404

            LoguearInfo("Success Vendedores GetItem", Endpoint.Maestros.Vendedores("GetItem"), stopwatch, 1, 1, 1);

            return new cResponseVendedor
            {
                vendedor = vendedor,
                success = true,
                paginacion = new cPaginacion(1, 1, 1, 1, 1),
            };
        }

        public cResponseDescuentosVendedoresXAplicacion GetListDescuentos(int canalDeVentaID, int aplicacionID, int vendedorID,
                                                                          int pageNumber, int pageSize)
            => Paginar(new cResponseDescuentosVendedoresXAplicacion(),
                       // Igual que la V2: se pasan los filtros como short (0 = sin filtro)
                       () => vendedoresBLL.GetListDescuentosXAplicaciones((short)canalDeVentaID, (short)aplicacionID, (short)vendedorID),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.Vendedores("GetDescuentosXAplicacion"),
                       nombreLog: "Vendedores GetDescuentosXAplicacion",
                       mensajeNoEncontrado: "No se encontraron descuentos de vendedores",
                       asignarItems: (r, items) => r.descuentosVendedores = items);
    }
}

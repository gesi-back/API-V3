using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IAlmacenesServicio
    {
        cResponseAlmacenes GetList(int pageNumber, int pageSize, bool? controlDeTransferencias);
        cResponseAlmacen GetItem(int almacenID);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.AlmacenesMgr estático con _MIAPISessionMgr estático).
    /// Devuelve cAlmacenAPI (el almacen del Core sin EmpresaID), igual que la V2.
    /// </summary>
    internal sealed class AlmacenesServicio(
        IAlmacenesBLL almacenesBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IAlmacenesServicio
    {
        public cResponseAlmacenes GetList(int pageNumber, int pageSize, bool? controlDeTransferencias)
        {
            // Igual que la V2: la sucursal 1 (casa central) ve los almacenes de todas las sucursales
            int sucursalID = Sesion.SucursalID > 1 ? Sesion.SucursalID : 0;

            return Paginar(new cResponseAlmacenes(),
                           () => almacenesBLL.GetList(sucursalID, controlDeTransferencias),
                           pageNumber, pageSize,
                           endpoint: Endpoint.Maestros.Almacenes("GetList"),
                           nombreLog: "Almacenes GetList",
                           mensajeNoEncontrado: "No se encontraron almacenes",
                           asignarItems: (r, items) => r.almacenes = items.Select(a => new cAlmacenAPI(a)).ToList());
        }

        public cResponseAlmacen GetItem(int almacenID)
        {
            var stopwatch = Stopwatch.StartNew();

            if (almacenID == 0)
                throw new cCodigoNoDetalladoException("No se especifico un almacen");   // → 400

            var almacen = almacenesBLL.GetItem((uint)Sesion.EmpresaID, (uint)almacenID)
                          ?? throw new cDatosNoEncontradosException("No se encontro el almacen");   // → 404

            LoguearInfo("Success Almacenes GetItem. AlmacenID: " + almacenID, Endpoint.Maestros.Almacenes("GetItem"), stopwatch, 1, 1, 1);

            return new cResponseAlmacen
            {
                almacen = new cAlmacenAPI(almacen),
                success = true,
                paginacion = new cPaginacion(1, 1, 1, 1, 1),
            };
        }
    }
}

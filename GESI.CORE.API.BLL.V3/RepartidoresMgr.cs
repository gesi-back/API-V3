using GESI.CORE.API.BO.V3;
using GESI.ERP.Core.V3.BLL;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3
{
    public interface IRepartidoresServicio
    {
        cResponseRepartidores GetList(int pageNumber, int pageSize);
        cResponseRepartidor GetItem(int repartidorID);
    }

    /// <summary>
    /// Migrado de la V2 (Repartos.BLL.RepartidoresMgr estático con _MiAPISessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class RepartidoresServicio(
        IRepartidoresBLL repartidoresBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IRepartidoresServicio
    {
        public cResponseRepartidores GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseRepartidores(),
                       () => repartidoresBLL.GetList(),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.Repartidores("GetList"),
                       nombreLog: "Repartidores GetList",
                       mensajeNoEncontrado: "No se encontraron repartidores",
                       asignarItems: (r, items) => r.repartidores = items);

        public cResponseRepartidor GetItem(int repartidorID)
        {
            var stopwatch = Stopwatch.StartNew();

            if (repartidorID == 0)
                throw new cCodigoNoDetalladoException("Se debe especificar el parametro repartidorID");   // → 400

            var repartidor = repartidoresBLL.GetList(repartidorID)?.FirstOrDefault()
                             ?? throw new cDatosNoEncontradosException("No se encontró el repartidor " + repartidorID);   // → 404

            LoguearInfo("Success Repartidores GetItem", Endpoint.Maestros.Repartidores("GetItem"), stopwatch, 1, 1, 1);

            return new cResponseRepartidor
            {
                repartidor = repartidor,
                success = true,
                paginacion = new cPaginacion(1, 1, 1, 1, 1),
            };
        }
    }
}

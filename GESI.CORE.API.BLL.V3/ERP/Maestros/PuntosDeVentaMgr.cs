using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IPuntosDeVentaServicio
    {
        cResponsePuntosDeVenta GetList(int pageNumber, int pageSize);
        cResponsePuntoDeVenta GetItem(int puntoDeVentaID);
    }

    /// <summary>
    /// Migrado de la V2 (BLL.PuntosDeVentaMgr estático con _MIAPISessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class PuntosDeVentaServicio(
        IPuntosDeVentaBLL puntosDeVentaBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IPuntosDeVentaServicio
    {
        public cResponsePuntosDeVenta GetList(int pageNumber, int pageSize)
            => Paginar(new cResponsePuntosDeVenta(),
                       () => puntosDeVentaBLL.GetList(),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.PuntosDeVenta("GetList"),
                       nombreLog: "Puntos de venta GetList",
                       mensajeNoEncontrado: "No se encontraron puntos de venta",
                       asignarItems: (r, items) => r.puntosDeVenta = items);

        public cResponsePuntoDeVenta GetItem(int puntoDeVentaID)
        {
            var stopwatch = Stopwatch.StartNew();

            if (puntoDeVentaID == 0)
                throw new cCodigoNoDetalladoException("No se especifico el puntoDeVentaID");   // → 400

            var puntoDeVenta = puntosDeVentaBLL.GetItem(puntoDeVentaID)
                               ?? throw new cDatosNoEncontradosException("No se encontró el punto de venta " + puntoDeVentaID);   // → 404

            LoguearInfo("Success PuntosDeVenta GetItem", Endpoint.Maestros.PuntosDeVenta("GetItem"), stopwatch, 1, 1, 1);

            return new cResponsePuntoDeVenta
            {
                puntoDeVenta = puntoDeVenta,
                success = true,
                paginacion = new cPaginacion(1, 1, 1, 1, 1),
            };
        }
    }
}

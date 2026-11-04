using GESI.CORE.API.BO.V3;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Net;

namespace GESI.CORE.API.BLL.V3
{

    public interface ICanalesDeVentaServicio
    {
        cResponseCanalesDeVenta GetList(int pageNumber, int pageSize);
    }

    internal sealed class CanalesDeVentaServicio(
        ICanalesDeVentaBLL canalesDeVentaBLL,
        IAPIHelper APIHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ICanalesDeVentaServicio
    {
        public cResponseCanalesDeVenta GetList(int pageNumber, int pageSize)
        {
            var stopwatch = Stopwatch.StartNew();
            var respuesta = new cResponseCanalesDeVenta();

            pageSize = APIHelper.DevolverTamanoPaginacion(sesion.TamanoPagina, pageSize);
            pageNumber = Math.Max(pageNumber, 1);

            // Empresa y usuario salen de la sesión del request
            var habilitados = canalesDeVentaBLL
                .GetCanalesDeVentaPorUsuario(sesion.EmpresaID, sesion.Usuario)
                .ToHashSet();

            if (habilitados.Count == 0)
            {
                Loguear("Not Found CanalesDeVenta GetList", stopwatch, 0, pageNumber, 0);
                respuesta.success = true;
                respuesta.error = new cError((int)cCodigosError.cDatosNoEncontrados,
                                             "No se encontraron canales de venta habilitados x usuario",
                                             ApiErrors.errDatosNoEncontrados);   // ajustar al valor real
                return respuesta;
            }

            // Primero filtrar, después paginar
            var canales = canalesDeVentaBLL.GetList()
                                           .Where(c => habilitados.Contains(c.CanalDeVentaID))
                                           .ToList();

            int total = canales.Count;
            int totalPaginas = (int)Math.Ceiling((double)total / pageSize);
            respuesta.paginacion = new cPaginacion(total, totalPaginas, pageNumber, pageSize, pageNumber);
            respuesta.success = true;

            if (pageNumber > totalPaginas)
            {
                respuesta.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);
                return respuesta;
            }

            respuesta.canalesDeVenta = canales.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            Loguear("Success CanalesDeVenta GetList", stopwatch, respuesta.canalesDeVenta.Count, pageNumber, pageSize);
            return respuesta;
        }

        private void Loguear(string mensaje, Stopwatch sw, int cantidad, int pagina, int tamano)
        {
            sw.Stop();
            var ts = sw.Elapsed;
            apiLogger.LoguearSucesosAPI(mensaje, tTiposDeError.tInformativo, sesion.Usuario,
                Endpoint.Maestros.CanalesDeVenta("GetList"),
                sesion.EmpresaID, sesion.SucursalID,
                TiempoEjecucion: $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}",
                cantidadDeElementos: cantidad, numeroPagina: pagina, tamanoPagina: tamano,
                IPRemoteDesktop: sesion.RemoteIPAddress, IPPublica: sesion.Forwarded, Agent: sesion.UserAgent);
        }
    }
}

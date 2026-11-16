using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Net;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{

    public interface ICanalesDeVentaServicio
    {
        cResponseCanalesDeVenta GetList(int pageNumber, int pageSize);

        cResponseCanalDeVenta GetItem(int canalDeVentaID);
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

        /// <summary>
        /// Devuelve un canal de venta (migrado de la V2: CanalesDeVentaMgr.GetItem)
        /// </summary>
        public cResponseCanalDeVenta GetItem(int canalDeVentaID)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            // NUEVO respecto de la V2: sin canalDeVentaID devuelve 400 (la V2 buscaba el canal "0" y terminaba en 404)
            if (canalDeVentaID <= 0)
                throw new cCodigoNoDetalladoException("No se especificaron canales de venta a buscar");   // → 400

            cResponseCanalDeVenta oResponse = new cResponseCanalDeVenta();

            cCanalDeVenta? oCanal = canalesDeVentaBLL.GetItem(canalDeVentaID.ToString());

            // Igual que la V2: canal inexistente → 404
            if (oCanal == null)
                throw new cDatosNoEncontradosException("No se encontró el canal de venta");

            oResponse.success = true;
            oResponse.canalDeVenta = oCanal;
            oResponse.paginacion = new cPaginacion(1, 1, 1, 1, 1);

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            apiLogger.LoguearSucesosAPI("Success CanalesDeVenta GetItem", tTiposDeError.tInformativo, sesion.Usuario,
                Endpoint.Maestros.CanalesDeVenta("GetItem"),
                sesion.EmpresaID, sesion.SucursalID,
                TiempoEjecucion: $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}",
                cantidadDeElementos: 1, numeroPagina: 1, tamanoPagina: 1,
                IPRemoteDesktop: sesion.RemoteIPAddress, IPPublica: sesion.Forwarded, Agent: sesion.UserAgent);

            return oResponse;
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

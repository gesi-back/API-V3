using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3
{
    /// <summary>
    /// Base de los servicios migrados de la V2. Concentra lo que antes se repetía en cada Mgr:
    /// la paginación en memoria, las respuestas de "no encontrado" / "paginación excedida" y el log informativo.
    /// </summary>
    internal abstract class ServicioBase(IAPIHelper apiHelper, IApiSessionManager sesion, IApiLogger apiLogger)
    {
        /// <summary>Sesión del request (empresa, usuario, IP, etc.).</summary>
        protected IApiSessionManager Sesion => sesion;

        /// <summary>Logger de la API, para logs que no son el informativo estándar.</summary>
        protected IApiLogger Logger => apiLogger;

        /// <summary>Helper de la API (paginación por códigos, tamaño de página, etc.).</summary>
        protected IAPIHelper ApiHelper => apiHelper;

        /// <summary>
        /// Obtiene la lista completa, la pagina en memoria y arma la respuesta con el mismo contrato de la V2:
        /// <list type="bullet">
        /// <item>Lista vacía → 200, success=true, error "datos no encontrados".</item>
        /// <item>Página fuera de rango → 200, success=true, paginación cargada y error "paginación excedida".</item>
        /// <item>OK → 200, success=true, paginación + ítems de la página.</item>
        /// </list>
        /// Las excepciones (SQL, etc.) no se atrapan: las traduce el controller con Responder.
        /// </summary>
        /// <param name="respuesta">Respuesta vacía a completar (de acá se infiere el tipo).</param>
        /// <param name="obtener">Llamada al BLL del Core que devuelve la lista completa.</param>
        /// <param name="pageNumber">Página pedida (si es menor a 1 se usa 1).</param>
        /// <param name="pageSize">Tamaño pedido (se limita al máximo de la habilitación, mínimo 1).</param>
        /// <param name="endpoint">Endpoint para el log, ej. Endpoint.Maestros.Zonas("GetList").</param>
        /// <param name="nombreLog">Texto del log, ej. "Zonas GetList" → "Success Zonas GetList".</param>
        /// <param name="mensajeNoEncontrado">Mensaje que recibe el cliente si no hay datos.</param>
        /// <param name="asignarItems">Dónde va la página dentro de la respuesta, ej. (r, items) => r.zonas = items.</param>
        protected TResp Paginar<TResp, TItem>(
            TResp respuesta,
            Func<List<TItem>?> obtener,
            int pageNumber,
            int pageSize,
            string endpoint,
            string nombreLog,
            string mensajeNoEncontrado,
            Action<TResp, List<TItem>> asignarItems)
            where TResp : cResponse, IRespuestaPaginada
        {
            var stopwatch = Stopwatch.StartNew();

            pageSize = Math.Max(apiHelper.DevolverTamanoPaginacion(sesion.TamanoPagina, pageSize), 1);
            pageNumber = Math.Max(pageNumber, 1);

            var lista = obtener() ?? [];
            respuesta.success = true;

            if (lista.Count == 0)
            {
                LoguearInfo($"Not Found {nombreLog}", endpoint, stopwatch, 0, pageNumber, 0);
                respuesta.error = new cError((int)cCodigosError.cDatosNoEncontrados,
                                             mensajeNoEncontrado,
                                             ApiErrors.errDatosNoEncontrados);
                return respuesta;
            }

            int total = lista.Count;
            int totalPaginas = (int)Math.Ceiling((double)total / pageSize);
            respuesta.paginacion = new cPaginacion(total, totalPaginas, pageNumber, pageSize, pageNumber);

            if (pageNumber > totalPaginas)
            {
                respuesta.error = new cError((int)cCodigosError.cPaginacionExcedida,
                                             "Paginación Excedida",
                                             ApiErrors.errPaginacionExcedida);
                return respuesta;
            }

            asignarItems(respuesta, lista.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());

            LoguearInfo($"Success {nombreLog}", endpoint, stopwatch, total, pageNumber, pageSize);
            return respuesta;
        }

        /// <summary>
        /// Convierte "1, 2,3" en [1, 2, 3] para los Delete.
        /// Vacío → cCodigoNoDetalladoException (400); un código no numérico → FormatException (400).
        /// </summary>
        protected static List<int> ParsearCodigos(string? codigos)
        {
            var ids = (codigos ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(int.Parse)
                        .ToList();

            if (ids.Count == 0)
                throw new cCodigoNoDetalladoException("No se informo el codigo a eliminar");

            return ids;
        }

        /// <summary>
        /// Log informativo estándar con tiempo de ejecución y datos de la sesión.
        /// </summary>
        protected void LoguearInfo(string mensaje, string endpoint, Stopwatch stopwatch, int cantidad, int pagina, int tamano)
        {
            stopwatch.Stop();
            var ts = stopwatch.Elapsed;
            apiLogger.LoguearSucesosAPI(mensaje, tTiposDeError.tInformativo, sesion.Usuario,
                endpoint,
                sesion.EmpresaID, sesion.SucursalID,
                TiempoEjecucion: $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}",
                cantidadDeElementos: cantidad, numeroPagina: pagina, tamanoPagina: tamano,
                IPRemoteDesktop: sesion.RemoteIPAddress, IPPublica: sesion.Forwarded, Agent: sesion.UserAgent);
        }
    }
}

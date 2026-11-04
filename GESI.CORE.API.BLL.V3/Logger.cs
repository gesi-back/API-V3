using GESI.CORE.API.BO.V3;
using GESI.ERP.Core.V3.BO;
using System;
using System.Collections.Generic;
using System.Text;

namespace GESI.CORE.API.BLL.V3
{
     
    public interface IApiLogger
    {
        void LoguearSucesosAPI(string? strDescripcionError,
                               string? strTipo,
                               string? usuario,
                               string? Endpoint,
                               int empresaID,
                               int sucursalID,
                               int codigoErrorInterno = 200,
                               string TiempoEjecucion = "",
                               int cantidadDeElementos = 0,
                               int numeroPagina = 0,
                               string parametros = "",
                               int tamanoPagina = 0,
                               string IPRemoteDesktop = "",
                               string IPPublica = "",
                               string Agent = "");
    }

    internal sealed class ApiLogger : IApiLogger
    {
        private const int MaxIntentos = 3;
        private static readonly object Lock = new();

        // Carpeta de la aplicación (más confiable que GetCurrentDirectory cuando corre en IIS o como servicio)
        private static readonly string CarpetaLogs = Path.Combine(AppContext.BaseDirectory, "logs");

        public void LoguearSucesosAPI(string? strDescripcionError,
                                      string? strTipo,
                                      string? usuario,
                                      string? Endpoint,
                                      int empresaID,
                                      int sucursalID,
                                      int codigoErrorInterno = 200,
                                      string TiempoEjecucion = "",
                                      int cantidadDeElementos = 0,
                                      int numeroPagina = 0,
                                      string parametros = "",
                                      int tamanoPagina = 0,
                                      string IPRemoteDesktop = "",
                                      string IPPublica = "",
                                      string Agent = "")
        {
            var mes = DateTime.Now.ToString("yyyy-MM");

            (string carpeta, string archivo)? destino = strTipo switch
            {
                tTiposDeError.tInformativo => ("info", $"logAPI-Info{mes}.txt"),
                tTiposDeError.tError => ("errors", $"logAPI-Error{mes}.txt"),
                "T" => (Path.Combine("errors", "timeout"), $"logAPI-Timeout{mes}.txt"),
                _ => null,
            };
            if (destino is null) return;   // tipo desconocido: igual que antes, no se loguea

            var partes = new List<string?>
        {
            DateTime.Now.ToString(), strTipo, usuario, Endpoint, codigoErrorInterno.ToString(), strDescripcionError,
            $"EmpresaID:{empresaID}", $"SucursalID:{sucursalID}",
        };
            if (strTipo == tTiposDeError.tInformativo)   // el log informativo tenía dos campos extra
            {
                partes.Add($"Cantidad de Elementos: {cantidadDeElementos}");
                partes.Add($"Pagina: {numeroPagina}");
            }
            partes.AddRange(
            [
                $"Tiempo:{TiempoEjecucion}", $"Parametros:{parametros}", $"TamanoPagina:{tamanoPagina}",
            $"IPRemote:{IPRemoteDesktop}", $"IPPublica:{IPPublica}", $"Agent:{Agent}",
        ]);
            var linea = string.Join("|", partes);

            var carpeta = Path.Combine(CarpetaLogs, destino.Value.carpeta);
            var ruta = Path.Combine(carpeta, destino.Value.archivo);

            for (int intento = 1; intento <= MaxIntentos; intento++)
            {
                try
                {
                    lock (Lock)   // un solo hilo escribe a la vez
                    {
                        Directory.CreateDirectory(carpeta);       // no hace nada si ya existe
                        File.AppendAllText(ruta, linea + Environment.NewLine);
                    }
                    return;
                }
                catch (IOException) when (intento < MaxIntentos)
                {
                    Thread.Sleep(50);   // otro proceso (otra instancia de la API) tiene el archivo
                }
                catch
                {
                    return;   // el log nunca tiene que romper el request
                }
            }
        }
    }
}

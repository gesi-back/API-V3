using GESI.CORE.API.BO.V3;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3
{
    public interface ITiposDeOperacionServicio
    {
        cResponseTiposDeOperacion GetList(int pageNumber, int pageSize, string? ordenarPor, string? direccion);
        cResponseTipoDeOperacion GetItem(int tipoDeOperacionID);
        List<cResponseTipoDeOperacionSave> Save(List<cTipoDeOperacion>? tiposDeOperacion, bool alta);
        cResponse Delete(string? codigos);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.TiposDeOperacionMgr estático con _MiApiSessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class TiposDeOperacionServicio(
        ITiposDeOperacionesBLL tiposDeOperacionBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), ITiposDeOperacionServicio
    {
        public cResponseTiposDeOperacion GetList(int pageNumber, int pageSize, string? ordenarPor, string? direccion)
            => Paginar(new cResponseTiposDeOperacion(),
                       () => tiposDeOperacionBLL.GetList(ordenarPor, direccion),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.TiposDeOperacion("GetList"),
                       nombreLog: "TiposDeOperacion GetList",
                       mensajeNoEncontrado: "No se encontraron tipos de operacion",
                       asignarItems: (r, items) => r.tiposDeOperacion = items);

        public cResponseTipoDeOperacion GetItem(int tipoDeOperacionID)
        {
            var stopwatch = Stopwatch.StartNew();

            if (tipoDeOperacionID == 0)
                throw new cCodigoNoDetalladoException("Se debe especificar el parametro tipoDeOperacionID");   // → 400

            var tipoDeOperacion = tiposDeOperacionBLL.GetItem(tipoDeOperacionID)
                                  ?? throw new cDatosNoEncontradosException("No se encontró el tipo de operacion " + tipoDeOperacionID);   // → 404

            LoguearInfo("Success TiposDeOperacion GetItem", Endpoint.Maestros.TiposDeOperacion("GetItem"), stopwatch, 1, 1, 1);

            return new cResponseTipoDeOperacion
            {
                tipoOperacion = tipoDeOperacion,
                success = true,
                paginacion = new cPaginacion(1, 1, 1, 1, 1),
            };
        }

        public List<cResponseTipoDeOperacionSave> Save(List<cTipoDeOperacion>? tiposDeOperacion, bool alta)
        {
            var stopwatch = Stopwatch.StartNew();
            string accion = alta ? "Create" : "Update";

            if (tiposDeOperacion is null || tiposDeOperacion.Count == 0)
                throw new cCodigoNoDetalladoException("No se informaron tipos de operacion a grabar");   // → 400

            var respuesta = new List<cResponseTipoDeOperacionSave>();

            foreach (var tipoDeOperacion in tiposDeOperacion)
            {
                // La empresa sale siempre de la sesión: el cliente no puede grabar en otra empresa
                tipoDeOperacion.EmpresaID = Sesion.EmpresaID;

                if (alta)
                    tipoDeOperacion.TipoDeOperacionID = 0;   // en el alta el ID lo asigna el ERP
                else if (tipoDeOperacion.TipoDeOperacionID == 0)
                {
                    respuesta.Add(new cResponseTipoDeOperacionSave(tipoDeOperacion, false, "No se detalló el tipoDeOperacionID"));
                    continue;
                }

                try
                {
                    var grabado = tiposDeOperacionBLL.Save(tipoDeOperacion);
                    respuesta.Add(new cResponseTipoDeOperacionSave(grabado, true, null));
                }
                catch (Exception ex)
                {
                    // Un ítem con error no corta el lote, igual que en la V2
                    Logger.LoguearSucesosAPI($"Error TiposDeOperacion {accion} ID {tipoDeOperacion.TipoDeOperacionID}: {ex.Message}",
                        tTiposDeError.tError, Sesion.Usuario, Endpoint.Maestros.TiposDeOperacion(accion),
                        Sesion.EmpresaID, Sesion.SucursalID,
                        IPRemoteDesktop: Sesion.RemoteIPAddress, IPPublica: Sesion.Forwarded, Agent: Sesion.UserAgent);

                    respuesta.Add(new cResponseTipoDeOperacionSave(tipoDeOperacion, false, ex.Message));
                }
            }

            LoguearInfo($"TiposDeOperacion {accion}: {respuesta.Count(r => r.success)} de {respuesta.Count} grabados",
                        Endpoint.Maestros.TiposDeOperacion(accion), stopwatch, respuesta.Count, 1, respuesta.Count);
            return respuesta;
        }

        public cResponse Delete(string? codigos)
        {
            var stopwatch = Stopwatch.StartNew();

            var ids = ParsearCodigos(codigos);   // vacío o no numérico → 400

            int eliminados = tiposDeOperacionBLL.Delete(ids);

            if (eliminados == 0)
                throw new cDatosNoEncontradosException("El registro a eliminar no existe o ya fue eliminado");   // → 404

            LoguearInfo($"Success TiposDeOperacion Delete: {codigos}", Endpoint.Maestros.TiposDeOperacion("Delete"),
                        stopwatch, eliminados, 1, eliminados);
            return new cResponse { success = true };
        }
    }
}

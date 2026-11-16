using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Ventas
{
    public interface IFormasDePagoServicio
    {
        cResponseFormasDePago GetList(int pageNumber, int pageSize, string? ordenarPor, string? direccion);
        cResponseFormaDePago GetItem(int formaDePagoID);
        List<cResponseFormaDePagoSave> Save(List<cFormaDePago>? formasDePago, bool alta);
        cResponse Delete(string? codigos);
    }

    /// <summary>
    /// Migrado de la V2 (FormasDePagoMgr estático con _MiAPISessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class FormasDePagoServicio(
        IFormasDePagoBLL formasDePagoBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IFormasDePagoServicio
    {
        public cResponseFormasDePago GetList(int pageNumber, int pageSize, string? ordenarPor, string? direccion)
            => Paginar(new cResponseFormasDePago(),
                       () => formasDePagoBLL.GetList(strOrdenarPor: ordenarPor, strDireccion: direccion),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.FormasDePago("GetList"),
                       nombreLog: "FormasDePago GetList",
                       mensajeNoEncontrado: "No se encontraron formas de pago",
                       asignarItems: (r, items) => r.formasDePago = items);

        public cResponseFormaDePago GetItem(int formaDePagoID)
        {
            var stopwatch = Stopwatch.StartNew();

            if (formaDePagoID == 0)
                throw new cCodigoNoDetalladoException("Se debe especificar el parametro formaDePagoID");   // → 400

            var formaDePago = formasDePagoBLL.GetItem(formaDePagoID)
                              ?? throw new cDatosNoEncontradosException("No se encontró la forma de pago " + formaDePagoID);   // → 404

            LoguearInfo("Success FormasDePago GetItem", Endpoint.Maestros.FormasDePago("GetItem"), stopwatch, 1, 1, 1);

            return new cResponseFormaDePago
            {
                formaDePago = formaDePago,
                success = true,
                paginacion = new cPaginacion(1, 1, 1, 1, 1),
            };
        }

        public List<cResponseFormaDePagoSave> Save(List<cFormaDePago>? formasDePago, bool alta)
        {
            var stopwatch = Stopwatch.StartNew();
            string accion = alta ? "Create" : "Update";

            if (formasDePago is null || formasDePago.Count == 0)
                throw new cCodigoNoDetalladoException("No se informaron formas de pago a grabar");   // → 400

            var respuesta = new List<cResponseFormaDePagoSave>();

            foreach (var formaDePago in formasDePago)
            {
                // La empresa sale siempre de la sesión: el cliente no puede grabar en otra empresa
                formaDePago.EmpresaID = (short)Sesion.EmpresaID;

                if (alta)
                    formaDePago.FormaDePagoID = 0;   // en el alta el ID lo asigna el ERP
                else if (formaDePago.FormaDePagoID == 0)
                {
                    respuesta.Add(new cResponseFormaDePagoSave(formaDePago, false, "No se detalló la formaDePagoID"));
                    continue;
                }

                try
                {
                    var grabada = formasDePagoBLL.Save(formaDePago);
                    respuesta.Add(new cResponseFormaDePagoSave(grabada, true, null));
                }
                catch (Exception ex)
                {
                    // Un ítem con error no corta el lote, igual que en la V2
                    Logger.LoguearSucesosAPI($"Error FormasDePago {accion} ID {formaDePago.FormaDePagoID}: {ex.Message}",
                        tTiposDeError.tError, Sesion.Usuario, Endpoint.Maestros.FormasDePago(accion),
                        Sesion.EmpresaID, Sesion.SucursalID,
                        IPRemoteDesktop: Sesion.RemoteIPAddress, IPPublica: Sesion.Forwarded, Agent: Sesion.UserAgent);

                    respuesta.Add(new cResponseFormaDePagoSave(formaDePago, false, ex.Message));
                }
            }

            LoguearInfo($"FormasDePago {accion}: {respuesta.Count(r => r.success)} de {respuesta.Count} grabadas",
                        Endpoint.Maestros.FormasDePago(accion), stopwatch, respuesta.Count, 1, respuesta.Count);
            return respuesta;
        }

        public cResponse Delete(string? codigos)
        {
            var stopwatch = Stopwatch.StartNew();

            var ids = ParsearCodigos(codigos);   // vacío o no numérico → 400

            int eliminados = formasDePagoBLL.Delete(ids);

            if (eliminados == 0)
                throw new cDatosNoEncontradosException("El registro a eliminar no existe o ya fue eliminado");   // → 404

            LoguearInfo($"Success FormasDePago Delete: {codigos}", Endpoint.Maestros.FormasDePago("Delete"),
                        stopwatch, eliminados, 1, eliminados);
            return new cResponse { success = true };
        }
    }
}

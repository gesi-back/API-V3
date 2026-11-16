using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Contabilidad;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using GESI.Contabilidad.V3.BO;
using System.Diagnostics;
using GESI.Contabilidad.V3.BLL;

namespace GESI.CORE.API.BLL.V3.ERP.Contabilidad
{
    public interface IEjerciciosServicio
    {
        cResponseEjercicios GetList(int pageNumber, int pageSize);
        List<cResponseEjerciciosSave> Save(List<cEjercicio>? lstEjercicios);
        cResponse Delete(int ejercicioID);
    }

    /// <summary>
    /// Migrado de la V2 (BLL.EjerciciosMgr estático con MiApiSessionManager estático).
    /// Mantiene los mismos pasos que la V2; la sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class EjerciciosServicio(
        IEjerciciosBLL ejerciciosBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IEjerciciosServicio
    {
        /// <summary>
        /// Devuelve la lista de ejercicios contables
        /// </summary>
        public cResponseEjercicios GetList(int pageNumber, int pageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseEjercicios oRespuesta = new cResponseEjercicios();

            // NUEVO respecto de la V2: se respeta el tamaño máximo de página de la habilitación
            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            try
            {
                List<cEjercicio>? lstEjercicios = ejerciciosBLL.GetList();

                if (lstEjercicios == null || lstEjercicios.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron ejercicios");

                int cantidadElementos = lstEjercicios.Count;

                lstEjercicios = lstEjercicios.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstEjercicios.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber));

                oRespuesta.success = true;
                oRespuesta.ejercicios = lstEjercicios;
                oRespuesta.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success Ejercicios GetList", Endpoint.Contabilidad.Ejercicios("GetList"), stopwatch, cantidadElementos, pageNumber, pageSize);

                return oRespuesta;
            }
            catch (cIntervalException oIntervalo)
            {
                oRespuesta.paginacion = oIntervalo._paginacion;
                oRespuesta.success = true;
                oRespuesta.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);

                return oRespuesta;
            }
            catch (cDatosNoEncontradosException mensaje)
            {
                oRespuesta.error = new cError((int)cCodigosError.cDatosNoEncontrados, mensaje.Message, ApiErrors.errDatosNoEncontrados);
                oRespuesta.success = true;

                return oRespuesta;
            }
        }

        /// <summary>
        /// Graba ejercicios (lo usan Create y Update, igual que la V2)
        /// </summary>
        public List<cResponseEjerciciosSave> Save(List<cEjercicio>? lstEjercicios)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (lstEjercicios == null || lstEjercicios.Count == 0)
                throw new cCodigoNoDetalladoException("No se informaron ejercicios a grabar");   // → 400

            List<cResponseEjerciciosSave> lstRespuesta = new List<cResponseEjerciciosSave>();

            foreach (cEjercicio oEjercicio in lstEjercicios)
            {
                // La V2 le ponía la empresa al objeto de resultado y no al que se grababa;
                // acá se le pone al que se graba, así siempre queda en la empresa de la sesión
                oEjercicio.EmpresaID = (short)Sesion.EmpresaID;

                try
                {
                    cEjercicio? oEjercicioResultado = ejerciciosBLL.Save(oEjercicio);

                    lstRespuesta.Add(new cResponseEjerciciosSave(oEjercicioResultado, true, null));
                }
                catch (Exception ex)
                {
                    // Un ejercicio con error no corta el lote, igual que en la V2
                    Logger.LoguearSucesosAPI("Error al grabar Ejercicio " + oEjercicio.EjercicioID + ": " + ex.Message,
                                             tTiposDeError.tError,
                                             Sesion.Usuario,
                                             Endpoint.Contabilidad.Ejercicios("Save"),
                                             Sesion.EmpresaID,
                                             Sesion.SucursalID,
                                             IPRemoteDesktop: Sesion.RemoteIPAddress, IPPublica: Sesion.Forwarded, Agent: Sesion.UserAgent);

                    lstRespuesta.Add(new cResponseEjerciciosSave(oEjercicio, false, ex.Message));
                }
            }

            LoguearInfo("Success Ejercicios Save", Endpoint.Contabilidad.Ejercicios("Save"), stopwatch, lstEjercicios.Count, 1, lstEjercicios.Count);

            return lstRespuesta;
        }

        /// <summary>
        /// Elimina un ejercicio
        /// </summary>
        public cResponse Delete(int ejercicioID)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (ejercicioID <= 0)
                throw new cCodigoNoDetalladoException("No se detalló el ejercicioID");   // → 400

            int cantidadRegistros = ejerciciosBLL.Delete(ejercicioID);

            if (cantidadRegistros == 0)
                throw new cDatosNoEncontradosException("El registro a eliminar no existe o ya fue eliminado");   // → 404

            LoguearInfo("Success Ejercicios Delete ejercicioID: " + ejercicioID, Endpoint.Contabilidad.Ejercicios("Delete"), stopwatch, cantidadRegistros, 1, cantidadRegistros);

            return new cResponse { success = true };
        }
    }
}

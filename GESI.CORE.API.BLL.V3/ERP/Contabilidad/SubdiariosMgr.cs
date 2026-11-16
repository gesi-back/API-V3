using GESI.Contabilidad.V3.BLL;
using GESI.Contabilidad.V3.BO;
using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Contabilidad;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Contabilidad
{
    public interface ISubdiariosServicio
    {
        cResponseSubdiarios GetList(int pageNumber, int pageSize, int ejercicioID);
        List<cResponseSubdiarioSave> Save(List<cSubdiario>? lstSubdiario);
        cResponse Delete(int ejercicioID, string? codigos);
    }

    /// <summary>
    /// Migrado de la V2 (BLL.SubdiariosMgr estático con MiApiSessionManager estático).
    /// Mantiene los mismos pasos que la V2; la sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class SubdiariosServicio(
        ISubdiariosBLL subdiariosBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), ISubdiariosServicio
    {
        /// <summary>
        /// Devuelve los subdiarios de un ejercicio
        /// </summary>
        public cResponseSubdiarios GetList(int pageNumber, int pageSize, int ejercicioID)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseSubdiarios oRespuesta = new cResponseSubdiarios();

            if (ejercicioID == 0)
                throw new cCodigoNoDetalladoException("No se detalló el ejercicioID");   // → 400

            // NUEVO respecto de la V2: se respeta el tamaño máximo de página de la habilitación
            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            try
            {
                List<cSubdiario>? lstSubdiarios = subdiariosBLL.GetList(ejercicioID);

                if (lstSubdiarios == null || lstSubdiarios.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron subdiarios");   // la V2 decía "ejercicios"

                int cantidadElementos = lstSubdiarios.Count;

                lstSubdiarios = lstSubdiarios.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstSubdiarios.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber));

                oRespuesta.success = true;
                oRespuesta.subdiarios = lstSubdiarios;
                oRespuesta.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success Subdiarios GetList", Endpoint.Contabilidad.Subdiarios("GetList"), stopwatch, cantidadElementos, pageNumber, pageSize);

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
        /// Graba subdiarios (lo usan Create y Update, igual que la V2)
        /// </summary>
        public List<cResponseSubdiarioSave> Save(List<cSubdiario>? lstSubdiario)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (lstSubdiario == null || lstSubdiario.Count == 0)
                throw new cCodigoNoDetalladoException("No se informaron subdiarios a grabar");   // → 400

            List<cResponseSubdiarioSave> lstRespuesta = new List<cResponseSubdiarioSave>();

            foreach (cSubdiario oSubdiario in lstSubdiario)
            {
                // La V2 le ponía la empresa al objeto de resultado y no al que se grababa;
                // acá se le pone al que se graba, así siempre queda en la empresa de la sesión
                oSubdiario.EmpresaID = (short)Sesion.EmpresaID;

                try
                {
                    if (oSubdiario.EjercicioID <= 0)
                        throw new Exception("No se detallo el ejercicioID");

                    cSubdiario? oSubdiarioResultado = subdiariosBLL.Save(oSubdiario);

                    lstRespuesta.Add(new cResponseSubdiarioSave(oSubdiarioResultado, true, null));
                }
                catch (Exception ex)
                {
                    // Un subdiario con error no corta el lote, igual que en la V2
                    Logger.LoguearSucesosAPI("Error al agregar Subdiarios: " + ex.Message,
                                             tTiposDeError.tError,
                                             Sesion.Usuario,
                                             Endpoint.Contabilidad.Subdiarios("Save"),
                                             Sesion.EmpresaID,
                                             Sesion.SucursalID,
                                             IPRemoteDesktop: Sesion.RemoteIPAddress, IPPublica: Sesion.Forwarded, Agent: Sesion.UserAgent);

                    lstRespuesta.Add(new cResponseSubdiarioSave(oSubdiario, false, ex.Message));
                }
            }

            LoguearInfo("Success Subdiarios Save", Endpoint.Contabilidad.Subdiarios("Save"), stopwatch, lstSubdiario.Count, 1, lstSubdiario.Count);

            return lstRespuesta;
        }

        /// <summary>
        /// Elimina subdiarios de un ejercicio (codigos separados por coma)
        /// </summary>
        public cResponse Delete(int ejercicioID, string? codigos)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (ejercicioID <= 0)
                throw new cCodigoNoDetalladoException("No se detalló el ejercicioID");   // → 400

            // "1,2,3" → [1, 2, 3]; vacío o no numérico → 400
            List<int> lstCodigosABorrar = ParsearCodigos(codigos);

            int cantidadRegistros = subdiariosBLL.Delete(ejercicioID, lstCodigosABorrar);

            if (cantidadRegistros == 0)
                throw new cDatosNoEncontradosException("El registro a eliminar no existe o ya fue eliminado");   // → 404

            LoguearInfo("Success Subdiarios Delete: " + codigos, Endpoint.Contabilidad.Subdiarios("Delete"), stopwatch, cantidadRegistros, 1, cantidadRegistros);

            return new cResponse { success = true };
        }
    }
}

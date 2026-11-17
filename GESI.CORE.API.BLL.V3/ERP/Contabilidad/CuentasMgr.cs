using GESI.Contabilidad.V3.BLL;
using GESI.Contabilidad.V3.BO;
using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Contabilidad;
using GESI.ERP.Contable.V3.BLL;
using GESI.ERP.Contable.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Contabilidad
{
    public interface ICuentasServicio
    {
        cResponseCuentas GetList(int ejercicioID, int pageNumber, int pageSize);
        cResponse Delete(int ejercicioID, string? cuentaID);
        cResponseCuenta Save(cCuenta? oCuenta, bool blAlta = false);
    }

    /// <summary>
    /// Migrado de la V2 (BLL.CuentasMgr estático con MiApiSessionManager estático).
    /// </summary>
    internal sealed class CuentasServicio(
        ICuentasBLL cuentasBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), ICuentasServicio
    {
        /// <summary>
        /// Devuelve las cuentas contables de un ejercicio
        /// </summary>
        public cResponseCuentas GetList(int ejercicioID, int pageNumber, int pageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseCuentas oRespuesta = new cResponseCuentas();

            // NUEVO respecto de la V2: sin ejercicioID devuelve 400 (la V2 devolvía 200 con el error adentro).
            // El Core recibe el ejercicio como short, por eso también se controla el máximo.
            if (ejercicioID <= 0 || ejercicioID > short.MaxValue)
                throw new cCodigoNoDetalladoException("No se detalló un ejercicioID válido");   // → 400

            // NUEVO respecto de la V2: se respeta el tamaño máximo de página de la habilitación
            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            try
            {
                List<cCuenta>? lstCuentas = cuentasBLL.GetList((short)ejercicioID);

                if (lstCuentas == null || lstCuentas.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron cuentas");   // la V2 decía "asientos"

                int cantidadElementos = lstCuentas.Count;

                lstCuentas = lstCuentas.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstCuentas.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber));

                oRespuesta.success = true;
                oRespuesta.cuentas = lstCuentas;
                oRespuesta.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success Cuentas GetList", Endpoint.Contabilidad.Cuentas("GetList"), stopwatch, cantidadElementos, pageNumber, pageSize);

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
        /// Elimina una cuenta contable
        /// </summary>
        public cResponse Delete(int ejercicioID, string? cuentaID)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (string.IsNullOrEmpty(cuentaID))
                throw new cCodigoNoDetalladoException("No se detalló la cuentaID");   // → 400

            if (ejercicioID <= 0)
                throw new cCodigoNoDetalladoException("No se detalló el ejercicioID");   // → 400

            int cantidadRegistros = cuentasBLL.Delete(ejercicioID, cuentaID);

            if (cantidadRegistros == 0)
                throw new cDatosNoEncontradosException("El registro a eliminar no existe o ya fue eliminado");   // → 404

            LoguearInfo("Success Cuentas Delete cuentaID: " + cuentaID, Endpoint.Contabilidad.Cuentas("Delete"), stopwatch, cantidadRegistros, 1, cantidadRegistros);

            return new cResponse { success = true };
        }

        /// <summary>
        /// Crea (blAlta = true) o modifica una cuenta contable.
        /// Igual que la V2: si el Core rechaza la grabación responde 200 con success = false y el motivo en message.
        /// </summary>
        public cResponseCuenta Save(cCuenta? oCuenta, bool blAlta = false)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseCuenta oRespuesta = new cResponseCuenta();

            // NUEVO respecto de la V2: sin cuenta, o modificación sin CuentaID, devuelve 400 (la V2 devolvía 200 con el mensaje)
            if (oCuenta == null)
                throw new cCodigoNoDetalladoException("No se especificó el objeto cuenta");   // → 400

            if (!blAlta && string.IsNullOrEmpty(oCuenta.CuentaID))
                throw new cCodigoNoDetalladoException("No se especifico la cuentaID para la edicion de la cuenta");   // → 400

            // Igual que la V2: la cuenta se graba en la empresa de la sesión
            oCuenta.EmpresaID = (short)Sesion.EmpresaID;

            string strEndpoint = Endpoint.Contabilidad.Cuentas(blAlta ? "Create" : "Update");

            try
            {
                oRespuesta.cuenta = cuentasBLL.Save(oCuenta);
                oRespuesta.success = true;

                LoguearInfo("Success Cuentas " + (blAlta ? "Create" : "Update") + " cuentaID: " + oCuenta.CuentaID, strEndpoint, stopwatch, 1, 1, 1);

                return oRespuesta;
            }
            catch (Exception ex)
            {
                // Las BLL del Core envuelven los errores en ApplicationException: el motivo real está en InnerException
                Exception real = ex is ApplicationException && ex.InnerException != null ? ex.InnerException : ex;

                // NUEVO respecto de la V2: un error de base de datos responde 500 (la V2 devolvía 200 con el detalle técnico en message)
                if (real is Microsoft.Data.SqlClient.SqlException)
                    throw;

                // Igual que la V2: el rechazo del Core vuelve en message con success = false
                Logger.LoguearSucesosAPI("Cuenta no grabada cuentaID: " + oCuenta.CuentaID + " | " + real.Message, tTiposDeError.tError,
                                         Sesion.Usuario, strEndpoint, Sesion.EmpresaID, Sesion.SucursalID);

                oRespuesta.message = real.Message;
                return oRespuesta;
            }
        }
    }
}

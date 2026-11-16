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
    }

    /// <summary>
    /// Migrado de la V2 (BLL.CuentasMgr estático con MiApiSessionManager estático).
    /// PENDIENTE: la V2 tenía Create/Update (CuentasManager.Save), pero el ICuentasBLL del Core V3
    /// no tiene Save, así que esos dos endpoints no se migraron.
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
    }
}

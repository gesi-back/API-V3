using GESI.Contabilidad.V3.BLL;
using GESI.Contabilidad.V3.BO;
using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Contabilidad;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Contabilidad
{
    public interface IAsientosServicio
    {
        cResponseAsientos GetList(int ejercicioID, int pageNumber, int pageSize);
        cResponseSaveAsiento Save(cAsiento? oAsiento);
    }

    /// <summary>
    /// Migrado de la V2 (BLL.AsientosMgr estático con MiApiSessionManager estático).
    /// </summary>
    internal sealed class AsientosServicio(
        IAsientosBLL asientosBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IAsientosServicio
    {
        /// <summary>
        /// Devuelve los asientos de un ejercicio
        /// </summary>
        public cResponseAsientos GetList(int ejercicioID, int pageNumber, int pageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseAsientos oRespuesta = new cResponseAsientos();

            // NUEVO respecto de la V2: sin ejercicioID devuelve 400 (la V2 devolvía 200 con el error adentro).
            // El Core recibe el ejercicio como byte: en la V2 un ejercicio mayor a 255 "daba la vuelta"
            // (300 se convertía en 44) y devolvía los asientos de otro ejercicio. Ahora se rechaza.
            if (ejercicioID <= 0 || ejercicioID > byte.MaxValue)
                throw new cCodigoNoDetalladoException("No se detalló un ejercicioID válido");   // → 400

            // NUEVO respecto de la V2: se respeta el tamaño máximo de página de la habilitación
            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);
            
            try
            {
                List<cAsiento>? lstAsientos = asientosBLL.GetList((byte)ejercicioID);

                if (lstAsientos == null || lstAsientos.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron asientos");

                int cantidadElementos = lstAsientos.Count;

                lstAsientos = lstAsientos.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstAsientos.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber));

                oRespuesta.success = true;
                oRespuesta.asientos = lstAsientos;
                oRespuesta.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success Asientos GetList", Endpoint.Contabilidad.Asientos("GetList"), stopwatch, cantidadElementos, pageNumber, pageSize);

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
        /// Graba un asiento contable con su detalle.
        /// Igual que la V2: success = lo que devuelve el Core; si hay errores van en error (200), si no message = "OK".
        /// </summary>
        public cResponseSaveAsiento Save(cAsiento? oAsiento)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseSaveAsiento oRespuesta = new cResponseSaveAsiento();

            // NUEVO respecto de la V2: sin asiento devuelve 400 (la V2 fallaba adentro del Core y devolvía 200 con el mensaje)
            if (oAsiento == null)
                throw new cCodigoNoDetalladoException("No se especificó el asiento a grabar");   // → 400

            // NUEVO respecto de la V2: el asiento y su detalle se graban en la empresa de la sesión
            // (la V2 grababa con la EmpresaID que viniera en el body)
            oAsiento.EmpresaID = (short)Sesion.EmpresaID;

            if (oAsiento.Detalle != null)
            {
                foreach (cDetalleAsiento oDetalle in oAsiento.Detalle)
                {
                    oDetalle.EmpresaID = (short)Sesion.EmpresaID;
                }
            }

            string strEndpoint = Endpoint.Contabilidad.Asientos("Create");

            try
            {
                (bool blRet, List<string>? lstErrores) = asientosBLL.Save(oAsiento);

                oRespuesta.success = blRet;

                if (lstErrores?.Count > 0)
                    oRespuesta.error = new cError((int)cCodigosError.cErrorConversionDato, string.Join(",", lstErrores), ApiErrors.errJsonInvalido);
                else
                    oRespuesta.message = "OK";

                LoguearInfo("Asientos Create Resultado:" + blRet + (lstErrores?.Count > 0 ? " Errores: " + string.Join(",", lstErrores) : ""),
                            strEndpoint, stopwatch, 1, 1, 1);

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
                Logger.LoguearSucesosAPI("Asiento no grabado | " + real.Message, tTiposDeError.tError,
                                         Sesion.Usuario, strEndpoint, Sesion.EmpresaID, Sesion.SucursalID);

                oRespuesta.message = real.Message;
                return oRespuesta;
            }
        }
    }
}

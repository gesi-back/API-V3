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
    }

    /// <summary>
    /// Migrado de la V2 (BLL.AsientosMgr estático con MiApiSessionManager estático).
    /// PENDIENTE: la V2 tenía Create (Contable.AsientosManager.Save), pero en el Core V3 ya no existe
    /// ese manager ni su cAsiento, así que ese endpoint no se migró.
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
    }
}

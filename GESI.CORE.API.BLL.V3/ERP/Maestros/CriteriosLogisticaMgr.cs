using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface ICriteriosLogisticaServicio
    {
        cResponseCriterioLogistica GetList(int pageNumber, int pageSize);
    }

    /// <summary>
    /// Migrado de la V2 (BLL.CriterioLogistica estático con _MIAPISessionMgr estático).
    /// Mantiene los mismos pasos que la V2; la sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class CriteriosLogisticaServicio(
        ICriterioLogisticaBLL criterioLogisticaBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), ICriteriosLogisticaServicio
    {
        /// <summary>
        /// Devuelve la lista de criterios de logistica
        /// </summary>
        public cResponseCriterioLogistica GetList(int pageNumber, int pageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseCriterioLogistica oRespuesta = new cResponseCriterioLogistica();

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            try
            {
                List<cCriterioLogistica>? lstCriteriosLogistica = criterioLogisticaBLL.GetList();

                if (lstCriteriosLogistica == null || lstCriteriosLogistica.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron criterios de logistica");

                int cantidadElementos = lstCriteriosLogistica.Count;

                lstCriteriosLogistica = lstCriteriosLogistica.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstCriteriosLogistica.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber));

                oRespuesta.criteriosLogistica = lstCriteriosLogistica;
                oRespuesta.success = true;
                oRespuesta.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success Criterios Logistica GetList", Endpoint.Maestros.CriterioLogistica("GetList"), stopwatch, cantidadElementos, pageNumber, pageSize);

                return oRespuesta;
            }
            catch (cIntervalException oIntervalo)
            {
                oRespuesta.success = true;
                oRespuesta.paginacion = oIntervalo._paginacion;
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

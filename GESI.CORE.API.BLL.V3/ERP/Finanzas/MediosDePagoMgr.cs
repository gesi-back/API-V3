using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Finanzas;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Finanzas
{
    public interface IMediosDePagoServicio
    {
        cResponseMediosDePagos GetList(string tipo, int estado, int pageNumber, int pageSize);
        cResponseMedioDePago GetItem(int medioDePagoID);
    }

    /// <summary>
    /// Migrado de la V2 (Finanzas.BLL.MediosDePagoMgr estático con MiAPISessionManager estático).
    /// Los medios de pago son los "valores" del ERP (IValoresBLL).
    /// </summary>
    internal sealed class MediosDePagoServicio(
        IValoresBLL valoresBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IMediosDePagoServicio
    {
        /// <summary>
        /// Devuelve la lista de medios de pago
        /// </summary>
        public cResponseMediosDePagos GetList(string tipo, int estado, int pageNumber, int pageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseMediosDePagos oResponse = new cResponseMediosDePagos();

            // NUEVO respecto de la V2: se respeta el tamaño máximo de página de la habilitación
            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            try
            {
                List<cValor>? lstValores = valoresBLL.GetList(strTipo: tipo, intEstado: estado);

                if (lstValores == null || lstValores.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron medios de pago");

                int cantidadElementos = lstValores.Count;

                lstValores = lstValores.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstValores.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, 0));

                oResponse.success = true;
                oResponse.mediosDePago = lstValores;
                oResponse.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success MediosDePago GetList", Endpoint.Maestros.MediosDePago("GetList"), stopwatch, cantidadElementos, pageNumber, pageSize);

                return oResponse;
            }
            catch (cIntervalException ointervalo)
            {
                oResponse.success = true;
                oResponse.paginacion = ointervalo._paginacion;
                oResponse.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);

                return oResponse;
            }
            catch (cDatosNoEncontradosException mensaje)
            {
                oResponse.error = new cError((int)cCodigosError.cDatosNoEncontrados, mensaje.Message, ApiErrors.errDatosNoEncontrados);
                oResponse.success = true;

                return oResponse;
            }
        }

        /// <summary>
        /// Devuelve un medio de pago
        /// </summary>
        public cResponseMedioDePago GetItem(int medioDePagoID)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (medioDePagoID == 0)
                throw new cCodigoNoDetalladoException("No se especifico un medioDePagoID");   // → 400

            cValor? oValorObtenido = valoresBLL.GetItem((uint)medioDePagoID);

            if (oValorObtenido == null)
                throw new cDatosNoEncontradosException("No se encontró el medio de pago " + medioDePagoID);   // → 404

            cResponseMedioDePago oResponseMedioDePago = new cResponseMedioDePago();
            oResponseMedioDePago.medioDePago = oValorObtenido;
            oResponseMedioDePago.success = true;
            oResponseMedioDePago.paginacion = new cPaginacion(1, 1, 1, 1, 1);

            LoguearInfo("Success MediosDePago GetItem", Endpoint.Maestros.MediosDePago("GetItem"), stopwatch, 1, 1, 1);

            return oResponseMedioDePago;
        }
    }
}

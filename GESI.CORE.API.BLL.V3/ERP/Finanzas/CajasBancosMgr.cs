using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Finanzas;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Finanzas
{
    public interface ICajasBancosServicio
    {
        cResponseCajasBancos GetList(int sucursalID, string estado, int pageNumber, int pageSize);
        cResponseCajaBanco GetItem(int cajaBancoID);
    }

    /// <summary>
    /// Migrado de la V2 (Finanzas.BLL.CajaBancoMgr estático con MiApiSessionManager estático).
    /// Mantiene los mismos pasos que la V2; la sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class CajasBancosServicio(
        ICajasBancosBLL cajasBancosBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), ICajasBancosServicio
    {
        /// <summary>
        /// Devuelve la lista de cajas / bancos
        /// </summary>
        public cResponseCajasBancos GetList(int sucursalID, string estado, int pageNumber, int pageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseCajasBancos oRespuesta = new cResponseCajasBancos();

            // NUEVO respecto de la V2: se respeta el tamaño máximo de página de la habilitación
            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            try
            {
                List<cCajaBanco>? lstCajasBancos = cajasBancosBLL.GetList(intSucursalID: (uint)sucursalID, strEstado: estado);

                if (lstCajasBancos == null || lstCajasBancos.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron cajas / Bancos");

                int cantidadElementos = lstCajasBancos.Count;

                lstCajasBancos = lstCajasBancos.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstCajasBancos.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber));

                oRespuesta.success = true;
                oRespuesta.cajasBancos = lstCajasBancos;
                oRespuesta.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                // La V2 logueaba "Success Almacenes GetList" por error
                LoguearInfo("Success CajasBancos GetList", Endpoint.Maestros.CajasBancos("GetList"), stopwatch, cantidadElementos, pageNumber, pageSize);

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
        /// Devuelve una caja / banco
        /// </summary>
        public cResponseCajaBanco GetItem(int cajaBancoID)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (cajaBancoID == 0)
                throw new cCodigoNoDetalladoException("No se detallo la cajabancoID");   // → 400

            cCajaBanco? oCaja = cajasBancosBLL.GetItem((uint)Sesion.EmpresaID, (uint)cajaBancoID);

            if (oCaja == null)
                throw new cDatosNoEncontradosException("No se encontró la caja / banco " + cajaBancoID);   // → 404

            cResponseCajaBanco oRespuesta = new cResponseCajaBanco();
            oRespuesta.cajaBanco = oCaja;
            oRespuesta.success = true;
            oRespuesta.paginacion = new cPaginacion(1, 1, 1, 1, 1);

            LoguearInfo("Success CajasBancos GetItem", Endpoint.Maestros.CajasBancos("GetItem"), stopwatch, 1, 1, 1);

            return oRespuesta;
        }
    }
}

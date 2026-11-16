using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.ERP.Compras.V3.BLL;
using GESI.ERP.Core.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Compras
{
    public interface IFormasDePagoProveedoresServicio
    {
        cResponseFormasDePago GetList(int pageNumber, int pageSize, string? ordenarPor, string? direccion);
        cResponseFormaDePago GetItem(int formaDePagoID);
    }

    /// <summary>
    /// Migrado de la V2. En la V2 no tenía Mgr propio: usaba FormasDePagoMgr con blProveedores = true,
    /// que en vez del manager de ventas llamaba a FormasDePagoProveedoresManager (Compras).
    /// Acá queda separado: este servicio solo trabaja con las formas de pago de proveedores.
    /// Devuelve las mismas respuestas que FormasDePago (cResponseFormasDePago / cResponseFormaDePago), igual que la V2.
    /// </summary>
    internal sealed class FormasDePagoProveedoresServicio(
        IFormasDePagoProveedoresBLL formasDePagoProveedoresBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IFormasDePagoProveedoresServicio
    {
        /// <summary>
        /// Devuelve la lista de formas de pago de proveedores
        /// </summary>
        public cResponseFormasDePago GetList(int pageNumber, int pageSize, string? ordenarPor, string? direccion)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseFormasDePago oResponse = new cResponseFormasDePago();

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            try
            {
                List<cFormaDePago>? lstFormasDePago = formasDePagoProveedoresBLL.GetList(strOrdenarPor: ordenarPor!, strDireccion: direccion!);

                if (lstFormasDePago == null || lstFormasDePago.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron formas de pago de proveedores");

                int cantidadElementos = lstFormasDePago.Count;

                lstFormasDePago = lstFormasDePago.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstFormasDePago.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, 0));

                oResponse.formasDePago = lstFormasDePago;
                oResponse.success = true;
                oResponse.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success FormasDePagoProveedores GetList", Endpoint.Maestros.FormasDePagoProveedores("GetList"), stopwatch, cantidadElementos, pageNumber, pageSize);

                return oResponse;
            }
            catch (cIntervalException oIntervalo)
            {
                oResponse.paginacion = oIntervalo._paginacion;
                oResponse.success = true;
                oResponse.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);

                return oResponse;
            }
            catch (cDatosNoEncontradosException mensaje)
            {
                LoguearInfo("Not Found FormasDePagoProveedores GetList", Endpoint.Maestros.FormasDePagoProveedores("GetList"), stopwatch, 0, pageNumber, pageSize);

                oResponse.error = new cError((int)cCodigosError.cDatosNoEncontrados, mensaje.Message, ApiErrors.errDatosNoEncontrados);
                oResponse.success = true;

                return oResponse;
            }
        }

        /// <summary>
        /// Devuelve una forma de pago de proveedores
        /// </summary>
        public cResponseFormaDePago GetItem(int formaDePagoID)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (formaDePagoID == 0)
                throw new cCodigoNoDetalladoException("No se detalló el formaDePagoID");   // → 400

            cFormaDePago? oFormaDePago = formasDePagoProveedoresBLL.GetItem(formaDePagoID);

            if (oFormaDePago == null)
                throw new cDatosNoEncontradosException("No se encontró la forma de pago " + formaDePagoID);   // → 404

            cResponseFormaDePago oRespuesta = new cResponseFormaDePago();
            oRespuesta.formaDePago = oFormaDePago;
            oRespuesta.success = true;
            oRespuesta.paginacion = new cPaginacion(1, 1, 1, 1, 1);

            LoguearInfo("Success FormasDePagoProveedores GetItem", Endpoint.Maestros.FormasDePagoProveedores("GetItem"), stopwatch, 1, 1, 1);

            return oRespuesta;
        }
    }
}

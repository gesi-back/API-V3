using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IAplicacionesServicio
    {
        cResponseAplicacionesProducto GetList(int pageNumber, int pageSize);
    }

    /// <summary>
    /// Migrado de la V2. En la V2 no tenía Mgr propio: el controller llamaba a ProductosMgr.AplicacionesaGetList.
    /// Acá queda como servicio aparte (igual que GruposDeProductos) y reutiliza cAplicacionesProducto
    /// y cResponseAplicacionesProducto, que ya estaban migradas en la V3.
    /// </summary>
    internal sealed class AplicacionesServicio(
        IAplicacionesProductoBLL aplicacionesProductoBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IAplicacionesServicio
    {
        /// <summary>
        /// Devuelve la lista de aplicaciones de productos
        /// </summary>
        public cResponseAplicacionesProducto GetList(int pageNumber, int pageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseAplicacionesProducto oResponse = new cResponseAplicacionesProducto();

            // NUEVO respecto de la V2: se respeta el tamaño máximo de página de la habilitación
            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            try
            {
                List<cAplicacionProducto>? lstAplicacionesProducto = aplicacionesProductoBLL.GetList();

                if (lstAplicacionesProducto == null || lstAplicacionesProducto.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron Aplicaciones");

                int cantidadElementos = lstAplicacionesProducto.Count;

                lstAplicacionesProducto = lstAplicacionesProducto.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstAplicacionesProducto.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, 0));

                // Igual que la V2: se devuelve cAplicacionesProducto (sin EmpresaID)
                oResponse.aplicacionProductos = new List<cAplicacionesProducto>();
                foreach (cAplicacionProducto oAplicacion in lstAplicacionesProducto)
                {
                    oResponse.aplicacionProductos.Add(new cAplicacionesProducto(oAplicacion));
                }

                oResponse.success = true;
                oResponse.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success Aplicaciones de Productos GetList", Endpoint.Maestros.Aplicaciones("GetList"), stopwatch, cantidadElementos, pageNumber, pageSize);

                return oResponse;
            }
            catch (cIntervalException oIntervalo)
            {
                // NUEVO respecto de la V2: la V2 no ponía success = true en este caso (el resto de los maestros sí)
                oResponse.success = true;
                oResponse.paginacion = oIntervalo._paginacion;
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
    }
}

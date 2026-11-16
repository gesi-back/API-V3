using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IProveedoresServicio
    {
        cResponseProveedores GetList(int pageNumber, int pageSize);
        cResponseProveedor GetItem(int proveedorID);
    }

    /// <summary>
    /// Migrado de la V2 (Compras.BLL.ProveedoresMgr estático con _MIAPISessionMgr estático).
    /// Mantiene los mismos pasos que la V2; la sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class ProveedoresServicio(
        IProveedoresBLL proveedoresBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IProveedoresServicio
    {
        /// <summary>
        /// Devuelve una lista de proveedores
        /// </summary>
        public cResponseProveedores GetList(int pageNumber, int pageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseProveedores oRespuesta = new cResponseProveedores();

            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            try
            {
                List<cProveedor>? lstProveedores = proveedoresBLL.GetList();

                if (lstProveedores == null || lstProveedores.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron proveedores");

                int cantidadElementos = lstProveedores.Count;

                lstProveedores = lstProveedores.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstProveedores.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, 0));

                oRespuesta.success = true;
                oRespuesta.proveedores = lstProveedores;
                oRespuesta.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                // NUEVO respecto de la V2: la V2 no dejaba log informativo en proveedores
                LoguearInfo("Success Proveedores GetList", Endpoint.Maestros.Proveedores("GetList"), stopwatch, cantidadElementos, pageNumber, pageSize);

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
                // NUEVO respecto de la V2: igual que el resto de los maestros, "sin datos" vuelve 200 con el error adentro
                LoguearInfo("Not Found Proveedores GetList", Endpoint.Maestros.Proveedores("GetList"), stopwatch, 0, pageNumber, pageSize);

                oRespuesta.error = new cError((int)cCodigosError.cDatosNoEncontrados, mensaje.Message, ApiErrors.errDatosNoEncontrados);
                oRespuesta.success = true;

                return oRespuesta;
            }
        }

        /// <summary>
        /// Devuelve un proveedor
        /// </summary>
        public cResponseProveedor GetItem(int proveedorID)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (proveedorID == 0)
                throw new cCodigoNoDetalladoException("No se detalló el proveedorID");   // → 400

            cProveedor? oProveedor = proveedoresBLL.GetItem(proveedorID);

            // NUEVO respecto de la V2: no encontrado devuelve 404 (la V2 devolvía 400)
            if (oProveedor == null)
                throw new cDatosNoEncontradosException("No se encontró el proveedor " + proveedorID);   // → 404

            cResponseProveedor oRespuesta = new cResponseProveedor();
            oRespuesta.proveedor = oProveedor;
            oRespuesta.success = true;
            oRespuesta.paginacion = new cPaginacion(1, 1, 1, 1, 1);

            LoguearInfo("Success Proveedores GetItem proveedorID: " + proveedorID, Endpoint.Maestros.Proveedores("GetItem"), stopwatch, 1, 1, 1);

            return oRespuesta;
        }
    }
}

using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.CORE;
using GESI.CORE.V3.BLL;
using GESI.CORE.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.CORE
{
    public interface IUsuariosServicio
    {
        cResponseOpcionesUtilizadas GetOpcionesUtilizadasPorUsuario(string? usuarioID, int pageNumber, int pageSize);
        cResponseNivelAcceso GetNivelAcceso(int accesoID);
    }

    /// <summary>
    /// Migrado de la V2 (BLL.UsuariosMgr estático con _MIAPISessionMgr estático).
    /// Mantiene los mismos pasos que la V2; la sesión, los BLL del CORE y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class UsuariosServicio(
        IOpcionesUtilizadasPorUsuarioBLL opcionesUtilizadasBLL,
        IUsuariosBLL usuariosBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IUsuariosServicio
    {
        /// <summary>
        /// Devuelve las opciones utilizadas por un usuario
        /// </summary>
        public cResponseOpcionesUtilizadas GetOpcionesUtilizadasPorUsuario(string? usuarioID, int pageNumber, int pageSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseOpcionesUtilizadas oRespuesta = new cResponseOpcionesUtilizadas();

            // NUEVO respecto de la V2: sin usuarioID devuelve 400 (la V2 devolvía 200 con el error adentro)
            if (string.IsNullOrEmpty(usuarioID))
                throw new cCodigoNoDetalladoException("No se detallo el usuarioID");   // → 400

            // NUEVO respecto de la V2: se respeta el tamaño máximo de página de la habilitación
            pageSize = ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize);

            try
            {
                List<cOpcionUtilizadaPorUsuario>? lstOpciones = opcionesUtilizadasBLL.GetList(usuarioID);

                if (lstOpciones == null || lstOpciones.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron opciones utilizadas por el usuario " + usuarioID);

                int cantidadElementos = lstOpciones.Count;

                lstOpciones = lstOpciones.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstOpciones.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber));

                oRespuesta.success = true;
                oRespuesta.opcionesUtilizadas = lstOpciones;
                oRespuesta.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);

                LoguearInfo("Success Opciones Utilizadas Usuarios GetList", Endpoint.CORE.Usuarios("GetOpcionesUtilizadasUsuarios"), stopwatch, cantidadElementos, pageNumber, pageSize);

                return oRespuesta;
            }
            catch (cIntervalException oIntervalo)
            {
                // NUEVO respecto de la V2: la V2 no atrapaba este caso en el Mgr; ahora responde igual que el resto de los listados
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
        /// Devuelve el nivel de acceso del usuario de la sesión para un permiso
        /// </summary>
        public cResponseNivelAcceso GetNivelAcceso(int accesoID)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cResponseNivelAcceso oRespuesta = new cResponseNivelAcceso();

            // NUEVO respecto de la V2: sin accesoID devuelve 400 (la V2 devolvía 200 con el error adentro)
            if (accesoID <= 0)
                throw new cCodigoNoDetalladoException("No se detalló el accesoID");   // → 400

            Dictionary<int, string>? miDiccionario = usuariosBLL.GetNivelDeAccesoConDescripcion(accesoID);

            // NUEVO respecto de la V2: si el Core no devuelve nada, 404 (la V2 fallaba con error 500 en el First())
            if (miDiccionario == null || miDiccionario.Count == 0)
                throw new cDatosNoEncontradosException("No se encontró el nivel de acceso para el accesoID " + accesoID);   // → 404

            oRespuesta.accesoID = accesoID;
            oRespuesta.success = true;
            oRespuesta.nivelDeAcceso = miDiccionario.First().Key;
            oRespuesta.descripcion = miDiccionario.First().Value;

            LoguearInfo("Success Usuarios GetNivelDeAcceso accesoID: " + accesoID, Endpoint.CORE.Usuarios("GetNivelDeAcceso"), stopwatch, 1, 1, 1);

            return oRespuesta;
        }
    }
}

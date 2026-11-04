using GESI.CORE.API.BO.V3;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GESI.CORE.API.BLL.V3
{
    public interface IProductosServicio
    {
        cResponseProductos GetList(int pageNumber, int pageSize,
                                                                            string costoSolicitado,
                                                                            string imagenes,
                                                                            string fechamodificaciones = "",
                                                                            string stock = "N",
                                                                            string publicaEcommerce = "T",
                                                                            string aplicaciones = "N",
                                                                            string fechamodificacionesimagenes = "",
                                                                            string precios = "S",
                                                                            string? sucursales = "",
                                                                            string fechamodificacionesprecios = "",
                                                                            string codigosadicionales = "N",
                                                                            string definicionesporalmacen = "N",
                                                                            string insumos = "N",
                                                                            string impuestos = "N",
                                                                            string cuentas = "N",
                                                                            string elementos = "N",
                                                                            string canalesDeVenta = "",
                                                                            string subproductos = "N",
                                                                            string soloproductospadre = "N",
                                                                            string rubros = "",
                                                                            string subrubros = "",
                                                                            string subsubrubros = "",
                                                                            string filtroArticulos1 = "",
                                                                            string filtroArticulos2 = "",
                                                                            string filtroArticulos3 = "",
                                                                            string? ordenarPor = null,
                                                                            string? direccion = null,
                                                                            string incluirExistenciaAlmacenesEnCero = "N",
                                                                            string incluirExistenciasLotes = "N",
                                                                            string incluirSucursalesHabilitadas = "N",
                                                                            string proveedorID = "",
                                                                            string parametros = "",
                                                                            string incluirLotesDelProducto = "N",
                                                                            string codigos = "",
                                                                            string? estado = null);
    }

    internal sealed class ProductosServicio(
        IProductosBLL productosBLL,
        IAPIHelper APIHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger,
        ICanalesDeVentaBLL canalesDeVentaBLL,
        IImpuestoXProductoBLL impuestosProductoBLL,
        IInsumosProductoBLL insumosProductoBLL) : IProductosServicio
    {
        /// <summary>
        /// Devuelve todos los productos de forma paginada
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="CanalesDeVenta"></param>
        /// <param name="costoSolicitado"></param>
        /// <param name="costoUsuario"></param>
        /// <param name="EstadosProductos"></param>
        /// <param name="CategoriasIDs"></param>
        /// <param name="imagenes"></param>
        /// <param name="fechamodificaciones"></param>
        /// <param name="stock"></param>
        /// <param name="Almacenes"></param>
        /// <param name="publicaEcommerce"></param>
        /// <returns></returns>
        public cResponseProductos GetList(int pageNumber,int pageSize,
                                                                            string costoSolicitado,
                                                                            string imagenes,
                                                                            string fechamodificaciones = "",
                                                                            string stock = "N",
                                                                            string publicaEcommerce = "T",
                                                                            string aplicaciones = "N",
                                                                            string fechamodificacionesimagenes = "",
                                                                            string precios = "S",
                                                                            string? sucursales = "",
                                                                            string fechamodificacionesprecios = "",
                                                                            string codigosadicionales = "N",
                                                                            string definicionesporalmacen = "N",
                                                                            string insumos = "N",
                                                                            string impuestos = "N",
                                                                            string cuentas = "N",
                                                                            string elementos = "N",
                                                                            string canalesDeVenta = "",
                                                                            string subproductos = "N",
                                                                            string soloproductospadre = "N",
                                                                            string rubros = "",
                                                                            string subrubros = "",
                                                                            string subsubrubros = "",
                                                                            string filtroArticulos1 = "",
                                                                            string filtroArticulos2 = "",
                                                                            string filtroArticulos3 = "",
                                                                            string? ordenarPor = null,
                                                                            string? direccion = null,
                                                                            string incluirExistenciaAlmacenesEnCero = "N",
                                                                            string incluirExistenciasLotes = "N",
                                                                            string incluirSucursalesHabilitadas = "N",
                                                                            string proveedorID = "",
                                                                            string parametros = "",
                                                                            string incluirLotesDelProducto = "N",
                                                                            string codigos = "",
                                                                            string? estado = null)
        {

            string strQueEstabaHaciendo = "";
           

            cResponseProductos oRespuesta = new cResponseProductos();
            List<string>? lstCodigosProducto = new List<string>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            try
            {
                pageSize = APIHelper.DevolverTamanoPaginacion((int)sesion.TamanoPagina, pageSize);
                
                List<GESI.ERP.Core.V3.BO.cProducto> lstProductos = new List<ERP.Core.V3.BO.cProducto>();

                string? strCategoriasIDs = sesion.CategoriasIDs;

                if (sesion.CategoriasIDs?.Length == 0)
                    strCategoriasIDs = null;

                strQueEstabaHaciendo = "Verificando si tiene fecha de modificaciones ya sea precios, imagenes o datos generales productosv2Mgr";

                if (fechamodificaciones?.Length > 0 ||
                    fechamodificacionesimagenes?.Length > 0 ||
                    fechamodificacionesprecios?.Length > 0) // Se devuelven los productos a partir de una fecha de modificaciones
                {
                    DateTime fcFechaModificacion;
                    DateTime fcFechaModificacionImagenes;
                    DateTime fcFechaModificacionPrecios;
                    int tablaID = 0;
                    List<string> lstCodigosConModificacionesGenerales = new List<string>();
                    List<string> lstCodigosConModificacionesEnImagenes = new List<string>();
                    List<string> lstCodigosConModificacionesEnPrecios = new List<string>();

                    try
                    {                        
                        if (fechamodificaciones?.Length > 0)
                        {
                            fcFechaModificacion = DateTime.Parse(fechamodificaciones);
                            tablaID = (int)LogCambios.tProductoDatosGenerales;
                            lstCodigosConModificacionesGenerales = productosBLL.GetProductosModificadosDesdeFecha(fcFechaModificacion,
                                                                                                                                            estado,
                                                                                                                                            (ushort)tablaID);
                        }

                        if (fechamodificacionesimagenes?.Length > 0)
                        {
                            fcFechaModificacionImagenes = DateTime.Parse(fechamodificacionesimagenes);
                            tablaID = (int)LogCambios.tImagenesProductos;

                            lstCodigosConModificacionesEnImagenes = productosBLL.GetProductosModificadosDesdeFecha(fcFechaModificacionImagenes,
                                estado, (ushort)tablaID);
                        }

                        if (fechamodificacionesprecios?.Length > 0)
                        {
                            fcFechaModificacionPrecios = DateTime.Parse(fechamodificacionesprecios);
                            tablaID = (int)LogCambios.tPreciosProductos;
                            lstCodigosConModificacionesEnPrecios = productosBLL.GetProductosModificadosDesdeFecha(fcFechaModificacionPrecios,
                                                                                                                                        estado,
                                                                                                                                        (ushort)tablaID);
                        }


                    }
                    catch (cFormatoInvalidoException)
                    {
                        throw;
                    }

                    string strCosto = "";

                    strQueEstabaHaciendo = "Verifica el permiso sobre costos productosv2Mgr";


                    if (VerificarPermisoSobreCostos(sesion.CostosXProveedor, costoSolicitado))
                        strCosto = costoSolicitado;
                    else
                        strCosto = "N";

                    lstCodigosProducto = lstCodigosConModificacionesGenerales.Union(lstCodigosConModificacionesEnImagenes).Union(lstCodigosConModificacionesEnPrecios).ToList();

                    if (lstCodigosProducto?.Count == 0)
                        throw new cDatosNoEncontradosException(new cPaginacion(lstCodigosProducto.Count,
                                                                (int)Math.Ceiling((double)lstCodigosProducto.Count / pageSize),
                                                                pageNumber,
                                                                pageSize,
                                                                pageNumber));


                    

                    strQueEstabaHaciendo = "Verifica los canales de venta habilitados por usuario  productosv2Mgr";

                    List<int> CanalesDeVenta = canalesDeVentaBLL.GetCanalesDeVentaPorUsuario(sesion.EmpresaID,
                                                                                                                            sesion.Usuario ?? "");


                    strQueEstabaHaciendo = "Va al getList a buscar los productos   productosv2Mgr";

                   
                        
                        lstProductos = productosBLL.GetList(string.Join(",", lstCodigosProducto ?? new List<string>()),
                                                                                    CanalesDeVenta.ToArray(),
                                                                                    strCosto,
                                                                                    estado,
                                                                                    sesion.CategoriasIDs ?? "",
                                                                                    imagenes,
                                                                                    stock,
                                                                                    null,
                                                                                    strDevolverAplicaciones: aplicaciones,
                                                                                    strDevolverCodigosAdicionales: codigosadicionales,
                                                                                    strDevolverDefinicionesPorAlmacen: definicionesporalmacen,
                                                                                    strDevolverCuentasXProducto: cuentas,
                                                                                    strDevolverElementosXEntidad: elementos, blnBlIncluirExistenciaAlmacenesEn0: (incluirExistenciaAlmacenesEnCero == "S"), blnBlIncluirExistenciasLotes: (incluirExistenciasLotes.Equals("S")), strDevolverSucursalesHabilitadas: incluirSucursalesHabilitadas, strDevolverLotesDelProducto: incluirLotesDelProducto);





                        if (rubros?.Length > 0)
                        {
                            HashSet<int> rubrosFiltro = rubros
                                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(int.Parse)
                                                        .ToHashSet();

                            List<ERP.Core.V3.BO.cProducto> productosFiltrados = lstProductos
                                .Where(p => rubrosFiltro.Contains(p.RubroID))
                                .ToList();

                            lstProductos = productosFiltrados;
                        }

                        if (filtroArticulos1?.Length > 0)
                        {
                            HashSet<int> filtroArticulos1Filtro = filtroArticulos1
                                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(int.Parse)
                                                        .ToHashSet();

                            List<ERP.Core.V3.BO.cProducto> productosFiltrados = lstProductos
                                .Where(p => filtroArticulos1Filtro.Contains(p.FiltroArticulos1ID))
                                .ToList();

                            lstProductos = productosFiltrados;
                        }


                        if (filtroArticulos2?.Length > 0)
                        {
                            HashSet<int> filtroArticulos2Filtro = filtroArticulos2
                                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(int.Parse)
                                                        .ToHashSet();

                            List<ERP.Core.V3.BO.cProducto> productosFiltrados = lstProductos
                                .Where(p => filtroArticulos2Filtro.Contains(p.FiltroArticulos2ID))
                                .ToList();

                            lstProductos = productosFiltrados;
                        }

                        if (filtroArticulos3?.Length > 0)
                        {
                            HashSet<int> filtroArticulos3Filtro = filtroArticulos3
                                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(int.Parse)
                                                        .ToHashSet();

                            List<ERP.Core.V3.BO.cProducto> productosFiltrados = lstProductos
                                .Where(p => filtroArticulos3Filtro.Contains(p.FiltroArticulos3ID))
                                .ToList();

                            lstProductos = productosFiltrados;
                        }

                        lstProductos = lstProductos.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();



                    if (strCosto.Equals("N") && !costoSolicitado.Equals("N"))
                    {
                        oRespuesta.error = new cError((int)cCodigosError.cPermisoDenegadoCostos, "No contiene permisos para visualizar costos   productosv2Mgr", ApiErrors.errNoAutorizado);
                    }


                }
                else // Se devuelven todos los productos.
                {
                    int? publica = FiltroEcommerce(publicaEcommerce);

                    if (sucursales?.Length == 0)
                        sucursales = null;


                    strQueEstabaHaciendo = "Utiliza el getSearchResult para buscar todos los productos   productosv2Mgr";

                    string? strMensaje = "";

                    if (codigos?.Length > 0)
                        lstCodigosProducto = codigos.Split(',').ToList();
                    else
                        (lstCodigosProducto, strMensaje) =productosBLL.GetSearchResults(strEstado: estado,
                                                                                                    strCategorias: sesion.CategoriasIDs,
                                                                                                    shtPublicaECommerce: (ushort?)publica,
                                                                                                    strSucursales: sucursales,
                                                                                                    blnSoloProductosPadre: soloproductospadre == "S",
                                                                                                    strRubros: rubros,
                                                                                                    strSubRubros: subrubros,
                                                                                                    strSubSubRubros: subsubrubros,
                                                                                                    strFiltroArticulos1: filtroArticulos1,
                                                                                                    strFiltroArticulos2: filtroArticulos2,
                                                                                                    strFiltroArticulos3: filtroArticulos3,
                                                                                                    strOrdenarPor: ordenarPor,
                                                                                                    strDireccion: direccion, strProveedoresIDs: proveedorID);


                    string strCosto = "";
                    if (VerificarPermisoSobreCostos(sesion.CostosXProveedor, costoSolicitado))
                        strCosto = costoSolicitado;
                    else
                        strCosto = "N";


                    List<int> CanalesDeVentaXUsuario = new List<int>();

                    if (precios.Equals("S"))
                    {
                        
                        strQueEstabaHaciendo = "Verifica los canales de venta habilitados por Usuario en la obtencion de todos los articulos    productosv2Mgr";

                        CanalesDeVentaXUsuario = canalesDeVentaBLL.GetCanalesDeVentaPorUsuario(sesion.EmpresaID,
                                                                                                                        sesion.Usuario ?? "");
                    }



                    if (canalesDeVenta?.Length > 0)   // SI PASA POR PARAMETRO CANALES DE VENTA LOS FILTRA 
                    {
                        List<int> lstCanalesDeVenta = canalesDeVenta
                                        .Split(',')
                                        .Select(int.Parse)
                                        .ToList();

                        List<int> lstNuevaListaCanalesDeVenta = lstCanalesDeVenta.Intersect(CanalesDeVentaXUsuario).ToList();
                        CanalesDeVentaXUsuario = lstNuevaListaCanalesDeVenta;
                    }

                    strQueEstabaHaciendo = "Va al getList a buscar los productos    productosv2Mgr";
 
                        lstProductos = productosBLL.GetList(APIHelper.DevolverCodigosSeparadosPorComa(lstCodigosProducto, pageNumber, pageSize),
                                                                                     CanalesDeVentaXUsuario.ToArray(),
                                                                                     strCosto,
                                                                                     estado,
                                                                                     sesion.CategoriasIDs ?? "",
                                                                                     imagenes,
                                                                                     stock, null,
                                                                                     strDevolverAplicaciones: aplicaciones,
                                                                                     strDevolverCodigosAdicionales: codigosadicionales,
                                                                                     strDevolverDefinicionesPorAlmacen: definicionesporalmacen,
                                                                                     strDevolverCuentasXProducto: cuentas,
                                                                                     strDevolverElementosXEntidad: elementos, strDevolverSubProductos: subproductos,
                                                                                     blnBlIncluirExistenciaAlmacenesEn0: (incluirExistenciaAlmacenesEnCero == "S"), blnBlIncluirExistenciasLotes: (incluirExistenciasLotes.Equals("S")), strDevolverSucursalesHabilitadas: incluirSucursalesHabilitadas, strDevolverLotesDelProducto: incluirLotesDelProducto);

                    
                    


                    // NO TIENE PERMISOS PARA VISUALIZAR COSTOS 
                    if (strCosto.Equals("N") &&
                        !costoSolicitado.Equals("N"))
                    {
                        oRespuesta.error = new cError((int)cCodigosError.cPermisoDenegadoCostos, "No contiene permisos para visualizar costos    productosv2Mgr", ApiErrors.errNoAutorizado);
                    }

                }

                if (lstCodigosProducto?.Count == 0)
                    throw new cCodigoNoDetalladoException();


                //  INSUMOS PRODUCTO

                strQueEstabaHaciendo = "Evalua si se deben devolver los Insumos    productosv2Mgr";

                List<cInsumoProducto>? lstInsumos = new List<cInsumoProducto>();
                if (insumos.Equals("S"))
                {
                    lstInsumos = insumosProductoBLL.GetList(string.Join(",", lstCodigosProducto ?? new List<string>()));
                }

                List<cImpuestoXProducto>? lstImpuestos = new List<cImpuestoXProducto>();

                strQueEstabaHaciendo = "Evalua si se deben devolver los Impuestos    productosv2Mgr";

                if (impuestos.Equals("S"))
                {
                   lstImpuestos = impuestosProductoBLL.GetList(string.Join(",", lstCodigosProducto ?? new List<string>()));
                }


                #region Paginacion
                cPaginacion oPaginacion = new cPaginacion(lstCodigosProducto?.Count ?? 0,
                                                            (int)Math.Ceiling((double)(lstCodigosProducto?.Count ?? 0) / pageSize),
                                                            pageNumber,
                                                            pageSize,
                                                            pageNumber);
                oRespuesta.paginacion = oPaginacion;
                #endregion

                #region Pasaje a cProducto API                
                List<GESI.CORE.API.BO.V3.cProducto> lstHijos = new List<GESI.CORE.API.BO.V3.cProducto>();

                strQueEstabaHaciendo = "Hago el pasaje a cProducto propio de la API    productosv2Mgr";


                foreach (GESI.ERP.Core.V3.BO.cProducto oPrd in lstProductos)
                {
                    if (lstInsumos?.Count > 0)
                    {
                        oPrd.Insumos = lstInsumos.Where(x => x.ProductoID == oPrd.ProductoID).ToList();
                    }

                    if (lstImpuestos?.Count > 0)
                    {
                        oPrd.Impuestos = lstImpuestos.Where(x => x.ProductoID == oPrd.ProductoID).ToList();
                    }

                    GESI.CORE.API.BO.V3.cProducto oHijo = new GESI.CORE.API.BO.V3.cProducto(oPrd);

                    if (oPrd.Categorias?.Count > 0)
                    {
                        foreach (GESI.ERP.Core.V3.BO.cCategoriaXProducto oCategoria in oPrd.Categorias)
                        {
                            oHijo.ListaDeCategorias.Add(oCategoria.CategoriaID);
                        }
                    }

                    lstHijos.Add(oHijo);
                }
                oRespuesta.productos = lstHijos;
                oRespuesta.success = true;
                #endregion


                stopwatch.Stop();


                TimeSpan ts = stopwatch.Elapsed;

                string elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                apiLogger.LoguearSucesosAPI("Success Productos GetList",tTiposDeError.tInformativo,sesion.Usuario,
                                                Endpoint.Maestros.Productos("GetList"),sesion.EmpresaID,sesion.SucursalID,
                                                cantidadDeElementos: lstCodigosProducto?.Count ?? 0,
                                                TiempoEjecucion: elapsedTime,
                                                numeroPagina: pageNumber,
                                                tamanoPagina: pageSize, parametros: parametros ?? "", IPRemoteDesktop: sesion.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion.Forwarded ?? "",
                                                                        Agent: sesion.UserAgent ?? "");

                return oRespuesta;

            }
            catch (SqlException)
            {
                stopwatch.Stop();
                throw;
            }
            catch (cDatosNoEncontradosException oDatosException)
            {
                stopwatch.Stop();

                oRespuesta.paginacion = oDatosException.Paginacion;

                apiLogger.LoguearSucesosAPI("Not Found GetList", tTiposDeError.tInformativo, sesion.Usuario,
                                                Endpoint.Maestros.Productos("GetList"), sesion.EmpresaID, sesion.SucursalID,
                                                cantidadDeElementos: lstCodigosProducto?.Count ?? 0,
                                                numeroPagina: pageNumber,
                                                tamanoPagina: pageSize, parametros: parametros ?? "", IPRemoteDesktop: sesion.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion.Forwarded ?? "",
                                                                        Agent: sesion.UserAgent ?? "");                

                oRespuesta.error = new cError();
                oRespuesta.error.code = (int)cCodigosError.cDatosNoEncontrados;
                oRespuesta.error.message = "No se encontraron elementos";

                return oRespuesta;
            }
            catch (cIntervalException oIntervalo)
            {
                #region INTERVALO EXCEDIDO
                stopwatch.Stop();
                oRespuesta.paginacion = oIntervalo._paginacion;
                oRespuesta.success = true;
                oRespuesta.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);


                return oRespuesta;   // DEVUELVE EL OBJETO PAGINACION . CASO EN EL QUE SE EXCEDA EL NUMERO DE PAGINA
                #endregion
            }
            catch (cCodigoNoDetalladoException)
            {
                stopwatch.Stop();

                apiLogger.LoguearSucesosAPI("Not Found Productos GetList", tTiposDeError.tInformativo, sesion.Usuario,
                                                Endpoint.Maestros.Productos("GetList"), sesion.EmpresaID, sesion.SucursalID,
                                                cantidadDeElementos: lstCodigosProducto?.Count ?? 0,
                                                numeroPagina: pageNumber,
                                                tamanoPagina: pageSize, parametros: parametros ?? "", IPRemoteDesktop: sesion.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion.Forwarded ?? "",
                                                                        Agent: sesion.UserAgent ?? "");

                throw;
            }
            catch (cFormatoInvalidoException)
            {
                stopwatch.Stop();

                throw new cFormatoInvalidoException("Conversion incorrecta de fecha");
            }
            catch (Exception)
            {
                stopwatch.Stop();

                apiLogger.LoguearSucesosAPI("Not Found Productos GetList", tTiposDeError.tError, sesion.Usuario,
                                                Endpoint.Maestros.Productos("GetList"), sesion.EmpresaID, sesion.SucursalID,
                                                cantidadDeElementos: lstCodigosProducto?.Count ?? 0,
                                                numeroPagina: pageNumber,
                                                tamanoPagina: pageSize, parametros: parametros ?? "", IPRemoteDesktop: sesion.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion.Forwarded ?? "",
                                                                        Agent: sesion.UserAgent ?? "");             

                throw;
            }
        }

        private bool VerificarPermisoSobreCostos(string? costoUsuario,
                                                       string costoSolicitado)
        {
            bool Habilitado = true;
            switch (costoUsuario)
            {
                case "N":
                    switch (costoSolicitado)
                    {
                        case "P":
                            Habilitado = false;
                            break;
                        case "S":
                            Habilitado = false;
                            break;
                    }
                    break;
                case "P":
                    switch (costoSolicitado)
                    {
                        case "S":
                            Habilitado = false;
                            break;
                    }
                    break;
            }
            return Habilitado;
        }

        private int? FiltroEcommerce(string publicaEcommerce)
        {
            int? publica = null;

            switch (publicaEcommerce)
            {
                case "N":
                    publica = 0;
                    break;

                case "S":
                    publica = 255;
                    break;

                default:
                    publica = null;
                    break;
            }
            return publica;
        }
    }
}

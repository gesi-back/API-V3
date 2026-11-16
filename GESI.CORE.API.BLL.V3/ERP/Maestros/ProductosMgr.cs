using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.CORE;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.CORE.V3.BLL;
using GESI.CORE.V3.BO;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using GESI.ERP.Core.V3.BO.Dinamicos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
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

        cResponseProductos GetSearchResults(string expresion = "",
                                                                                    int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                                    int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                                                    string costos = "N",
                                                                                    string categoriasafiltrar = "",
                                                                                    string imagenes = "N",
                                                                                    string stock = "N",
                                                                                    string publicaecommerce = "T",
                                                                                    string aplicaciones = "N",
                                                                                    string precios = "S",
                                                                                    string codigosadicionales = "N",
                                                                                    string definicionesporalmacen = "N",
                                                                                    string insumos = "N",
                                                                                    string impuestos = "N",
                                                                                    string cuentas = "N",
                                                                                    string elementos = "N",
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
                                                                                    string? estado = null,
                                                                                    string incluirSucursalesHabilitadas = "N",
                                                                                    string proveedorID = "",
                                                                                    string? parametros = "",
                                                                                    string? token = "",
                                                                                    string incluirLotesDelProducto = "N");

        cResponseProducto GetItem(string productoID = "",
                                                                            int canalDeVentaID = 0,
                                                                            string costos = "N",
                                                                            string imagenes = "N",
                                                                            string stock = "N",
                                                                            string insumos = "N",
                                                                            string impuestos = "N",
                                                                            string cuentas = "N",
                                                                            string elementos = "N",
                                                                            string subproductos = "N",
                                                                            string strIncluirExistenciasEnCero = "N",
                                                                            string incluirExistenciasLotes = "N",
                                                                            string incluirSucursalesHabilitadas = "N");

        cResponseHistorialCostos HistorialCostosGet(int pageNumber,
                                                                   int pageSize,
                                                                   string? productoID);

        List<cResponseProductosSave> Save(List<GESI.ERP.Core.V3.BO.cProducto> lstProductos, bool blAlta);

        cConfiguraciones GetConfiguraciones(string seccionID = "", string itemID = "");

        List<dActualizacionCostoProductoProveedorResponse> ActualizacionCostosPorProveedor(List<cActualizacionCosto>? lstCostos, bool blnAgregarFaltantes);
    }

    internal sealed class ProductosServicio(
        IProductosBLL productosBLL,
        IAPIHelper APIHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger,
        ICanalesDeVentaBLL canalesDeVentaBLL,
        IImpuestoXProductoBLL impuestosProductoBLL,
        IInsumosProductoBLL insumosProductoBLL,
        IAplicacionesProductoBLL aplicacionesProductoBLL,
        IGrupoProductosBLL gruposProductosBLL,
        ICostosHistoricosBLL costosHistoricosBLL,
        IConfiguracionBaseBLL configuracionesBLL) : IProductosServicio
    {
        // Igual que la V2: bloqueo para que dos actualizaciones de costos no se pisen (static porque el servicio es Scoped)
        private static readonly object _lockActualizacionCostos = new object();

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

            // Parámetros S/N/P/T sin distinguir mayúsculas
            costoSolicitado = Flag(costoSolicitado);
            imagenes = Flag(imagenes);
            stock = Flag(stock);
            publicaEcommerce = Flag(publicaEcommerce, "T");
            aplicaciones = Flag(aplicaciones);
            precios = Flag(precios, "S");
            codigosadicionales = Flag(codigosadicionales);
            definicionesporalmacen = Flag(definicionesporalmacen);
            insumos = Flag(insumos);
            impuestos = Flag(impuestos);
            cuentas = Flag(cuentas);
            elementos = Flag(elementos);
            subproductos = Flag(subproductos);
            soloproductospadre = Flag(soloproductospadre);
            incluirExistenciaAlmacenesEnCero = Flag(incluirExistenciaAlmacenesEnCero);
            incluirExistenciasLotes = Flag(incluirExistenciasLotes);
            incluirSucursalesHabilitadas = Flag(incluirSucursalesHabilitadas);
            incluirLotesDelProducto = Flag(incluirLotesDelProducto);

            string strQueEstabaHaciendo = "";


            cResponseProductos oRespuesta = new cResponseProductos();
            List<string>? lstCodigosProducto = new List<string>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            try
            {
                pageSize = APIHelper.DevolverTamanoPaginacion((int)sesion.TamanoPagina, pageSize);
                
                List<GESI.ERP.Core.V3.BO.cProducto> lstProductos = new List<GESI.ERP.Core.V3.BO.cProducto>();

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

                            List<GESI.ERP.Core.V3.BO.cProducto> productosFiltrados = lstProductos
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

                            List<GESI.ERP.Core.V3.BO.cProducto> productosFiltrados = lstProductos
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

                            List<GESI.ERP.Core.V3.BO.cProducto> productosFiltrados = lstProductos
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

                            List<GESI.ERP.Core.V3.BO.cProducto> productosFiltrados = lstProductos
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
                List<BO.V3.ERP.Maestros.cProducto> lstHijos = new List<BO.V3.ERP.Maestros.cProducto>();

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

                    BO.V3.ERP.Maestros.cProducto oHijo = new BO.V3.ERP.Maestros.cProducto(oPrd);

                    if (oPrd.Categorias?.Count > 0)
                    {
                        foreach (cCategoriaXProducto oCategoria in oPrd.Categorias)
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

        /// <summary>
        /// Indica si el usuario puede ver el nivel de costos solicitado.
        /// Deniega por defecto: un permiso de usuario nulo, vacío o desconocido equivale a "N",
        /// y un valor solicitado desconocido no se habilita.
        /// </summary>
        private static string Flag(string? valor, string porDefecto = "N")
            => string.IsNullOrWhiteSpace(valor) ? porDefecto : valor.Trim().ToUpperInvariant();

        private static bool VerificarPermisoSobreCostos(string? costoUsuario,
                                                       string? costoSolicitado)
        {
            if (costoSolicitado == "N")
                return true;   // no pide costos: siempre permitido

            return (costoUsuario, costoSolicitado) switch
            {
                ("S", "S") => true,
                ("S", "P") => true,
                ("P", "P") => true,
                _ => false,
            };
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


        /// <summary>
        /// Devuelve un conjunto de resultados de una busqueda por expresión
        /// </summary>
        /// <param name="expresion"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="costos"></param>
        /// <param name="categoriasafiltrar"></param>
        /// <param name="imagenes"></param>
        /// <param name="stock"></param>
        /// <param name="publicaecommerce"></param>
        /// <returns></returns>
        public cResponseProductos GetSearchResults(string expresion = "",
                                                                                    int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                                    int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                                                    string costos = "N",
                                                                                    string categoriasafiltrar = "",
                                                                                    string imagenes = "N",
                                                                                    string stock = "N",
                                                                                    string publicaecommerce = "T",
                                                                                    string aplicaciones = "N",
                                                                                    string precios = "S",
                                                                                    string codigosadicionales = "N",
                                                                                    string definicionesporalmacen = "N",
                                                                                    string insumos = "N",
                                                                                    string impuestos = "N",
                                                                                    string cuentas = "N",
                                                                                    string elementos = "N",
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
                                                                                    string? estado = null,
                                                                                    string incluirSucursalesHabilitadas = "N",
                                                                                    string proveedorID = "",
                                                                                    string? parametros = "",
                                                                                    string? token = "",
                                                                                    string incluirLotesDelProducto = "N")
        {
            // Parámetros S/N/P/T sin distinguir mayúsculas
            costos = Flag(costos);
            imagenes = Flag(imagenes);
            stock = Flag(stock);
            publicaecommerce = Flag(publicaecommerce, "T");
            aplicaciones = Flag(aplicaciones);
            precios = Flag(precios, "S");
            codigosadicionales = Flag(codigosadicionales);
            definicionesporalmacen = Flag(definicionesporalmacen);
            insumos = Flag(insumos);
            impuestos = Flag(impuestos);
            cuentas = Flag(cuentas);
            elementos = Flag(elementos);
            subproductos = Flag(subproductos);
            soloproductospadre = Flag(soloproductospadre);
            incluirSucursalesHabilitadas = Flag(incluirSucursalesHabilitadas);
            incluirLotesDelProducto = Flag(incluirLotesDelProducto);

            cResponseProductos? oResponse = new cResponseProductos();

            List<string>? lstCodigosProducto = new List<string>();

            string strQueEstabaHaciendo = "";
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
               
                pageSize = APIHelper.DevolverTamanoPaginacion((int)sesion.TamanoPagina, pageSize);

                strQueEstabaHaciendo = "Verificando si se filtra por categorias";

                string? strCategoriasIDs = sesion.CategoriasIDs;

                if (sesion.CategoriasIDs?.Length == 0)
                    strCategoriasIDs = null;

                expresion = Uri.UnescapeDataString(expresion);

                List<GESI.ERP.Core.V3.BO.cProducto> lstProductos = new List<GESI.ERP.Core.V3.BO.cProducto>();

                strQueEstabaHaciendo = "Obteniendo codigos de productos";
                string? strMensaje = "";
                (lstCodigosProducto, strMensaje) = productosBLL.GetSearchResults(expresion,
                                                                                            estado,
                                                                                            categoriasafiltrar,
                                                                                            sesion.CategoriasIDs ?? "",
                                                                                            (ushort?)FiltroEcommerce(publicaecommerce), blnSoloProductosPadre: soloproductospadre == "S", strRubros: rubros, strSubRubros: subrubros, strSubSubRubros: subsubrubros,
                                                                                                strFiltroArticulos1: filtroArticulos1, strFiltroArticulos2: filtroArticulos2,
                                                                                                strFiltroArticulos3: filtroArticulos3, strOrdenarPor: ordenarPor, strDireccion: direccion, strProveedoresIDs: proveedorID);

                string strCosto = "";

                strQueEstabaHaciendo = "Verificando permisos sobre costos";

                if (VerificarPermisoSobreCostos(sesion?.CostosXProveedor, costos))
                    strCosto = costos;
                else
                    strCosto = "N";

                List<int> CanalesDeVenta = new List<int>();

                if (precios.Equals("S"))
                {
                    strQueEstabaHaciendo = "Obteniendo canales de venta habilitados por Usuario";                    
                    CanalesDeVenta = canalesDeVentaBLL.GetCanalesDeVentaPorUsuario
                                       (sesion?.EmpresaID ?? 0, sesion?.Usuario ?? "");
                }

                if (lstCodigosProducto == null)
                    throw new cDatosNoEncontradosException("No se encontraron resultados");

                if (lstCodigosProducto.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron resultados");


                strQueEstabaHaciendo = "Buscando datos de productos (Antes de GetList)";

                lstProductos = productosBLL.GetList(APIHelper.DevolverCodigosSeparadosPorComa(lstCodigosProducto, pageNumber, pageSize),
                                                                            CanalesDeVenta.ToArray(),
                                                                            strCosto,
                                                                            sesion?.EstadoProductos,
                                                                            categoriasafiltrar,
                                                                            imagenes,
                                                                            stock,
                                                                            sesion?.Almacenes,
                                                                            (ushort?)FiltroEcommerce(publicaecommerce),
                                                                            strDevolverAplicaciones: aplicaciones,
                                                                            strDevolverCodigosAdicionales: codigosadicionales,
                                                                            strDevolverDefinicionesPorAlmacen: definicionesporalmacen,
                                                                            strDevolverCuentasXProducto: cuentas,
                                                                            strDevolverElementosXEntidad: elementos,
                                                                            strDevolverSubProductos: subproductos, strDevolverSucursalesHabilitadas: incluirSucursalesHabilitadas, strDevolverLotesDelProducto: incluirLotesDelProducto);


                #region Paginacion
                cPaginacion oPaginacion = new cPaginacion(lstCodigosProducto.Count,
                                                        (int)Math.Ceiling((double)lstCodigosProducto.Count / pageSize),
                                                        pageNumber,
                                                        pageSize,
                                                        pageNumber);
                oResponse.paginacion = oPaginacion;
                #endregion

                //---------------------INSUMOS PRODUCTOS-----------------------------

                List<cInsumoProducto>? lstInsumos = new List<cInsumoProducto>();
                if (insumos.Equals("S"))
                {
                    strQueEstabaHaciendo = "Obteniendo Insumos de Productos";

                    lstInsumos = insumosProductoBLL.GetList(string.Join(",", lstCodigosProducto));
                }

                //---------------------IMPUESTOS PRODUCTOS-----------------

                List<cImpuestoXProducto>? lstImpuestos = new List<cImpuestoXProducto>();

                if (impuestos.Equals("S"))
                {
                    strQueEstabaHaciendo = "Obteniendo impuestos de productos";
                    lstImpuestos = impuestosProductoBLL.GetList(string.Join(",", lstCodigosProducto));
                }

                #region Pasaje a cProducto API

                if (lstProductos?.Count > 0)
                {
                    strQueEstabaHaciendo = "Haciendo el pasaje a cProducto propio de la API";
                    List<BO.V3.ERP.Maestros.cProducto> lstHijos = new List<BO.V3.ERP.Maestros.cProducto>();
                    foreach (GESI.ERP.Core.V3.BO.cProducto oPrd in lstProductos)
                    {
                        if (lstInsumos?.Count > 0) // AGREGO LOS INSUMOS AL PRODUCTO
                        {
                            oPrd.Insumos = lstInsumos.Where(x => x.ProductoID == oPrd.ProductoID).ToList();
                        }

                        if (lstImpuestos?.Count > 0)
                        {
                            oPrd.Impuestos = lstImpuestos.Where(x => x.ProductoID == oPrd.ProductoID).ToList();
                        }

                        BO.V3.ERP.Maestros.cProducto oHijo = new BO.V3.ERP.Maestros.cProducto(oPrd);

                        if (oPrd.Categorias?.Count > 0)
                        {
                            foreach (cCategoriaXProducto oCategoria in oPrd.Categorias)
                            {
                                oHijo.ListaDeCategorias.Add(oCategoria.CategoriaID);
                            }
                        }

                        lstHijos.Add(oHijo);
                    }

                    oResponse.productos = lstHijos;
                }
                else
                    throw new cDatosNoEncontradosException("No se encontraron resultados");

                oResponse.success = true;
                #endregion


                stopwatch.Stop();


                TimeSpan ts = stopwatch.Elapsed;

                string elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                apiLogger.LoguearSucesosAPI("Success Productos GetSearchResults: " + expresion,tTiposDeError.tInformativo,sesion?.Usuario,
                                                Endpoint.Maestros.Productos("GetSearchResults"),sesion?.EmpresaID ?? 0,sesion?.SucursalID ?? 0,
                                                cantidadDeElementos: lstCodigosProducto.Count,
                                                numeroPagina: pageNumber, tamanoPagina: pageSize, parametros: parametros ?? "", TiempoEjecucion: elapsedTime,
                                                IPRemoteDesktop: sesion?.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion?.Forwarded ?? "",
                                                                        Agent: sesion?.UserAgent ?? "");

                return oResponse;
            }
            catch (SqlException)
            {
                stopwatch.Stop();
                throw;
            }
            catch (cIntervalException oIntervalo)
            {
                stopwatch.Stop();
                #region INTERVALO EXCEDIDO
                oResponse.paginacion = oIntervalo._paginacion;
                oResponse.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);

                oResponse.success = true;

                return oResponse;
                #endregion
            }
            catch (cDatosNoEncontradosException mensaje)
            {
                strQueEstabaHaciendo = "No se encontraron productos";
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                apiLogger.LoguearSucesosAPI("Not Found Productos GetSearchResults: " + mensaje, tTiposDeError.tInformativo, sesion.Usuario,
                                                Endpoint.Maestros.Productos("GetSearchResults"), sesion.EmpresaID, sesion.SucursalID,
                                                cantidadDeElementos: lstCodigosProducto?.Count ?? 0,
                                                numeroPagina: pageNumber, tamanoPagina: pageSize, parametros: parametros ?? "", TiempoEjecucion: elapsedTime,
                                                IPRemoteDesktop: sesion?.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion?.Forwarded ?? "",
                                                                        Agent: sesion?.UserAgent ?? "");

                oResponse.error = new cError();
                oResponse.error.code = (int)cCodigosError.cDatosNoEncontrados;
                oResponse.error.message = mensaje.Message;
                oResponse.success = true;
                return oResponse;
            }
            catch (ArgumentNullException)
            {
                stopwatch.Stop();
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                apiLogger.LoguearSucesosAPI(ex.Message + " Haciendo pv2: " + strQueEstabaHaciendo + " - Token:" + token, tTiposDeError.tError, sesion.Usuario,
                                                Endpoint.Maestros.Productos("GetSearchResults"), sesion.EmpresaID, sesion.SucursalID,
                                                cantidadDeElementos: lstCodigosProducto?.Count ?? 0,
                                                numeroPagina: pageNumber, tamanoPagina: pageSize, parametros: parametros ?? "",
                                                IPRemoteDesktop: sesion?.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion?.Forwarded ?? "",
                                                                        Agent: sesion?.UserAgent ?? "");               
                throw;
            }
        }

        public cResponseProducto GetItem(string productoID = "",
                                                                            int canalDeVentaID = 0,
                                                                            string costos = "N",
                                                                            string imagenes = "N",
                                                                            string stock = "N",
                                                                            string insumos = "N",
                                                                            string impuestos = "N",
                                                                            string cuentas = "N",
                                                                            string elementos = "N",
                                                                            string subproductos = "N",
                                                                            string strIncluirExistenciasEnCero = "N",
                                                                            string incluirExistenciasLotes = "N",
                                                                            string incluirSucursalesHabilitadas = "N")
        {
            // Parámetros S/N/P sin distinguir mayúsculas
            costos = Flag(costos);
            imagenes = Flag(imagenes);
            stock = Flag(stock);
            insumos = Flag(insumos);
            impuestos = Flag(impuestos);
            cuentas = Flag(cuentas);
            elementos = Flag(elementos);
            subproductos = Flag(subproductos);
            strIncluirExistenciasEnCero = Flag(strIncluirExistenciasEnCero);
            incluirExistenciasLotes = Flag(incluirExistenciasLotes);
            incluirSucursalesHabilitadas = Flag(incluirSucursalesHabilitadas);

            cResponseProducto oResponse = new cResponseProducto();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {

                List<GESI.ERP.Core.V3.BO.cProducto> lstProductos = new List<GESI.ERP.Core.V3.BO.cProducto>();

                if (string.IsNullOrEmpty(productoID))
                    throw new cCodigoNoDetalladoException("Se debe especificar el parametro productoID");

                string? strCostosXProveedor = sesion?.CostosXProveedor;
                if (string.IsNullOrEmpty((strCostosXProveedor ?? "")))
                    strCostosXProveedor = "N";

                string strCosto = "";
                if (VerificarPermisoSobreCostos(strCostosXProveedor, costos))
                    strCosto = costos;
                else
                    strCosto = "N";



                List<int> CanalesDeVenta = canalesDeVentaBLL.GetCanalesDeVentaPorUsuario(sesion?.EmpresaID ?? 0,
                                                                                                                    sesion?.Usuario ?? "");


                string decodificado1 = WebUtility.UrlDecode(productoID);

                // Segunda decodificación
                productoID = WebUtility.UrlDecode(decodificado1);


                lstProductos = productosBLL.GetList(productoID,
                                                                            CanalesDeVenta.ToArray(),
                                                                            strCosto,
                                                                            sesion?.EstadoProductos ,
                                                                            strDevolverExistencias: stock,
                                                                            strDevolverImagenes: imagenes,
                                                                            intAAlmacenes: sesion?.Almacenes,
                                                                            strDevolverCuentasXProducto: cuentas,
                                                                            strDevolverElementosXEntidad: elementos, strDevolverSubProductos: subproductos, blnBlIncluirExistenciaAlmacenesEn0: (strIncluirExistenciasEnCero == "S"), blnBlIncluirExistenciasLotes: (incluirExistenciasLotes.Equals("S")), strDevolverSucursalesHabilitadas: incluirSucursalesHabilitadas);

                if (lstProductos?.Count > 0)
                    oResponse.producto = new BO.V3.ERP.Maestros.cProducto(lstProductos[0]);
                else
                    throw new cDatosNoEncontradosException("No se encontró el producto " + productoID);

                //------------ INSUMOS PRODUCTO ------------------------------------------

                if (insumos.Equals("S"))
                {
                    oResponse.producto.Insumos = insumosProductoBLL.GetList(oResponse.producto.ProductoID) ?? new List<cInsumoProducto>();
                }

                //-------------- IMPUESTOS PRODUCTO --------------------------------------

                if (impuestos.Equals("S"))
                {
                    oResponse.producto.Impuestos = impuestosProductoBLL.GetList(oResponse.producto.ProductoID) ?? new List<cImpuestoXProducto>();
                }


                oResponse.success = true;
                oResponse.paginacion = new cPaginacion(1, 1, 1, 1, 1);

                stopwatch.Stop();


                TimeSpan ts = stopwatch.Elapsed;

                string elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                apiLogger.LoguearSucesosAPI(
                    "Success Productos GetItem: " + productoID,tTiposDeError.tInformativo,sesion?.Usuario,
                    Endpoint.Maestros.Productos("GetItem"),sesion?.EmpresaID ?? 0,sesion?.SucursalID ?? 0,
                    cantidadDeElementos: 1,
                    numeroPagina: 1,
                    tamanoPagina: 1, TiempoEjecucion: elapsedTime, IPRemoteDesktop: sesion?.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion?.Forwarded ?? "",
                                                                        Agent: sesion?.UserAgent ?? "");

                return oResponse;

            }
            catch (SqlException mensaje)
            {
                stopwatch.Stop();
                oResponse.error = new cError();
                oResponse.error.code = (int)cCodigosError.cErrorInternoAplicacion;
                oResponse.error.message = mensaje.Message;
                oResponse.success = true;

                return oResponse;
            }
            catch (cCodigoNoDetalladoException mensaje)
            {
                stopwatch.Stop();
                oResponse.error = new cError();
                oResponse.error.code = (int)cCodigosError.cCodigoDetalladoEnLaSolicitud;
                oResponse.error.message = mensaje.Message;
                oResponse.success = true;

                return oResponse;
            }
            catch (cDatosNoEncontradosException mensaje)
            {
                stopwatch.Stop();

                apiLogger.LoguearSucesosAPI(
                    "Not Found Productos GetItem: " + productoID, tTiposDeError.tInformativo, sesion.Usuario,
                    Endpoint.Maestros.Productos("GetItem"), sesion.EmpresaID, sesion.SucursalID,
                    cantidadDeElementos: 1,
                    numeroPagina: 1,
                    tamanoPagina: 1, IPRemoteDesktop: sesion?.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion?.Forwarded ?? "",
                                                                        Agent: sesion?.UserAgent ?? "");

                oResponse.error = new cError();
                oResponse.error.code = (int)cCodigosError.cDatosNoEncontrados;
                oResponse.error.message = mensaje.Message;
                oResponse.success = true;

                return oResponse;

            }
            catch (Exception)
            {
                stopwatch.Stop();
                throw;
            }
        }


        /// <summary>
        /// Devuelve las aplicaciones de Productos
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public cResponseAplicacionesProducto AplicacionesaGetList(int pageNumber,
                                                                        int pageSize)
        {
            cResponseAplicacionesProducto oResponse = new cResponseAplicacionesProducto();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                
                List<cAplicacionProducto>? lstAplicacionesProducto = aplicacionesProductoBLL.GetList();

                if (lstAplicacionesProducto?.Count > 0)
                {
                    int cantidadElementos = lstAplicacionesProducto.Count;

                    lstAplicacionesProducto = lstAplicacionesProducto?.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                    if (lstAplicacionesProducto?.Count > 0)
                    {
                        oResponse.aplicacionProductos = new List<cAplicacionesProducto>();
                        foreach (cAplicacionProducto oAplicacion in lstAplicacionesProducto)
                        {
                            oResponse.aplicacionProductos.Add(new cAplicacionesProducto(oAplicacion));
                        }
                        oResponse.success = true;
                        oResponse.paginacion = new cPaginacion(cantidadElementos,
                                                                (int)Math.Ceiling((double)cantidadElementos / pageSize),
                                                                pageNumber,
                                                                pageSize,
                                                                pageNumber);

                        stopwatch.Stop();


                        TimeSpan ts = stopwatch.Elapsed;

                        string elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                        apiLogger.LoguearSucesosAPI("Success Aplicaciones de Productos GetList",tTiposDeError.tInformativo,sesion.Usuario,"",sesion.EmpresaID,sesion.SucursalID,TiempoEjecucion:elapsedTime,cantidadDeElementos:lstAplicacionesProducto.Count,numeroPagina:pageNumber,tamanoPagina:pageSize,IPRemoteDesktop:sesion.RemoteIPAddress);

                    }
                    else
                        throw new cIntervalException(new cPaginacion(cantidadElementos,
                                                    (int)Math.Ceiling((double)cantidadElementos / pageSize),
                                                    pageNumber,
                                                    pageSize,
                                                    0));

                }
                else
                    throw new cDatosNoEncontradosException("No se encontraron Aplicaciones");

                return oResponse;
            }
            catch (SqlException)
            {
                stopwatch.Stop();
                throw;
            }
            catch (cIntervalException oIntervalo)
            {
                stopwatch.Stop();
                oResponse.paginacion = oIntervalo._paginacion;
                oResponse.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);

                return oResponse;
            }
            catch (cDatosNoEncontradosException mensaje)
            {
                stopwatch.Stop();
                oResponse.error = new cError();
                oResponse.error.code = (int)cCodigosError.cDatosNoEncontrados;
                oResponse.error.message = mensaje.Message;
                oResponse.success = true;
                return oResponse;
            }
            catch (Exception)
            {
                stopwatch.Stop();
                throw;
            }
        }

        /// <summary>
        /// Devuelve una lista de Grupo de Articulos
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public  cResponseGrupoDeArticulos GrupoArticulosGetList(int pageNumber,
                                                                        int pageSize)
        {
            cResponseGrupoDeArticulos oRespuesta = new cResponseGrupoDeArticulos();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                


                List<cGrupoDeArticulos>? lstGrupoArticulos = gruposProductosBLL.GetList();

                if (lstGrupoArticulos?.Count > 0)
                {
                    int cantidadElementos = lstGrupoArticulos.Count;

                    lstGrupoArticulos = lstGrupoArticulos?.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                    if (lstGrupoArticulos?.Count > 0)
                    {
                        oRespuesta.grupoArticulos = lstGrupoArticulos;
                        oRespuesta.success = true;
                        oRespuesta.paginacion = new cPaginacion(cantidadElementos,
                                                                (int)Math.Ceiling((double)cantidadElementos / pageSize),
                                                                pageNumber,
                                                                pageSize,
                                                                pageNumber);

                        stopwatch.Stop();


                        TimeSpan ts = stopwatch.Elapsed;

                        string elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                        apiLogger.LoguearSucesosAPI("Success Grupo de Productos GetList",tTiposDeError.tInformativo,sesion.Usuario,
                                                        Endpoint.Maestros.GrupoDeProductos("GetList"),sesion.EmpresaID,sesion.SucursalID,
                                                        cantidadDeElementos: lstGrupoArticulos.Count,
                                                        TiempoEjecucion: elapsedTime,
                                                        numeroPagina: pageNumber,
                                                        tamanoPagina: pageSize,
                                                        IPRemoteDesktop: sesion?.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion?.Forwarded ?? "",
                                                                        Agent: sesion?.UserAgent ?? "");


                    }
                    else
                        throw new cIntervalException(new cPaginacion(cantidadElementos,
                                                    (int)Math.Ceiling((double)cantidadElementos / pageSize),
                                                    pageNumber,
                                                    pageSize,
                                                    0));

                }
                else
                    throw new cDatosNoEncontradosException("No se encontraron grupos de productos");

                return oRespuesta;
            }
            catch (SqlException)
            {
                stopwatch.Stop();
                throw;
            }
            catch (cIntervalException oIntervalo)
            {
                stopwatch.Stop();
                oRespuesta.paginacion = oIntervalo._paginacion;
                oRespuesta.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);

                return oRespuesta;
            }
            catch (cDatosNoEncontradosException mensaje)
            {
                stopwatch.Stop();

                oRespuesta.error = new cError();
                oRespuesta.error.code = (int)cCodigosError.cDatosNoEncontrados;
                oRespuesta.error.message = mensaje.Message;
                oRespuesta.success = true;

                return oRespuesta;
            }
            catch (Exception)
            {
                stopwatch.Stop();
                throw;
            }
        }

        /// <summary>
        /// Devuelve un objeto GrupoArticulos
        /// </summary>
        /// <param name="grupoArtID"></param>
        /// <returns></returns>
        public cResponseGrupoDeArticulo GrupoArticulosGetItem(int grupoArtID = 0)
        {
            #region Timers
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            #endregion

            try
            {
                cResponseGrupoDeArticulo oRespuesta = new cResponseGrupoDeArticulo();
                cGrupoDeArticulos? oAlmacen = null;

                if (grupoArtID == 0)
                    throw new cCodigoNoDetalladoException("No se especifico un GrupoArticulosID");
                else
                    oAlmacen = gruposProductosBLL.GetItem(grupoArtID);

                if (oAlmacen == null)
                    throw new cDatosNoEncontradosException("No se encontro el almacen");
                else
                {
                    oRespuesta.grupoArticulos = oAlmacen;
                    oRespuesta.success = true;
                    oRespuesta.paginacion = new cPaginacion(1, 1, 1, 1, 1);

                }


                #region Timer
                stopwatch.Stop();


                TimeSpan ts = stopwatch.Elapsed;

                // Formatear e imprimir el tiempo transcurrido
                string elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                apiLogger.LoguearSucesosAPI("Success GrupoArticulos GetItem. GrupoArtID: " + grupoArtID,tTiposDeError.tInformativo,sesion.Usuario,
                                                Endpoint.Maestros.GrupoDeProductos("GetItem"),sesion.EmpresaID,sesion.SucursalID,
                                                TiempoEjecucion: elapsedTime,
                                                cantidadDeElementos: 1,
                                                numeroPagina: 1,
                                                tamanoPagina: 1,
                                                IPRemoteDesktop: sesion?.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion?.Forwarded ?? "",
                                                                        Agent: sesion?.UserAgent ?? "");

                #endregion



                return oRespuesta;
            }
            catch (SqlException)
            {
                stopwatch.Stop();
                throw;
            }
            catch (cCodigoNoDetalladoException)
            {
                stopwatch.Stop();
                throw;
            }
            catch (cDatosNoEncontradosException)
            {

                apiLogger.LoguearSucesosAPI("Not Found GrupoArticulos GetItem. GrupoArtID: " + grupoArtID, tTiposDeError.tInformativo, sesion.Usuario,
                                                Endpoint.Maestros.GrupoDeProductos("GetItem"), sesion.EmpresaID, sesion.SucursalID,
                                                cantidadDeElementos: 1,
                                                numeroPagina: 1,
                                                tamanoPagina: 1,
                                                IPRemoteDesktop: sesion?.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion?.Forwarded ?? "",
                                                                        Agent: sesion?.UserAgent ?? "");

                stopwatch.Stop();
                throw;
            }
            catch (Exception)
            {
                stopwatch.Stop();
                throw;
            }
        }

        /// <summary>
        /// Devuelve el historial de costos de un producto
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="productoID"></param>
        /// <returns></returns>
        public cResponseHistorialCostos HistorialCostosGet(int pageNumber,
                                                                   int pageSize,
                                                                   string? productoID)
        {
            #region Timers
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            #endregion

            try
            {
                cResponseHistorialCostos oRespuesta = new cResponseHistorialCostos();


                if (productoID == null || productoID.Length == 0)
                    throw new cCodigoNoDetalladoException("No se especifico un ProductoID");

                // El historial expone costos: exige permiso completo sobre costos
                if (!VerificarPermisoSobreCostos(sesion.CostosXProveedor, "S"))
                    throw new UnauthorizedAccessException("No contiene permisos para visualizar costos");

                List<cCostoHistorico>? historialCostos = costosHistoricosBLL.GetList(productoID);

                if (historialCostos?.Count > 0)
                {
                    int cantidadElementos = historialCostos.Count;

                    historialCostos = historialCostos?.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                    if (historialCostos?.Count > 0)
                    {
                        #region Paginacion
                        oRespuesta.historialCostos = historialCostos;
                        oRespuesta.success = true;
                        oRespuesta.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);
                        #endregion

                        #region Timers y Logs Informativos
                        stopwatch.Stop();


                        TimeSpan ts = stopwatch.Elapsed;

                        // Formatear e imprimir el tiempo transcurrido
                        string elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                        apiLogger.LoguearSucesosAPI("Success CostosHistoricos GetList", tTiposDeError.tInformativo, sesion.Usuario,
                                                Endpoint.Maestros.GrupoDeProductos("GetItem"), sesion.EmpresaID, sesion.SucursalID,
                                                TiempoEjecucion: elapsedTime,
                                                cantidadDeElementos: 1,
                                                numeroPagina: 1,
                                                tamanoPagina: 1,
                                                IPRemoteDesktop: sesion?.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion?.Forwarded ?? "",
                                                                        Agent: sesion?.UserAgent ?? "");

                        #endregion

                        return oRespuesta;


                    }
                    else
                        throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, 0));

                }
                else
                    throw new cDatosNoEncontradosException("No se encontraron CostosHistoricos");

            }
            catch (SqlException)
            {
                stopwatch.Stop();
                throw;
            }
            catch (cCodigoNoDetalladoException)
            {
                stopwatch.Stop();
                throw;
            }
            catch (cDatosNoEncontradosException)
            {
                apiLogger.LoguearSucesosAPI("Not Found GrupoArticulos GetItem", tTiposDeError.tInformativo, sesion.Usuario,
                                                Endpoint.Maestros.GrupoDeProductos("GetItem"), sesion.EmpresaID, sesion.SucursalID,
                                                cantidadDeElementos: 1,
                                                numeroPagina: 1,
                                                tamanoPagina: 1,
                                                IPRemoteDesktop: sesion?.RemoteIPAddress ?? "",
                                                                        IPPublica: sesion?.Forwarded ?? "",
                                                                        Agent: sesion?.UserAgent ?? "");               

                stopwatch.Stop();
                throw;
            }
            catch (Exception)
            {
                stopwatch.Stop();
                throw;
            }

        }

        /// <summary>
        /// Crea productos en lote 
        /// </summary>
        /// <param name="lstProductos"></param>
        /// <returns></returns>
        public List<cResponseProductosSave> Save(List<GESI.ERP.Core.V3.BO.cProducto> lstProductos,
                                                                                            bool blAlta)
        {
            #region Timers
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            #endregion

            List<cResponseProductosSave> oResponse = new List<cResponseProductosSave>();

            try
            {
                if (lstProductos == null || lstProductos.Count == 0)
                    throw new cCodigoNoDetalladoException("No se encontraron productos a crear");

                // NUEVO respecto de la V2: la V2 usaba ArgumentOutOfRangeException con el texto como nombre de parámetro
                // y el cliente recibía "Error interno en la aplicacion. Mensaje: Specified argument was out of the range...".
                // Ahora sale el mensaje limpio (mismo formato de respuesta: 200 con el error en el único ítem)
                if (lstProductos.Count > 10)
                    throw new cCodigoNoDetalladoException("Se permite un maximo de 10 productos por llamada");

                lstProductos.ForEach(x => x.EmpresaID = (short)(sesion?.EmpresaID ?? 0));
                
                string strMensajes = "";

                bool FueImportado = false;
                int i = 1;
                foreach (GESI.ERP.Core.V3.BO.cProducto oProductoACrear in lstProductos)
                {
                    FueImportado = false;
                    cResponseProductosSave oRespuesta = new cResponseProductosSave();
                    try
                    {
                        (GESI.ERP.Core.V3.BO.cProducto? oProductoCreado, FueImportado) = productosBLL.Save(oProductoACrear,
                                                                                                                    blAlta,
                                                                                                                    out strMensajes);

                        oRespuesta = new cResponseProductosSave(FueImportado, oProductoACrear.ProductoID,
                                                                                        oProductoACrear.Descripcion, "",
                                                                                        oProductoACrear.Estado,
                                                                                        oProductoACrear.AlicuotaIVA, i);



                    }
                    catch (SqlException sex)
                    {
                        oRespuesta = new cResponseProductosSave(FueImportado, oProductoACrear.ProductoID,
                                                                                        oProductoACrear.Descripcion,
                                                                                        sex.Message,
                                                                                        oProductoACrear.Estado,
                                                                                        oProductoACrear.AlicuotaIVA, i);

                        oRespuesta.error = APIHelper.GenerarMensajePersonalizadoPorErrorExcepcion(sex);

                    }
                    catch (Exception ex)
                    {
                        oRespuesta = new cResponseProductosSave(FueImportado, oProductoACrear.ProductoID,
                                                                                        oProductoACrear.Descripcion,
                                                                                        ex.Message,
                                                                                        oProductoACrear.Estado,
                                                                                        oProductoACrear.AlicuotaIVA, i);

                        oRespuesta.error = APIHelper.GenerarMensajePersonalizadoPorErrorExcepcion(ex);

                    }
                    i++;
                    oResponse.Add(oRespuesta);
                }

                return oResponse;

            }
            catch (cCodigoNoDetalladoException cex)
            {
                cResponseProductosSave oRespuesta = new cResponseProductosSave(false, "", "", cex.Message, "", 0, 0);
                oRespuesta.error = APIHelper.GenerarMensajePersonalizadoPorErrorExcepcion(cex);
                oResponse.Add(oRespuesta);
                stopwatch.Stop();
                return oResponse;
            }
        }



        /// <summary>
        /// Devuelve las configuraciones de productos
        /// </summary>
        /// <param name="seccionID"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public cConfiguraciones GetConfiguraciones(string seccionID = "", string itemID = "")
        {
            cConfiguraciones oConfiguraciones = new cConfiguraciones();
            
            try
            {
                List<cConfiguracionBase>? lstConfiguraciones = configuracionesBLL.GetList("Productos", seccionID, itemID);

                List<cConfiguracionBase>? lstConfiguracionesNoEncontradas = new List<cConfiguracionBase>();

                if (lstConfiguraciones?.Count > 0)
                    lstConfiguraciones = lstConfiguraciones.Where(x => ConfigItemsProductos.Preferencias.Contains(x.ItemID)).ToList();

                if (lstConfiguraciones?.Count > 0)
                    lstConfiguracionesNoEncontradas = lstConfiguraciones.Where(x => !ConfigItemsProductos.Preferencias.Contains(x.ItemID)).ToList();

                if (lstConfiguraciones?.Count > 0)
                {
                    cConfiguracionBase? oConfiguracion = lstConfiguraciones.FirstOrDefault(x => x.ItemID.Equals("DescripcionFiltro1Plural"));

                    if (oConfiguracion == null)
                    {
                        oConfiguracion = new cConfiguracionBase();
                        oConfiguracion.GrupoID = "Productos";
                        oConfiguracion.SeccionID = "Preferencias";
                        oConfiguracion.ItemID = "DescripcionFiltro1Plural";
                        oConfiguracion.Valor = "Marcas";

                        lstConfiguraciones.Add(oConfiguracion);

                        oConfiguracion = new cConfiguracionBase();
                        oConfiguracion.GrupoID = "Productos";
                        oConfiguracion.SeccionID = "Preferencias";
                        oConfiguracion.ItemID = "DescripcionFiltro1Singular";
                        oConfiguracion.Valor = "Marca";

                        lstConfiguraciones.Add(oConfiguracion);

                    }

                    oConfiguracion = lstConfiguraciones.FirstOrDefault(x => x.ItemID.Equals("DescripcionFiltro2Plural"));

                    // NUEVO respecto de la V2: la V2 acá volvía a agregar los ítems del Filtro1 ("Marcas"/"Marca") por un copy-paste,
                    // así que la respuesta salía con el Filtro1 duplicado y sin el Filtro2. Ahora agrega los del Filtro2.
                    if (oConfiguracion == null)
                    {
                        oConfiguracion = new cConfiguracionBase();
                        oConfiguracion.GrupoID = "Productos";
                        oConfiguracion.SeccionID = "Preferencias";
                        oConfiguracion.ItemID = "DescripcionFiltro2Plural";
                        oConfiguracion.Valor = "Filtros de articulos 2";   // valor por defecto (el ERP no define uno)

                        lstConfiguraciones.Add(oConfiguracion);

                        oConfiguracion = new cConfiguracionBase();
                        oConfiguracion.GrupoID = "Productos";
                        oConfiguracion.SeccionID = "Preferencias";
                        oConfiguracion.ItemID = "DescripcionFiltro2Singular";
                        oConfiguracion.Valor = "Filtro de articulos 2";

                        lstConfiguraciones.Add(oConfiguracion);

                    }


                    oConfiguraciones.success = true;
                    oConfiguraciones.grupo = "productos";
                    oConfiguraciones.secciones = lstConfiguraciones
                                                    .GroupBy(x => x.SeccionID)
                                                    .ToDictionary(
                                                        g => g.Key.ToLower(), // opcional normalizar
                                                        g => g.Select(x => new SeccionItem(x)).ToList()
                                                    );
                }

                return oConfiguraciones;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Actualiza los costos del proveedor
        /// </summary>
        /// <param name="lstCostos"></param>
        /// <returns></returns>
        /// <exception cref="cCodigoNoDetalladoException"></exception>
        public  List<dActualizacionCostoProductoProveedorResponse> ActualizacionCostosPorProveedor(List<cActualizacionCosto>? lstCostos,
                                                                                                            bool blnAgregarFaltantes)
        {

            if (lstCostos == null ||
                lstCostos.Count == 0)
                throw new cCodigoNoDetalladoException("No se especificaron costos de proveedor a actualizar");

            List<dActualizacionCostoProductoProveedorResponse> oRespuesta =
                new List<dActualizacionCostoProductoProveedorResponse>();

            // Igual que la V2: una sola actualización de costos a la vez (bloqueo compartido por todos los requests del proceso)
            lock (_lockActualizacionCostos)
            {
                foreach (cActualizacionCosto oCostoAActualizar in lstCostos)
                {
                    List<dActualizacionCostoProductoProveedorRequest> lstCostosProv = new List<dActualizacionCostoProductoProveedorRequest>();
                    lstCostosProv = ConvertirUpdateACostoProductoProveedor(oCostoAActualizar.costos ?? new List<cUpdateCostoProducto>());
                    lstCostosProv.ForEach(x => x.ProveedorID = oCostoAActualizar.proveedorID);
                    lstCostosProv.ForEach(x => x.Fecha = DateTime.Now);

                    try
                    {
                        oRespuesta.AddRange(productosBLL.ActualizarCostosPorProveedor(oCostoAActualizar.proveedorID, lstCostosProv, blnAgregarFaltantes));
                    }
                    catch (Exception ex)
                    {
                        dActualizacionCostoProductoProveedorResponse oCostoProv = new dActualizacionCostoProductoProveedorResponse();
                        oCostoProv.message = ex.Message;
                        oRespuesta.Add(oCostoProv);
                    }
                }
            }


            #region Log
            foreach (dActualizacionCostoProductoProveedorResponse oRespuestaUnitaria in oRespuesta)
            {
                apiLogger.LoguearSucesosAPI($"Producto {oRespuestaUnitaria.ProductoID} del proveedor {oRespuestaUnitaria.ProveedorID} con costo {oRespuestaUnitaria.CostoNeto} message: {oRespuestaUnitaria.message}",
                    (oRespuestaUnitaria.success) ? tTiposDeError.tInformativo : tTiposDeError.tError,
                    sesion?.Usuario ?? "",
                    Endpoint.Maestros.Productos("Costos/Update"),
                    sesion?.EmpresaID ?? 0,
                    sesion?.SucursalID ?? 0);
            }
            #endregion


            return oRespuesta;
        }


        private static List<dActualizacionCostoProductoProveedorRequest> ConvertirUpdateACostoProductoProveedor(List<cUpdateCostoProducto> lstCostos)
        {
            List<dActualizacionCostoProductoProveedorRequest> lstCostoProducto = new List<dActualizacionCostoProductoProveedorRequest>();
            foreach (cUpdateCostoProducto oUpdateCosto in lstCostos)
            {
                dActualizacionCostoProductoProveedorRequest oCosto = new dActualizacionCostoProductoProveedorRequest();
                oCosto.ProductoID = oUpdateCosto.ProductoID;
                oCosto.ReferenciaProveedor = oUpdateCosto.ReferenciaProveedor;
                oCosto.CostoBasePrecios = oUpdateCosto.CostoBasePrecios;
                oCosto.CostoBruto = oUpdateCosto.CostoBruto;
                oCosto.DescuentoArticulo = oUpdateCosto.DescuentoArticulo;
                oCosto.DescuentoGeneral = oUpdateCosto.DescuentoGeneral;
                oCosto.DivisaID = (short)oUpdateCosto.DivisaID;
                lstCostoProducto.Add(oCosto);
            }

            return lstCostoProducto;
        }

    }
}


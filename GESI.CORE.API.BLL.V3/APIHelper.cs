using GESI.CORE.API.BO.V3;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace GESI.CORE.API.BLL.V3
{

    public interface IAPIHelper
    {
        cError LanzarExcepcion(Exception ex,
                                            string usuario,
                                            string endpoint,
                                            int empresaID,
                                            string parametros,
                                            string tiempoEjecucion = "",
                                            int cantidadElementos = 0);

        int DevolverTamanoPaginacion(int TamanoArchivoConfig,
                                                    int TamanoSolicitado);

        string DevolverCodigosSeparadosPorComa(List<string>? lstCodigos,
                                                            int pageNumber,
                                                            int pageSize);

        cError GenerarMensajePersonalizadoPorErrorExcepcion(Exception ex);
    }

    internal sealed class APIHelper(IApiLogger APILogger,IEmpresasBLL EmpresasBLL): IAPIHelper
    {
        /// <summary>
        /// Evalua el tipo de Excepcion y la cambia para que sea mas amigable al usuario
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="usuario"></param>
        /// <param name="endpoint"></param>
        /// <param name="tiempoEjecucion"></param>
        /// <param name="cantidadElementos"></param>
        /// <returns></returns>
        public  cError LanzarExcepcion(Exception ex,
                                            string usuario,
                                            string endpoint,
                                            int empresaID,
                                            string parametros,
                                            string tiempoEjecucion = "",
                                            int cantidadElementos = 0)
        {
            string errorHash = GenerateErrorHash(ex);

            cError oNuevoError = GenerarMensajePersonalizadoPorErrorExcepcion(ex);
            cEmpresa oEmpresa = EmpresasBLL.GetItem(empresaID);  // RECUPERO LA EMPRESA PARA INFORMARLA EN EL ASUNTO


            if (ex.Message.Contains("Se agotó el tiempo de espera"))
            {
                string strCuerpoCorreo = "Endpoint: " + endpoint + ". Usuario: " + usuario + "Mensaje Excepcion: " + ex.Message + " | Parametros: " + parametros;

            }
            else
                APILogger.LoguearSucesosAPI("Hash: " + errorHash + " Msg: " + ex.Message,
                                            tTiposDeError.tError,
                                            usuario,
                                            endpoint,
                                            empresaID,
                                            0,
                                            TiempoEjecucion: tiempoEjecucion,
                                            cantidadDeElementos: cantidadElementos,
                                            parametros: parametros);

            

            return oNuevoError;
        }


        /// <summary>
        /// Genera un Hash con el error que se produjo
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string GenerateErrorHash(Exception ex)
        {
            // Crear un hash basado en el mensaje de error y su StackTrace
            string errorContent = ex.Message + ex.StackTrace;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(errorContent));
                return Convert.ToBase64String(hashBytes).Substring(0, 8); // Usar los primeros 8 caracteres
            }
        }


        /// <summary>
        /// Modifica el Mensaje Obtenido de la Excepcion
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public cError GenerarMensajePersonalizadoPorErrorExcepcion(Exception ex)
        {
            cError oNuevoError = new cError();

            switch (ex) // EVALUO EL TIPO DE EXCEPCION
            {
                case SqlException:  //EXCEPCION DE TIPO SQL                   
                    if (ex.Message.Contains("conflicto con la restricción REFERENCE"))
                    {
                        oNuevoError.code = (int)cCodigosError.cErrorInternoAplicacion;
                        oNuevoError.message = "No se puede realizar la accion porque existen datos relacionados";
                        break;
                    }

                    if (ex.Message.Contains("conflicto con la restricción FOREIGN KEY"))
                    {
                        oNuevoError.code = (int)cCodigosError.cErrorInternoAplicacion;
                        oNuevoError.message = "No se puede realizar la accion porque no existe la relacion del elemento";
                        break;
                    }

                    oNuevoError.code = (int)cCodigosError.cErrorInternoAplicacion;
                    oNuevoError.message = ex.Message;

                    break;

                case cCodigoNoDetalladoException:  // EXCEPCION CUANDO NO SE ENVIA EL PARAMETRO EN LA SOLICITUD
                    oNuevoError.code = (int)cCodigosError.cCodigoDetalladoEnLaSolicitud;
                    oNuevoError.message = ex.Message;
                    break;

                case cDatosNoEncontradosException:
                    oNuevoError.code = (int)cCodigosError.cDatosNoEncontrados;
                    oNuevoError.message = ex.Message;
                    break;

                case ProtocolViolationException: // EXCEPCION CUANDO EL PROTOCOLO DE LA SOLICITUD ES INCORRECTO
                    oNuevoError.code = (int)cCodigosError.cProtocoloIncorrecto;
                    oNuevoError.message = "Protocolo Incorrecto en la solicitud";
                    break;

                case cFormatoInvalidoException:
                    oNuevoError.code = (int)cCodigosError.cErrorConversionDato;
                    oNuevoError.message = ex.Message;
                    break;


                default:   // ERROR GENERICO DE LA API EN CASO QUE NO SE ESPECIFIQUE EL TIPO DE EXCEPTION

                    oNuevoError.code = (int)cCodigosError.cErrorInternoAplicacion;
                    oNuevoError.message = "Error interno en la aplicacion. Mensaje: " + ex.Message;

                    break;

            }

            return oNuevoError;

        }

        /// <summary>
        /// Evalua la cantidad de elementos que pueden venir en una paginacion
        /// </summary>
        /// <param name="TamanoArchivoConfig"></param>
        /// <param name="TamanoSolicitado"></param>
        /// <returns></returns>
        public int DevolverTamanoPaginacion(int TamanoArchivoConfig,
                                                    int TamanoSolicitado)
        {
            int intCantidadMaximaPaginado = 1000;
            int pageSize = TamanoSolicitado;

            if (TamanoSolicitado > TamanoArchivoConfig)
            {
                if (TamanoSolicitado >= intCantidadMaximaPaginado)
                    pageSize = intCantidadMaximaPaginado;
                else
                    pageSize = TamanoArchivoConfig;
            }

            return pageSize;
        }

        private readonly Lazy<string?> RutaSecrets = new(() =>
        {
            try
            {
                return new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build()
                    .GetSection("AppSettings")["RutaSecrets"];
            }
            catch
            {
                return null;   // usa la ruta por defecto de UserSecretGet
            }
        });

        /// <summary>
        /// Devuelve un string de codigos separados por coma
        /// </summary>
        /// <param name="lstCodigos"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public string DevolverCodigosSeparadosPorComa(List<string>? lstCodigos,
                                                            int pageNumber,
                                                            int pageSize)
        {
            string resultado = "";
            if (lstCodigos?.Count > 0)
            {
                lstCodigos = lstCodigos.Where(x => x != "").ToList();
                string commaSeparatedIds = string.Join(",", lstCodigos);

                List<string> splits = commaSeparatedIds.Split(',').ToList();

                List<string> nuevosplit = splits.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (nuevosplit.Count > 0)
                    resultado = string.Join(",", nuevosplit);
                else
                    throw new cIntervalException(new cPaginacion(lstCodigos.Count,
                                                (int)Math.Ceiling((double)lstCodigos.Count / pageSize),
                                                pageNumber,
                                                pageSize,
                                                0));

            }

            return resultado;
        }


       
    }

    /// <summary>
    /// Constantes de Tipos de Error Informativo / Error
    /// </summary>
    public static class tTiposDeError
    {
        public const string tInformativo = "I";
        public const string tError = "E";
    }

    /// <summary>
    /// Tabla con los codigos de error
    /// </summary>
    public enum cCodigosError
    {
        cUsuarioIncorrecto = 4011,
        cTokenInvalido = 4012,
        cTokenNoEncontrado = 4013,
        cRefreshTokenInvalido = 4014,
        cSintaxisIncorrecta = 4001,
        cErrorConversionDato = 4002,
        cErrorInternoAlDevolverToken = 5001,
        cErrorInternoAplicacion = 5002,
        cModeloNoValido = 4151,
        cPermisoDenegadoCostos = 4017,
        cCodigoDetalladoEnLaSolicitud = 4041,
        cProtocoloIncorrecto = 5555,
        cURLNoPermitida = 4018,
        cEncabezadoNoEncontrado = 4020,
        cSessionMgrError = 6500,
        cPaginacionExcedida = 7000,
        cDatosNoEncontrados = 4025

    }

    public static class ApiErrors
    {
        public const string errRateLimitExceeded = "RATE_LIMIT_EXCEEDED";
        public const string errInvalidToken = "INVALID_TOKEN";
        public const string errClienteNotFound = "CLIENTE_NOT_FOUND";
        public const string errInternalServerError = "INTERNAL_SERVER_ERROR";
        public const string errProtocolServerError = "PROTOCOL_ERROR";
        public const string errUsuarioNoHallado = "USER_NOT_FOUND";
        public const string errPaginacionExcedida = "PAGE_OUT_OF_RANGE";
        public const string errEncabezadoNoEncontrado = "HEADER_NOT_FOUND";
        public const string errNoAutorizado = "UNAUTHORIZED";
        public const string errJsonInvalido = "INVALID_JSON";
        public const string errDatosNoEncontrados = "DATA_NOT_FOUND";
        public const string errParametroInvalido = "INVALID_PARAMETER";

    }

    /// <summary>
    /// Paginacion Valores Predeterminados
    /// </summary>
    public enum pPaginacionValoresPredeterminados
    {
        pPageNumber = 1,
        pPageSize = 10
    }

    /// <summary>
    /// Codigos de Tabla Log Cambios Tablas
    /// </summary>
    public enum LogCambios
    {
        tProductoDatosGenerales = 310,
        tPreciosProductos = 312,
        tClientesDatosGenerales = 116,
        tCostosProductos = 311,
        tImagenesProductos = 348
    }

    public static class Endpoint
    {
        private const string Api = "api";


        // ========================
        // CORE
        // ========================

        public static class CORE
        {
            private const string Base = Api + "/CORE";

            public static string Usuarios(string accion)
                => $"{Base}/Usuarios/{accion}";

        }

        // ========================
        // MAESTROS
        // ========================
        public static class Maestros
        {
            private const string Base = Api + "/Maestros";

            public static string Productos(string accion)
                => $"{Base}/Productos/{accion}";

            public static string TiposDeDocumento(string accion)
                => $"{Base}/TiposDeDocumento/{accion}";

            public static string TiposDeCliente(string accion)
                => $"{Base}/TiposDeClientes/{accion}";

            public static string Aplicaciones(string accion)
                => $"{Base}/Aplicaciones/{accion}";

            public static string HistorialCostos(string accion)
                => $"{Base}/HistorialCostos/{accion}";

            public static string EstadosDeComprobantes(string accion)
                => $"{Base}/EstadosDeComprobantes/{accion}";

            public static string Almacenes(string accion)
                => $"{Base}/Almacenes/{accion}";

            public static string FiltroArticulos1(string accion)
                => $"{Base}/FiltroArticulos1/{accion}";

            public static string FiltroArticulos2(string accion)
               => $"{Base}/FiltroArticulos2/{accion}";

            public static string FiltroArticulos3(string accion)
               => $"{Base}/FiltroArticulos3/{accion}";

            public static string CanalesDeVenta(string accion)
                => $"{Base}/CanalesDeVentas/{accion}";

            public static string AlmacenesDestinoTransferencia(string accion)
                => $"{Base}/AlmacenesDestinoTransferencia/{accion}";

            public static string Repartidores(string accion)
                => $"{Base}/Repartidores/{accion}";

            public static string TiposDeOperacion(string accion)
                => $"{Base}/TiposDeOperacion/{accion}";

            public static string Zonas(string accion)
                => $"{Base}/Zonas/{accion}";

            public static string ReferenciasContables(string accion)
                => $"{Base}/ReferenciasContables/{accion}";

            public static string Paises(string accion)
                => $"{Base}/Paises/{accion}";

            public static string PuntosDeVenta(string accion)
                => $"{Base}/PuntosDeVenta/{accion}";


            public static string CategoriasComision(string accion)
                => $"{Base}/CategoriasComision/{accion}";

            public static string Agentes(string accion)
                => $"{Base}/Agentes/{accion}";

            public static string CriterioLogistica(string accion)
                => $"{Base}/CriterioLogistica/{accion}";

            public static string Empresas(string accion)
                => $"{Base}/Empresas/{accion}";

            public static string Comprobantes(string accion)
                 => $"{Base}/Comprobantes/{accion}";

            public static string Divisas(string accion)
                => $"{Base}/Divisas/{accion}";

            public static string MediosDePago(string accion)
                => $"{Base}/MediosDePago/{accion}";
            public static string GrupoDeProductos(string accion)
                => $"{Base}/GrupoDeProductos/{accion}";

            public static string Configuraciones(string accion)
                => $"{Base}/Configuraciones/{accion}";
            public static string Categorias(string accion)
                => $"{Base}/Categorias/{accion}";

            public static string Clientes(string accion)
                => $"{Base}/Clientes/{accion}";

            public static string Proveedores(string accion)
                => $"{Base}/Proveedores/{accion}";

            public static string Rubros(string accion)
                => $"{Base}/Rubros/{accion}";

            public static string SubRubros(string accion)
                => $"{Base}/SubRubros/{accion}";

            public static string SubSubRubros(string accion)
                => $"{Base}/SubSubRubros/{accion}";

            public static string Vendedores(string accion)
                => $"{Base}/Vendedores/{accion}";

            public static string Cobradores(string accion)
                => $"{Base}/Cobradores/{accion}";

            public static string FormasDePago(string accion)
                => $"{Base}/FormasDePago/{accion}";

            public static string FormasDePagoProveedores(string accion)
                => $"{Base}/FormasDePagoProveedores/{accion}";

            public static string CajasBancos(string accion)
                => $"{Base}/CajasBancos/{accion}";


        }


        public static class Contabilidad
        {
            private const string Base = Api + "/Maestros/Contabilidad";

            public static string Ejercicios(string accion)
                 => $"{Base}/Ejercicios/{accion}";

            public static string Cuentas(string accion)
                 => $"{Base}/Cuentas/{accion}";

            public static string Subdiarios(string accion)
                 => $"{Base}/Subdiarios/{accion}";

            public static string Asientos(string accion)
                 => $"{Base}/Asientos/{accion}";
        }


        // ========================
        // AUTENTICACION
        // ========================
        public static class Auth
        {
            private const string Base = Api + "/Autenticacion";

            public static string Login => $"{Base}/Login";
            public static string RefreshToken => $"{Base}/RefreshToken";
        }

        // ========================
        // VENTAS
        // ========================
        public static class Ventas
        {
            private const string Base = Api + "/Ventas";
            public static class Comprobantes
            {

                private const string Path = Base + "/Comprobantes";

                public static string Pedidos(string accion)
                    => $"{Path}/Pedidos/{accion}";

                public static string Cobros(string accion)
                    => $"{Path}/Cobros/{accion}";

                public static string Remitos(string accion)
                    => $"{Path}/Remitos/{accion}";

                public static string Facturas(string accion)
                    => $"{Path}/Facturas/{accion}";

                public static string Presupuestos(string accion)
                    => $"{Path}/Presupuestos/{accion}";

                public static string Acopios(string accion)
                    => $"{Path}/Acopios/{accion}";

                public static string Pendientes()
                    => $"{Path}/GetComprobantesPendientes";

                public static string Devolucion(string accion)
                    => $"{Path}/Devolucion/{accion}";

                public static string DevolucionRemitos(string accion)
                    => $"{Path}/DevolucionesDeRemito/{accion}";

                public static string NotasDeCredito(string accion)
                    => $"{Path}/NotasDeCredito/{accion}";

                public static string NotasDeDebito(string accion)
                    => $"{Path}/NotasDeDebito/{accion}";

            }
        }

        // ========================
        // STOCK
        // ========================
        public static class Stock
        {
            private const string Base = Api + "/Stock/Comprobantes";

            public static string Ingresos(string accion)
                => $"{Base}/Ingresos/{accion}";

            public static string Ajustes(string accion)
                => $"{Base}/Ingresos/{accion}";

            public static string Egresos(string accion)
                => $"{Base}/Egresos/{accion}";

            public static string Transferencias(string accion)
                => $"{Base}/Transferencias/{accion}";

            public static string TomaDeInventario(string accion)
                => $"{Base}/TomaDeInventario/{accion}";

            public static string Configuraciones(string accion)
               => $"{Base}/Configuraciones/{accion}";
        }

        // ========================
        // COMPRAS
        // ========================
        public static class Compras
        {
            private const string Base = Api + "/Compras";

            public static string OrdenesDeCompra(string accion)
                => $"{Base}/OrdenesDeCompra/{accion}";

            public static string OrdenesDePago(string accion)
                => $"{Base}/OrdenesDePago/{accion}";

            public static string NotasDeCredito(string accion)
                    => $"{Base}/NotasDeCredito/{accion}";

            public static string NotasDeDebito(string accion)
                => $"{Base}/NotasDeDebito/{accion}";

            public static string Facturas(string accion)
               => $"{Base}/Facturas/{accion}";
        }

        public static class Repartos
        {
            private const string Base = Api + "/Ventas";

            private const string Path = Base + "/Repartos";

            public static string GetFoja => $"{Path}/GetFoja";
            public static string GetProducto => $"{Path}/GetProducto";
            public static string CompararProductos => $"{Path}/CompararProductos";
            public static string CerrarFoja => $"{Path}/CerrarFoja";
            public static string Create => $"{Path}/Create";
            public static string GetList => $"{Path}/GetList";
            public static string GetComprobantesNoAsignados => $"{Path}/GetComprobantesNoAsignados";
        }
    }

    public static class ConfigItemsProductos
    {
        public static readonly List<string> Preferencias = new()
    {
        "Descripción Rubro (plural)",
        "Descripción Rubro (singular)",
        "DescripcionFiltro1Plural",
        "DescripcionFiltro1Singular",
        "DescripcionFiltro2Plural",
        "DescripcionFiltro2Singular"
    };
    }
}

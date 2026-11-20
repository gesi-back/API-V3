using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Contable.V3.BO;
using GESI.ERP.Core.V3.BO;
using GESI.Contabilidad.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Contabilidad
{
    /// <summary>
    /// Respuesta con ejercicios contables
    /// </summary>
    public class cResponseEjercicios : cResponse
    {

        
        /// <summary>
        /// Listado de ejercicios
        /// </summary>
        public List<cEjercicio>? ejercicios { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Resultado de grabar un ejercicio (Create / Update devuelven una lista de estos)
    /// </summary>
    public class cResponseEjerciciosSave
    {
        /// <summary>Ejercicio grabado (o el recibido, si hubo error)</summary>
        public cEjercicio? ejercicio { get; set; }

        /// <summary>Indica si se grabo</summary>
        public bool success { get; set; }

        /// <summary>Mensaje de error</summary>
        public string? mensaje { get; set; }

        /// <summary>Constructor vacio</summary>
        public cResponseEjerciciosSave() { }

        /// <summary>Constructor</summary>
        public cResponseEjerciciosSave(cEjercicio? ejercicio, bool success, string? mensaje)
        {
            this.ejercicio = ejercicio;
            this.success = success;
            this.mensaje = mensaje;
        }
    }

    /// <summary>
    /// Respuesta con subdiarios
    /// </summary>
    public class cResponseSubdiarios : cResponse
    {
        /// <summary>
        /// Listado de subdiarios
        /// </summary>
        public List<cSubdiario>? subdiarios { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Resultado de grabar un subdiario (Create / Update devuelven una lista de estos)
    /// </summary>
    public class cResponseSubdiarioSave
    {
        /// <summary>Subdiario grabado (o el recibido, si hubo error)</summary>
        public cSubdiario? subdiario { get; set; }

        /// <summary>Indica si se grabo</summary>
        public bool success { get; set; }

        /// <summary>Mensaje de error</summary>
        public string? mensaje { get; set; }

        /// <summary>Constructor vacio</summary>
        public cResponseSubdiarioSave() { }

        /// <summary>Constructor</summary>
        public cResponseSubdiarioSave(cSubdiario? subdiario, bool success, string? mensaje)
        {
            this.subdiario = subdiario;
            this.success = success;
            this.mensaje = mensaje;
        }
    }

    /// <summary>
    /// Respuesta con cuentas contables
    /// </summary>
    public class cResponseCuentas : cResponse
    {
        /// <summary>
        /// Listado de cuentas
        /// </summary>
        public List<cCuenta>? cuentas { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con asientos
    /// </summary>
    public class cResponseAsientos : cResponse
    {
        /// <summary>
        /// Listado de asientos
        /// </summary>
        public List<cAsiento>? asientos { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta del Create / Update de una cuenta contable (mismo JSON que la V2)
    /// </summary>
    public class cResponseCuenta : cResponse
    {
        /// <summary>
        /// Cuenta grabada
        /// </summary>
        public cCuenta? cuenta { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }

        /// <summary>
        /// Motivo cuando el Core rechaza la grabación
        /// </summary>
        public string? message { get; set; }
    }

    /// <summary>
    /// Respuesta del Create de un asiento (mismo JSON que la V2)
    /// </summary>
    public class cResponseSaveAsiento : cResponse
    {
        /// <summary>
        /// Asiento grabado (igual que la V2, no se completa)
        /// </summary>
        public cAsiento? asiento { get; set; }

        /// <summary>
        /// "OK" si se grabó, o el motivo cuando el Core lo rechaza
        /// </summary>
        public string? message { get; set; }
    }

    public class cResponseLibroDiario: cResponse
    {
        /// <summary>
        /// Libro diarios
        /// </summary>
        public cLibroDiario? librodiario { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    public class cResponseMayores :cResponse
    {
        /// <summary>
        /// Mayor de la cuenta
        /// </summary>
        public List<cMayorCuenta>? mayores { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con el balance de sumas y saldos
    /// </summary>
    public class cResponseBalanceDeSumasYSaldos : cResponse
    {
        /// <summary>
        /// Balance de sumas y saldos
        /// </summary>
        public cBalanceDeSumasYSaldos? balanceDeSumasYSaldos { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con el balance general
    /// </summary>
    public class cResponseBalanceGeneral : cResponse
    {
        /// <summary>
        /// Balance general
        /// </summary>
        public cBalanceGeneral? balanceGeneral { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }
}

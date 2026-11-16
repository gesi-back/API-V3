using GESI.CORE.API.BO.V3.API;
using GESI.ERP.Core.V3.BO;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Respuesta con tipos de operacion
    /// </summary>
    public class cResponseTiposDeOperacion : cResponse, IRespuestaPaginada
    {
        /// <summary>
        /// Listado de tipos de operacion
        /// </summary>
        public List<cTipoDeOperacion>? tiposDeOperacion { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Respuesta con un tipo de operacion
    /// </summary>
    public class cResponseTipoDeOperacion : cResponse
    {
        /// <summary>
        /// Objeto de tipo de operacion
        /// </summary>
        public cTipoDeOperacion? tipoOperacion { get; set; }

        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        public cPaginacion? paginacion { get; set; }
    }

    /// <summary>
    /// Resultado de grabar un tipo de operacion (un ítem por cada tipo de operacion enviado)
    /// </summary>
    public class cResponseTipoDeOperacionSave
    {
        /// <summary>
        /// Tipo de operacion grabado (o el enviado, si falló)
        /// </summary>
        public cTipoDeOperacion? tipoOperacion { get; set; }

        /// <summary>
        /// Indica si se grabó correctamente
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// Mensaje de error o advertencia
        /// </summary>
        public string? mensaje { get; set; }

        /// <summary>
        /// Constructor vacio
        /// </summary>
        public cResponseTipoDeOperacionSave() { }

        /// <summary>
        /// Constructor con parametros
        /// </summary>
        public cResponseTipoDeOperacionSave(cTipoDeOperacion? tipoOperacion, bool success, string? mensaje)
        {
            this.tipoOperacion = tipoOperacion;
            this.success = success;
            this.mensaje = mensaje;
        }
    }
}

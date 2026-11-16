namespace GESI.CORE.API.BO.V3.API
{
    /// <summary>
    /// Respuesta que incluye paginación. Permite que el paginador común de la BLL
    /// cargue <see cref="paginacion"/> sin conocer el tipo concreto de la respuesta.
    /// No cambia el JSON: la propiedad ya existe en cada respuesta.
    /// </summary>
    public interface IRespuestaPaginada
    {
        /// <summary>
        /// Paginacion del resultado
        /// </summary>
        cPaginacion? paginacion { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GESI.CORE.API.BO.V3.API
{
    /// <summary>
    /// Excepcion cuando excede el numero de pagina
    /// </summary>
    public class cIntervalException : ApplicationException
    {
        /// <summary>
        /// Objeto de paginacion
        /// </summary>
        public cPaginacion _paginacion;

        /// <summary>
        /// Constructor original 
        /// </summary>
        /// <param name="paginacion"></param>
        public cIntervalException(cPaginacion paginacion)
        {
            _paginacion = paginacion;
        }
    }

    /// <summary>
    /// Excepcion generada al no recibir los parametros necesarios
    /// </summary>
    public class cCodigoNoDetalladoException : Exception     // NO SE PASARON DATOS POR PARAMETRO
    {        
        /// <summary>
        /// Constructor original 
        /// </summary>
        /// <param name="mensaje"></param>
        public cCodigoNoDetalladoException(string mensaje) : base(mensaje) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public cCodigoNoDetalladoException() { }
    }

    /// <summary>
    /// Excepcion generada al no encontrar los datos especificados
    /// </summary>
    
    public class cDatosNoEncontradosException : Exception   // NO SE DEVOLVIO NINGUN REGISTRO EN LA RESPUESTA
    {
        /// <summary>
        /// Paginacion
        /// </summary>
        public cPaginacion? Paginacion { get; private set; }

        /// <summary>
        /// Constructor sin parametros
        /// </summary>
        public cDatosNoEncontradosException() { }

        /// <summary>
        /// Constructor original
        /// </summary>
        /// <param name="mensaje"></param>
        public cDatosNoEncontradosException(string mensaje) : base(mensaje) { }

        /// <summary>
        /// Constructor que acepta el mensaje y un objeto cPaginacion
        /// </summary>
        /// <param name="mensaje"></param>
        /// <param name="oPaginacion"></param>
        public cDatosNoEncontradosException(string mensaje, cPaginacion oPaginacion)
            : base(mensaje)
        {
            this.Paginacion = oPaginacion;  // Almacena el objeto cPaginacion
        }

        /// <summary>
        /// Constructor que solo acepta el objeto cPaginacion
        /// </summary>
        /// <param name="oPaginacion"></param>
        public cDatosNoEncontradosException(cPaginacion oPaginacion)
        {
            this.Paginacion = oPaginacion;  // Almacena el objeto cPaginacion
        }

    }

    /// <summary>
    /// Excepcion generada al convertir formatos (FECHAS)
    /// </summary>
    public class cFormatoInvalidoException : Exception       // FORMATO INVALIDO (FECHA / CONVERSION)
    {
        /// <summary>
        /// Constructor con mensaje personalizado
        /// </summary>
        /// <param name="mensaje"></param>
        public cFormatoInvalidoException(string mensaje) : base(mensaje) { }

        /// <summary>
        /// Constructor original
        /// </summary>
        public cFormatoInvalidoException() { } 
    }

    /// <summary>
    /// Excepcion generada al llenar el session manager
    /// </summary>
    public class  cSessionManagerException : Exception     // ERROR AL LLENAR EL SESSION MANAGER
    {
        
    }
}

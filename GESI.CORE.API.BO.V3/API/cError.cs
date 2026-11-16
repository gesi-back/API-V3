using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GESI.CORE.API.BO.V3.API
{
    /// <summary>
    /// Objeto error de la API
    /// </summary>
    public class cError
    {
        private int _code;
        private string? error;
        private string? _message;

        /// <summary>
        /// Codigo de error
        /// </summary>
        public int code { get => _code; set => _code = value; }
        /// <summary>
        /// Mmensaje de error / advertencia
        /// </summary>
        public string? message { get => _message; set => _message = value; }

        /// <summary>
        /// Codigo de error string
        /// </summary>
        public string? Error { get => error; set => error = value; }

        /// <summary>
        /// Constructor
        /// </summary>
        public cError() { }

        /// <summary>
        /// Constructor con parametros
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="error"></param>
        public cError(int code, string? message,string? error) { 
            this.code = code;
            this.message = message;
            this.Error = error;
        }

        /// <summary>
        /// Constructor original
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public cError(int code, string? message)
        {
            this.code = code;
            this.message = message;
            this.Error = "";
        }

    }



}

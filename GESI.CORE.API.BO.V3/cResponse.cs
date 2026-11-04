using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GESI.CORE.API.BO.V3
{
    /// <summary>
    /// Objeto respuesta basico
    /// </summary>
    public class cResponse
    {
        private bool _success;
        private cError? _error;

        /// <summary>
        /// Determina si es exitoso
        /// </summary>

        public bool success { get => _success; set => _success = value; }
        /// <summary>
        /// Objeto de tipo error
        /// </summary>
        public cError? error { get => _error; set => _error = value; }
    }
}

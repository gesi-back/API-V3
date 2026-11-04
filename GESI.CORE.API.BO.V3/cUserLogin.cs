using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GESI.CORE.API.BO.V3
{
    /// <summary>
    /// Datos para loguear con usuario y contraseña
    /// </summary>
    public class cUserLogin
    {
        /// <summary>
        /// Nombre de usuario del Verscom
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Contraseña del usuario del verscom
        /// </summary>
        public string? Password { get; set; }
    }
}

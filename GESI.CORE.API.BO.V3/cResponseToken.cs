using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GESI.CORE.API.BO.V3
{
    /// <summary>
    /// Clase que devuelve el token solicitado
    /// </summary>
    public class cResponseToken : cResponse
    {
        private string? _token;
        private cPaginacion? _paginacion;
        private string? _refresh_token;
        private string? _expiracion;

        /// <summary>
        /// token para consumir en la API
        /// </summary>
        public string? token { get => _token; set => _token = value; }

        /// <summary>
        /// refresh token para solicitar un nuevo acceso a la API
        /// </summary>

        public string? refresh_token { get => _refresh_token; set => _refresh_token = value; }

        /// <summary>
        /// Paginacion de la respuesta
        /// </summary>
        public cPaginacion? paginacion { get => _paginacion; set => _paginacion = value; }
        /// <summary>
        /// Fecha y hora en la que expira el token
        /// </summary>
        public string? expiracion { get => _expiracion; set => _expiracion = value; }
    }
}

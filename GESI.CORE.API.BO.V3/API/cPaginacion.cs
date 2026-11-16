using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GESI.CORE.API.BO.V3.API
{
    /// <summary>
    /// Determina la informacion de la pagina de resultados
    /// </summary>
    public class cPaginacion
    {
        private int _TotalElementos;
        private int _TotalPaginas;
        private int _PaginaActual;
        private int _TamañoPagina;
        private int _PaginaSolicitada;

        /// <summary>
        /// Cantidad total de elementos encontrados
        /// </summary>
        public int totalElementos { get => _TotalElementos; set => _TotalElementos = value; }

        /// <summary>
        /// Total de paginas de resultado
        /// </summary>
        public int totalPaginas { get => _TotalPaginas; set => _TotalPaginas = value; }
        /// <summary>
        /// Pagina actual
        /// </summary>
        public int paginaActual { get => _PaginaActual; set => _PaginaActual = value; }
        /// <summary>
        /// Cantidad de elementos por pagina
        /// </summary>
        public int tamañoPagina { get => _TamañoPagina; set => _TamañoPagina = value; }
        /// <summary>
        /// Pagina solicitada por el usuario
        /// </summary>
        public int PaginaSolicitada { get => _PaginaSolicitada; set => _PaginaSolicitada = value; }

        /// <summary>
        /// Constructor original
        /// </summary>
        public cPaginacion() { }

        /// <summary>
        /// Constructor con parametros
        /// </summary>
        /// <param name="_totalElementos"></param>
        /// <param name="_totalPaginas"></param>
        /// <param name="_PaginaSolicitada"></param>
        /// <param name="_TamanoPagina"></param>
        /// <param name="_paginaActual"></param>
        public cPaginacion(int _totalElementos, int _totalPaginas, int _PaginaSolicitada, int _TamanoPagina, int _paginaActual)
        {
            this._TotalElementos = _totalElementos;
            this._TamañoPagina = _TamanoPagina;
            this._PaginaSolicitada = _PaginaSolicitada;
            this._TotalPaginas = _totalPaginas;
            this._PaginaActual = _paginaActual;

        }
        

    }

    
}

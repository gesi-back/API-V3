using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GESI.CORE.API.BO.V3
{
    /// <summary>
    /// Define el tipo de error generado
    /// </summary>
    public class cTipoDeError
    {
        private int _CodigoError;
        private string? _DescripcionError;
        private int _CodigoStatus;
        private string? _TipoErrorAdvertencia;

        /// <summary>
        /// Codigo de error
        /// </summary>
        public int CodigoError { get => _CodigoError; set => _CodigoError = value; }
        /// <summary>
        /// Descripcion del error
        /// </summary>
        public string? DescripcionError { get => _DescripcionError; set => _DescripcionError = value; }

        /// <summary>
        /// Codigo de estado devuelto por API
        /// </summary>
        public int CodigoStatus { get => _CodigoStatus; set => _CodigoStatus = value; }

        /// <summary>
        /// Nivel de gravedad (Error / advertencia)
        /// </summary>
        
        public string? TipoErrorAdvertencia { get => _TipoErrorAdvertencia; set => _TipoErrorAdvertencia = value; }

        /// <summary>
        /// Constructor con parametros
        /// </summary>
        /// <param name="_codigo"></param>
        /// <param name="_mensaje"></param>
        /// <param name="codigoStatus"></param>
        /// <param name="_tipo"></param>
        public cTipoDeError(int _codigo, string? _mensaje, int codigoStatus, string? _tipo)
        {
            CodigoError = _codigo;
            DescripcionError = _mensaje;
            CodigoStatus = codigoStatus;
            TipoErrorAdvertencia = _tipo;
        }

        /// <summary>
        /// Constructor original
        /// </summary>
        public cTipoDeError()
        { }

    }
}

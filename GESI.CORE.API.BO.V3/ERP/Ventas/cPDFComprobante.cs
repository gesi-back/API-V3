using GESI.CORE.API.BO.V3.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace GESI.CORE.API.BO.V3.ERP.Ventas
{
    /// <summary>
    /// PDF del comprobante
    /// </summary>
    public class cPDFComprobante
    {
        private string? _url;
        private cError? _error;
        private bool? _success;

        /// <summary>
        /// Objeto con error
        /// </summary>
        public cError? error { get => _error; set => _error = value; }
        /// <summary>
        /// URL a la que se subio el comprobante
        /// </summary>
        public string? url { get => _url; set => _url = value; }
        /// <summary>
        /// Define si fue exitosa la operacion
        /// </summary>
        public bool? success { get => _success; set => _success = value; }
    }
}

using GESI.CORE.API.BO.V3.API;

namespace GESI.CORE.API.BO.V3.ERP.Ventas
{
    /// <summary>
    /// Resultado por aplicación de AplicarSaldo/Save (migrado de la V2, mismo JSON): la relación grabada + success/error
    /// </summary>
    public class cResponseSaveRelaciones : GESI.ERP.Ventas.V3.BO.cRelacionComprobantes
    {
        private cError? _error;
        private bool _success;

        public cError? error { get => _error; set => _error = value; }
        public bool success { get => _success; set => _success = value; }
    }
}

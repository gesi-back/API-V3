namespace GESI.CORE.API.BLL.V3.ERP.Ventas
{
    /// <summary>
    /// Subtipos de comprobantes de venta (copiado de la V2, APIHelper.SubTiposComprobantes)
    /// </summary>
    public static class SubTiposComprobantes
    {
        public const string sFactura = "F";
        public const string sNPE = "P";
        public const string sRemito = "M";
        public const string sRecibo = "R";
        public const string sNCR = "C";
        public const string sNDB = "D";
        public const string sPresupuesto = "S";
        public const string sDEV = "V";
    }

    /// <summary>
    /// Estados de comprobantes (copiado de la V2, APIHelper.EstadosComprobantes)
    /// </summary>
    public static class EstadosComprobantes
    {
        public const string ePendiente = "P";
        public const string eCumplido = "C";
    }
}

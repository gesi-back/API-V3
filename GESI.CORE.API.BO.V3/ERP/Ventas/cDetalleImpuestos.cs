using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GESI.CORE.API.BO.V3.ERP.Ventas
{
    /// <summary>
    /// Detalle de impuestos
    /// </summary>
    public class cDetalleImpuestos : GESI.ERP.Ventas.V3.BO.cImpuestosComprobanteDeVenta
    {
        /// <summary>
        /// Codigo de empresa
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public override int EmpresaID { get => base.EmpresaID; set => base.EmpresaID  = value; }

        /// <summary>
        /// Codigo de comprobante
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public override int ComprobanteID { get => base.ComprobanteID; set => base.ComprobanteID = value; }

        /// <summary>
        /// Serie del comprobante
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public override string Serie { get => base.Serie; set => base.Serie = value; }


        /// <summary>
        /// Codigo de punto de venta
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public override int PuntoDeVentaID { get => base.PuntoDeVentaID; set => base.PuntoDeVentaID = value; }

        /// <summary>
        /// Numero del comprobante
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public override int Numero { get => base.Numero; set => base.Numero = value; }

        /// <summary>
        /// Constructor original
        /// </summary>
        public cDetalleImpuestos() { }

        /// <summary>
        /// Constructor con parametros
        /// </summary>
        /// <param name="padre"></param>

        public cDetalleImpuestos(GESI.ERP.Ventas.V3.BO.cImpuestosComprobanteDeVenta padre)
        {
            EmpresaID = padre.EmpresaID;
            ComprobanteID = padre.ComprobanteID;
            Serie = padre.Serie;
            PuntoDeVentaID = padre.PuntoDeVentaID;
            Numero =  padre.Numero;
            RetencionPercepcionID = padre.RetencionPercepcionID;
            ImpuestoID = padre.ImpuestoID;
            ImporteBase = padre.ImporteBase;
            Alicuota = padre.Alicuota;
            ImporteImpuesto = padre.ImporteImpuesto;
            ImporteAjusteImpuesto = padre.ImporteAjusteImpuesto;
            PapelDeTrabajo = padre.PapelDeTrabajo;
            NumeroDeReferencia = padre.NumeroDeReferencia;
            ItemID = padre.ItemID;
            AlicuotaID = padre.AlicuotaID;
            CalculadoAutomaticamente = padre.CalculadoAutomaticamente;

        }


    }
}

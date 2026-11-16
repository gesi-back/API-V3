using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GESI.CORE.API.BO.V3.ERP.Ventas
{
    /// <summary>
    /// Detalle de venta heredado
    /// </summary>
    public class cDetalleDeVenta : GESI.ERP.Ventas.V3.BO.cItemComprobanteDeVenta
    {
        /// <summary>
        /// Codigo de comprobanteID
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public override int ComprobanteID { get => base.ComprobanteID; set => base.ComprobanteID = value; }

        /// <summary>
        /// Serie del comprobante
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public override string? Serie { get => base.Serie; set => base.Serie = value; }

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
        /// Codigo de empresa
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public override int EmpresaID { get => base.EmpresaID; set => base.EmpresaID= value; }
       
        /// <summary>
        /// Constructor original
        /// </summary>
        public cDetalleDeVenta() { }

        /// <summary>
        /// Constructor con parametros
        /// </summary>
        /// <param name="cItem"></param>
        public cDetalleDeVenta(GESI.ERP.Ventas.V3.BO.cItemComprobanteDeVenta cItem)
        {
            TipoDeItem = cItem.TipoDeItem;
            CodigoDeItem = cItem.CodigoDeItem;
            CantidadU1 = cItem.CantidadU1;
            CantidadU2 = cItem.CantidadU2;
            CantidadU3 = cItem.CantidadU3;
            TipoDeFacturacion = cItem.TipoDeFacturacion;
            PrecioUnitarioNeto = cItem.PrecioUnitarioNeto;
            PrecioUnitarioFinal = cItem.PrecioUnitarioFinal;
            DescuentosPorcentaje = cItem.DescuentosPorcentaje;
            DescuentoImporteNeto = cItem.DescuentoImporteNeto;
            DescuentoImporteFinal = cItem.DescuentoImporteFinal;
            ImporteTotalNeto = cItem.ImporteTotalNeto;
            ImporteTotalFinal = cItem.ImporteTotalFinal;
            ItemID = cItem.ItemID;
            AlicuotaID = cItem.AlicuotaID;
            AlicuotaIVA = cItem.AlicuotaIVA;
            Comentario = cItem.Comentario;
            SaldoFactura_U1 = cItem.SaldoFactura_U1;
            SaldoFactura_U2 = cItem.SaldoFactura_U2;
            SaldoEntrega_U1 = cItem.SaldoEntrega_U1;
            SaldoEntrega_U2 = cItem.SaldoEntrega_U2;
            AgenteID = cItem.AgenteID;
            LoteID = cItem.LoteID;
            DescuentoAutorizado = cItem.DescuentoAutorizado;
            AutorizadoVenderSinExistencias = cItem.AutorizadoVenderSinExistencias;
            EsNoGravado = cItem.EsNoGravado;
            PromocionID = cItem.PromocionID;
            YaEstaba = cItem.YaEstaba;
            Serie = cItem.Serie;
            PuntoDeVentaID = cItem.PuntoDeVentaID;
            ComprobanteID = cItem.ComprobanteID;
            EmpresaID = cItem.EmpresaID;
            Numero = cItem.Numero;
            CodigoDeItemQueYaEstaba = cItem.CodigoDeItemQueYaEstaba;
            AlicuotaIDQueYaEstaba = cItem.AlicuotaIDQueYaEstaba;
            CampoExtra = cItem.CampoExtra;
            CampoExtra2 = cItem.CampoExtra2;
        }


    }
}

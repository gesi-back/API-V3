using System.Text.Json.Serialization;

namespace GESI.CORE.API.BO.V3.ERP.Maestros
{
    /// <summary>
    /// Cliente tal como lo expone la API (copia del cAPICliente de la V2): el del Core sin EmpresaID
    /// ni los datos internos marcados con [JsonIgnore]. El constructor copia los mismos campos que la V2.
    /// </summary>
    public class cAPICliente : GESI.ERP.Core.V3.BO.cCliente
    {
        /// <summary>
        /// Codigo de empresa
        /// </summary>
        [JsonIgnore]
        
        public override short EmpresaID { get => base.EmpresaID; set => base.EmpresaID = value; }

        /// <summary>
        /// Constructor heredado del producto padre
        /// </summary>
        /// <param name="padre"></param>
        public cAPICliente(GESI.ERP.Core.V3.BO.cCliente padre)
        {
            ClienteID = padre.ClienteID;
            Referencia = padre.Referencia;
            RazonSocial = padre.RazonSocial;
            Fantasia = padre.Fantasia;
            Contacto = padre.Contacto;
            Cargo = padre.Cargo;
            Domicilio = padre.Domicilio;
            Localidad = padre.Localidad;
            CP = padre.CP;
            Telefono = padre.Telefono;
            Fax = padre.Fax;
            Email = padre.Email;
            WebSite = padre.WebSite;
            TipoDeDocumentoID = padre.TipoDeDocumentoID;
            NumeroDeDocumento = padre.NumeroDeDocumento;
            CobradorID = padre.CobradorID;
            VendedorID = padre.VendedorID;
            AgrupacionID = padre.AgrupacionID;
            TipoDeClienteID = padre.TipoDeClienteID;
            CanalDeVentaID = padre.CanalDeVentaID;
            ZonaID = padre.ZonaID;
            SubZonaID = padre.SubZonaID;
            RegionID = padre.RegionID;
            ImpuestoID = padre.ImpuestoID;
            CondicionIVA = padre.CondicionIVA;
            FormaDePagoID = padre.FormaDePagoID;
            EstadoID = padre.EstadoID;
            LimiteDeCredito = padre.LimiteDeCredito;
            DtoGral_Texto = padre.DtoGral_Texto;
            DtoGral_Porcentaje = padre.DtoGral_Porcentaje;
            PorcVarEnPrecios = padre.PorcVarEnPrecios;
            FechaDeAlta = padre.FechaDeAlta;
            FechaDeBaja = padre.FechaDeBaja;
            UltimaActualizacion = padre.UltimaActualizacion;
            Notas = padre.Notas;
            PaisID = padre.PaisID;
            ImpuestoIIBBID = padre.ImpuestoIIBBID;
            CondicionIIBB = padre.CondicionIIBB;
            NumeroIIBB = padre.NumeroIIBB;
            PorcentajeExcepcionIIBB = padre.PorcentajeExcepcionIIBB;
            ObservacionesDeEntrega = padre.ObservacionesDeEntrega;
            HabilitadoCuentaCorriente = padre.HabilitadoCuentaCorriente;
            BloqueoRemitos = padre.BloqueoRemitos;
            BloqueoVentas = padre.BloqueoVentas;
            LimiteKilogramos = padre.LimiteKilogramos;
            LimiteDeCredito_Dias = padre.LimiteDeCredito_Dias;
            LiberarBloqueosHasta = padre.LiberarBloqueosHasta;
            LimiteDiasValoresPostdatados = padre.LimiteDiasValoresPostdatados;
            Facebook = padre.Facebook;
            Instagram = padre.Instagram;
            GeoCoordenadas = padre.GeoCoordenadas;
            EmpresaID = padre.EmpresaID;
            PagosResponsable = padre.PagosResponsable;
            PagosDomicilio = padre.PagosDomicilio;
            PagosHorarios = padre.PagosHorarios;
            DomicilioDeEntrega = padre.DomicilioDeEntrega;
            DtoProntoPago = padre.DtoProntoPago;
            LeyendaFacturas = padre.LeyendaFacturas;
            CodigoProveedor = padre.CodigoProveedor;
            FechaEspecial = padre.FechaEspecial;
            DatosPersonales = padre.DatosPersonales;
            DivisaID = padre.DivisaID;
            DiasDePago = padre.DiasDePago;
            SerieComprobantes = padre.SerieComprobantes;
            TipoDeDescuento = padre.TipoDeDescuento;
            FechaDeNacimiento = padre.FechaDeNacimiento;
            LugarDeNacimiento = padre.LugarDeNacimiento;
            DocumentoExpedidoPor = padre.DocumentoExpedidoPor;
            Profesion = padre.Profesion;
            EstadoCivil = padre.EstadoCivil;
            RolEnAgrupacion = padre.RolEnAgrupacion;
            NombrePadre = padre.NombrePadre;
            NombreMadre = padre.NombreMadre;
            LoteDeFacturacion = padre.LoteDeFacturacion;
            FormatoEtiquetaID = padre.FormatoEtiquetaID;
            ReferenciaContableID = padre.ReferenciaContableID;
            LeyendaParaElOperador = padre.LeyendaParaElOperador;
            PorcentajeDeComision = padre.PorcentajeDeComision;
            CanalDeVentaID2 = padre.CanalDeVentaID2;
            PorcVarEnPrecios2 = padre.PorcVarEnPrecios2;
            CuentaDescuentoPie = padre.CuentaDescuentoPie;
            FechaVencimentoInscripcion = padre.FechaVencimentoInscripcion;
            ReferenciaContableVentasID = padre.ReferenciaContableVentasID;
            GrupoID = padre.GrupoID;
            SujetoVinculado = padre.SujetoVinculado;
            CopiasPRE = padre.CopiasPRE;
            CopiasNPE = padre.CopiasNPE;
            CopiasRTO = padre.CopiasRTO;
            CopiasFAC = padre.CopiasFAC;
            CopiasNCR = padre.CopiasNCR;
            CopiasREC = padre.CopiasREC;
            CargasPorcentajeSeguro = padre.CargasPorcentajeSeguro;
            CargasPorcentajeContrareembolso = padre.CargasPorcentajeContrareembolso;
            IIBBAlicuota = padre.IIBBAlicuota;
            IIBBAlicuotaVencimiento = padre.IIBBAlicuotaVencimiento;
            TipoContribuyenteIIBBID = padre.TipoContribuyenteIIBBID;
            DiaEspecialVencimiento = padre.DiaEspecialVencimiento;
            Repartidor2ID = padre.Repartidor2ID;
            CargasSeFacturaA = padre.CargasSeFacturaA;
            SENASA_GLP = padre.SENASA_GLP;
            SENASA_COLEGIO = padre.SENASA_COLEGIO;
            PreguntarDestinatariosEmailVentas = padre.PreguntarDestinatariosEmailVentas;
            EnviarXLSComprobantes = padre.EnviarXLSComprobantes;
            SubTipoComprobanteVenta = padre.SubTipoComprobanteVenta;
            HabilitadoDescuentoPie = padre.HabilitadoDescuentoPie;
            LocalidadID = padre.LocalidadID;

            if(padre.ListaSucursaleCliente?.Count > 0)
            {
                ListaSucursaleCliente = padre.ListaSucursaleCliente;
            }

            if(padre.ListaPreciosEspeciales?.Count > 0)
            {
                ListaPreciosEspeciales = padre.ListaPreciosEspeciales;
            }

            if(padre.LstContactoClientes?.Count > 0)
            {
                LstContactoClientes = padre.LstContactoClientes;
            }

            if(padre.LstUnidadesDeClientes?.Count > 0)
            {
                LstUnidadesDeClientes = padre.LstUnidadesDeClientes;
            }

            if (padre.LstDefinicionClienteSucursal?.Count > 0)
            {
                LstDefinicionClienteSucursal = padre.LstDefinicionClienteSucursal;
            }

        }
    

       /// <summary>
       /// Constructor original
       /// </summary>
        public cAPICliente()
        { }


        /// <summary>
        /// Responsable de pagos
        /// </summary>
        [JsonIgnore]
        public override string? PagosResponsable { get => base.PagosResponsable; set => base.PagosResponsable = value; }

        /// <summary>
        /// Domicilio de pago
        /// </summary>
        [JsonIgnore]
        public override string? PagosDomicilio { get => base.PagosDomicilio; set => base.PagosDomicilio = value; }


        /// <summary>
        /// Horario de pagos
        /// </summary>
        [JsonIgnore]
        public override string? PagosHorarios { get => base.PagosHorarios; set => base.PagosHorarios = value; }

        /// <summary>
        /// Domicilio de entrega
        /// </summary>

        [JsonIgnore]
        public override string? DomicilioDeEntrega { get => base.DomicilioDeEntrega; set => base.DomicilioDeEntrega = value; }

        /// <summary>
        /// Descuento pronto pago
        /// </summary>

        [JsonIgnore]
        public override decimal? DtoProntoPago { get => base.DtoProntoPago; set => base.DtoProntoPago = value; }

        
        /// <summary>
        /// Leyenda para facturas
        /// </summary>
        [JsonIgnore]
        public override string? LeyendaFacturas { get => base.LeyendaFacturas; set => base.LeyendaFacturas = value; }

        /// <summary>
        /// Codigo de proveedor
        /// </summary>

        [JsonIgnore]
        public override string? CodigoProveedor { get => base.CodigoProveedor; set => base.CodigoProveedor = value; }

        /// <summary>
        /// Fecha especial 
        /// </summary>

        [JsonIgnore]
        public override DateTime? FechaEspecial { get => base.FechaEspecial; set => base.FechaEspecial = value; }

        /// <summary>
        /// Datos personales 
        /// </summary>

        [JsonIgnore]
        public override string? DatosPersonales { get => base.DatosPersonales; set => base.DatosPersonales = value; }

        /// <summary>
        /// Codigo de divisa
        /// </summary>
        [JsonIgnore]
        public override short? DivisaID { get => base.DivisaID; set => base.DivisaID = value; }


        /// <summary>
        /// Dias de pago
        /// </summary>
        [JsonIgnore]
        public override string? DiasDePago { get => base.DiasDePago; set => base.DiasDePago = value; }

        
        /// <summary>
        /// Serie en comprobantes
        /// </summary>
        [JsonIgnore]
        public override string? SerieComprobantes { get => base.SerieComprobantes; set => base.SerieComprobantes = value; }

        /// <summary>
        /// Tipo de descuento
        /// </summary>
        [JsonIgnore]
        public override byte? TipoDeDescuento { get => base.TipoDeDescuento; set => base.TipoDeDescuento = value; }

        /// <summary>
        /// Fecha de nacimiento
        /// </summary>
        [JsonIgnore]
        public override DateTime? FechaDeNacimiento { get => base.FechaDeNacimiento; set => base.FechaDeNacimiento = value; }

        /// <summary>
        /// Lugar de nacimiento
        /// </summary>
        [JsonIgnore]
        public override string? LugarDeNacimiento { get => base.LugarDeNacimiento; set => base.LugarDeNacimiento = value; }

        /// <summary>
        /// Documento expedido por
        /// </summary>
        [JsonIgnore]
        public override string? DocumentoExpedidoPor { get => base.DocumentoExpedidoPor; set => base.DocumentoExpedidoPor = value; }

        /// <summary>
        /// Profesion
        /// </summary>
        [JsonIgnore]
        public override string? Profesion { get => base.Profesion; set => base.Profesion = value; }

        /// <summary>
        /// Estado civil
        /// </summary>
        [JsonIgnore]
        public override string? EstadoCivil { get => base.EstadoCivil; set => base.EstadoCivil = value; }

        /// <summary>
        /// Rol en agrupacion
        /// </summary>
        [JsonIgnore]
        public override short? RolEnAgrupacion { get => base.RolEnAgrupacion; set => base.RolEnAgrupacion = value; }

        /// <summary>
        /// Nombre del padre
        /// </summary>
        [JsonIgnore]
        public override string? NombrePadre { get => base.NombrePadre; set => base.NombrePadre = value; }

        /// <summary>
        /// Nombre de la madre
        /// </summary>
        [JsonIgnore]
        public override string? NombreMadre { get => base.NombreMadre; set => base.NombreMadre = value; }

        /// <summary>
        /// Lote de facturacion
        /// </summary>
        [JsonIgnore]
        public override string? LoteDeFacturacion { get => base.LoteDeFacturacion; set => base.LoteDeFacturacion = value; }

        /// <summary>
        /// Formato de etiquetaID
        /// </summary>
        [JsonIgnore]
        public override short? FormatoEtiquetaID { get => base.FormatoEtiquetaID; set => base.FormatoEtiquetaID = value; }

        /// <summary>
        /// Referencia contable del cliente
        /// </summary>
        [JsonIgnore]
        public override string? ReferenciaContableID { get => base.ReferenciaContableID; set => base.ReferenciaContableID = value; }

        /// <summary>
        /// Leyenda para el operador
        /// </summary>
        [JsonIgnore]
        public override string? LeyendaParaElOperador { get => base.LeyendaParaElOperador; set => base.LeyendaParaElOperador = value; }

        /// <summary>
        /// Porcentaje de comision
        /// </summary>
        [JsonIgnore]
        public override decimal? PorcentajeDeComision { get => base.PorcentajeDeComision; set => base.PorcentajeDeComision = value; }

        /// <summary>
        /// Canal de venta 2
        /// </summary>
        [JsonIgnore]
        public override short? CanalDeVentaID2 { get => base.CanalDeVentaID2; set => base.CanalDeVentaID2 = value; }

        /// <summary>
        /// Porcentaje de variacion de precios 2
        /// </summary>
        [JsonIgnore]
        public override string? PorcVarEnPrecios2 { get => base.PorcVarEnPrecios2; set => base.PorcVarEnPrecios2 = value; }

        /// <summary>
        /// Cuenta descuento PIE
        /// </summary>
        [JsonIgnore]
        public override string? CuentaDescuentoPie { get => base.CuentaDescuentoPie; set => base.CuentaDescuentoPie = value; }

        /// <summary>
        /// Fecha de vencimiento de inscripcion
        /// </summary>
        [JsonIgnore]
        public override DateTime? FechaVencimentoInscripcion { get => base.FechaVencimentoInscripcion; set => base.FechaVencimentoInscripcion = value; }

        /// <summary>
        /// Referencia contable de ventas
        /// </summary>
        [JsonIgnore]
        public override string? ReferenciaContableVentasID { get => base.ReferenciaContableVentasID; set => base.ReferenciaContableVentasID = value; }

        /// <summary>
        /// GrupoID
        /// </summary>

        [JsonIgnore]
        public override short? GrupoID { get => base.GrupoID; set => base.GrupoID = value; }

        
        /// <summary>
        /// Sujeto vinculado
        /// </summary>
        [JsonIgnore]
        public override bool? SujetoVinculado { get => base.SujetoVinculado; set => base.SujetoVinculado = value; }

        
        /// <summary>
        /// Copias de impresion para presupuestos
        /// </summary>
        [JsonIgnore]
        public override byte? CopiasPRE { get => base.CopiasPRE; set => base.CopiasPRE = value; }

        
        /// <summary>
        /// Copias de impresion para notas de pedido
        /// </summary>
        [JsonIgnore]
        public override byte? CopiasNPE { get => base.CopiasNPE; set => base.CopiasNPE = value; }

        
        /// <summary>
        /// Copias de impresion para remito
        /// </summary>
        [JsonIgnore]
        public override byte? CopiasRTO { get => base.CopiasRTO; set => base.CopiasRTO = value; }

        /// <summary>
        /// Copias de impresion para facturas
        /// </summary>
        [JsonIgnore]
        public override byte? CopiasFAC { get => base.CopiasFAC; set => base.CopiasFAC = value; }

        
        /// <summary>
        /// Copias de impresion para notas de credito
        /// </summary>
        [JsonIgnore]
        public override byte? CopiasNCR { get => base.CopiasNCR; set => base.CopiasNCR = value; }

        
        /// <summary>
        /// Copias de impresion para recibo
        /// </summary>
        [JsonIgnore]
        public override byte? CopiasREC { get => base.CopiasREC; set => base.CopiasREC = value; }

        /// <summary>
        /// Cargas porcentaje seguro
        /// </summary>
        [JsonIgnore]
        public override decimal? CargasPorcentajeSeguro { get => base.CargasPorcentajeSeguro; set => base.CargasPorcentajeSeguro = value; }

        /// <summary>
        /// Cargas porcentaje contrareembolso
        /// </summary>
        [JsonIgnore]
        public override decimal? CargasPorcentajeContrareembolso { get => base.CargasPorcentajeContrareembolso; set => base.CargasPorcentajeContrareembolso = value; }

        /// <summary>
        /// Alicuota de IIBB
        /// </summary>
        [JsonIgnore]
        public override decimal? IIBBAlicuota { get => base.IIBBAlicuota; set => base.IIBBAlicuota = value; }

        
        /// <summary>
        /// Alicuota IIBB Vencimiento
        /// </summary>
        [JsonIgnore]
        public override DateTime? IIBBAlicuotaVencimiento { get => base.IIBBAlicuotaVencimiento; set => base.IIBBAlicuotaVencimiento = value; }

        /// <summary>
        /// Tipo de contribuyente ID
        /// </summary>
        [JsonIgnore]
        public override short? TipoContribuyenteIIBBID { get => base.TipoContribuyenteIIBBID; set => base.TipoContribuyenteIIBBID = value; }

        /// <summary>
        /// Dias especial de vencimiento
        /// </summary>
        [JsonIgnore]
        public override byte? DiaEspecialVencimiento { get => base.DiaEspecialVencimiento; set => base.DiaEspecialVencimiento = value; }

        /// <summary>
        /// Repartidor del cliente
        /// </summary>
        [JsonIgnore]
        public override short? Repartidor2ID { get => base.Repartidor2ID; set => base.Repartidor2ID = value; }

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public override byte? CargasSeFacturaA { get => base.CargasSeFacturaA; set => base.CargasSeFacturaA = value; }

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public override string? SENASA_GLP { get => base.SENASA_GLP; set => base.SENASA_GLP = value; }

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public override string? SENASA_COLEGIO { get => base.SENASA_COLEGIO; set => base.SENASA_COLEGIO = value; }


        /// <summary>
        /// Email para envio de comprobantes de ventas 
        /// </summary>
        [JsonIgnore]
        public override bool? PreguntarDestinatariosEmailVentas { get => base.PreguntarDestinatariosEmailVentas; set => base.PreguntarDestinatariosEmailVentas = value; }

        /// <summary>
        /// Enviar xls de comprobantes
        /// </summary>
        [JsonIgnore]
        public override bool? EnviarXLSComprobantes { get => base.EnviarXLSComprobantes; set => base.EnviarXLSComprobantes = value; }


        /// <summary>
        /// SubTipo de comprobantes de venta
        /// </summary>
        [JsonIgnore]
        public override string? SubTipoComprobanteVenta { get => base.SubTipoComprobanteVenta; set => base.SubTipoComprobanteVenta = value; }

        /// <summary>
        /// Habilitado descuento en PIE
        /// </summary>
        [JsonIgnore]
        public override bool? HabilitadoDescuentoPie { get => base.HabilitadoDescuentoPie; set => base.HabilitadoDescuentoPie = value; }


        /// <summary>
        /// Codigo de localidad
        /// </summary>
        [JsonIgnore]
        public override short? LocalidadID { get => base.LocalidadID; set => base.LocalidadID = value; }

    }


}

using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BLL.V3.ERP.Compras;
using GESI.CORE.API.BLL.V3.ERP.Contabilidad;
using GESI.CORE.API.BLL.V3.ERP.Finanzas;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BLL.V3.ERP.Stock;
using GESI.ERP.Compras.V3.BLL;
using GESI.CORE.API.BLL.V3.ERP.Ventas;
using GESI.CORE.V3.BLL;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Ventas.V3.BLL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace GESI.CORE.API.BLL.V3
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiNegocio(this IServiceCollection services, IConfiguration config)
        {
            // CORE
            services.AddGesiERPCore();
            services.AddGesiERPVentas();   // CuentaCorriente (saldos de clientes); no duplica registros del Core
            services.AddGesiERPCompras();  // Formas de pago de proveedores; no duplica registros del Core

            // Sesión del CORE: una por request
            services.AddScoped<SessionManagerAccessor>();
            services.AddScoped<ISessionManagerAccessor>(sp => sp.GetRequiredService<SessionManagerAccessor>());

            // Servicio de Helper
            services.AddScoped<IApiLogger,ApiLogger>();
            services.AddScoped<IEmpresasBLL, EmpresasBLL>();
            services.AddScoped<IAPIHelper, APIHelper>();

            services.AddScoped<IAPIKeyBLL, APIKeyBLL>();
            
            services.Configure<ApiSettings>(config.GetSection(ApiSettings.Seccion));
            services.AddScoped<IApiSessionManager, ApiSessionManager>();
            services.AddScoped<IAccesoApiServicio, AccesoApiServicio>();
            // Servicios de negocio

            services.AddScoped<IApiKeyServicio, ApiKeyServicio>();
            services.AddScoped<IProductosServicio, ProductosServicio>();
            services.AddScoped<IPreciosServicio, PreciosServicio>();
            services.AddScoped<IExistenciasServicio, ExistenciasServicio>();
            services.AddScoped<ICanalesDeVentaServicio, CanalesDeVentaServicio>();
            services.AddScoped<ILoginServicio, LoginServicio>();
            services.AddScoped<IAgentesServicio, AgentesServicio>();
            services.AddScoped<IFormasDePagoServicio, FormasDePagoServicio>();
            services.AddScoped<ICobradoresServicio, CobradoresServicio>();
            services.AddScoped<IZonasServicio, ZonasServicio>();
            services.AddScoped<IVendedoresServicio, VendedoresServicio>();
            services.AddScoped<IPaisesServicio, PaisesServicio>();
            services.AddScoped<IDivisasServicio, DivisasServicio>();
            services.AddScoped<IRepartidoresServicio, RepartidoresServicio>();
            services.AddScoped<IPuntosDeVentaServicio, PuntosDeVentaServicio>();
            services.AddScoped<ITiposDeOperacionServicio, TiposDeOperacionServicio>();
            services.AddScoped<ICategoriasComisionServicio, CategoriasComisionServicio>();
            services.AddScoped<IAlmacenesServicio, AlmacenesServicio>();
            services.AddScoped<IRubrosServicio, RubrosServicio>();
            services.AddScoped<ISubRubrosServicio, SubRubrosServicio>();
            services.AddScoped<ISubSubRubrosServicio, SubSubRubrosServicio>();
            services.AddScoped<IFiltroArticulos1Servicio, FiltroArticulos1Servicio>();
            services.AddScoped<IFiltroArticulos2Servicio, FiltroArticulos2Servicio>();
            services.AddScoped<IFiltroArticulos3Servicio, FiltroArticulos3Servicio>();
            services.AddScoped<IGruposDeProductosServicio, GruposDeProductosServicio>();
            services.AddScoped<IReferenciasContablesServicio, ReferenciasContablesServicio>();
            services.AddScoped<IEstadosDeComprobanteServicio, EstadosDeComprobanteServicio>();
            services.AddScoped<IComprobantesServicio, ComprobantesServicio>();
            services.AddScoped<IEmpresasServicio, EmpresasServicio>();
            services.AddScoped<IClientesServicio, ClientesServicio>();
            services.AddScoped<ITiposDeClienteServicio, TiposDeClienteServicio>();
            services.AddScoped<ITiposDeDocumentoServicio, TiposDeDocumentoServicio>();
            services.AddScoped<IProveedoresServicio, ProveedoresServicio>();
            services.AddScoped<IFormasDePagoProveedoresServicio, FormasDePagoProveedoresServicio>();
            services.AddScoped<IUsuariosServicio, UsuariosServicio>();
            services.AddScoped<ICajasBancosServicio, CajasBancosServicio>();
            services.AddScoped<IMediosDePagoServicio, MediosDePagoServicio>();
            services.AddScoped<IEjerciciosServicio, EjerciciosServicio>();
            services.AddScoped<ISubdiariosServicio, SubdiariosServicio>();
            services.AddScoped<ICuentasServicio, CuentasServicio>();
            services.AddScoped<IAsientosServicio, AsientosServicio>();
            services.AddScoped<IAplicacionesServicio, AplicacionesServicio>();
            services.AddScoped<ICriteriosLogisticaServicio, CriteriosLogisticaServicio>();
            services.AddScoped<IMovimientosDeClientesServicio, MovimientosDeClientesServicio>();
            services.AddScoped<IRepartosServicio, RepartosServicio>();
            services.AddScoped<IMovimientosDeProveedoresServicio, MovimientosDeProveedoresServicio>();
            services.AddScoped<IMovimientosDeStockServicio, MovimientosDeStockServicio>();
            services.AddScoped<IAlmacenesDestinoTransferenciaServicio, AlmacenesDestinoTransferenciaServicio>();
            services.AddScoped<IConfiguracionesServicio, ConfiguracionesServicio>();
            


            return services;
        }
    }
}

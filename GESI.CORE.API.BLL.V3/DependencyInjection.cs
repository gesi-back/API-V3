using GESI.CORE.V3.BLL;
using GESI.ERP.Core.V3.BLL;
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
            services.AddScoped<ICanalesDeVentaServicio, CanalesDeVentaServicio>();
            services.AddScoped<ILoginServicio, LoginServicio>();
            


            return services;
        }
    }
}

using GESI.CORE.API.Web.V3.Handler;
using GESI.CORE.API.Web.V3.Middleware;
using GESI.CORE.API.Web.V3.Services;
using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using Microsoft.AspNetCore.Mvc;
using GESI.CORE.V3.BLL;
using GESI.CORE.V3.DAL;
using GESI.ERP.Compras.V3.BLL;
using GESI.ERP.Compras.V3.Exports;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.DAL;
using GESI.ERP.Ventas.V3.BLL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Session;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

// NUEVO respecto de la V2: el Core (ConfigurationManager) lee la conexión y los appSettings del App.config
// de la carpeta del sitio, en lugar del "GESI.CORE.API.Web.V3.dll.config" que genera la compilación.
// Tiene que ir antes de cualquier acceso a la base.
string rutaAppConfig = Path.Combine(AppContext.BaseDirectory, "App.config");

if (File.Exists(rutaAppConfig))
    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", rutaAppConfig);

var builder = WebApplication.CreateBuilder(args);

// Igual que la V2: las propiedades string no anulables de las clases del Core (ej. dClavesComprobante.TipoEntidad)
// no se vuelven obligatorias en los bodies; si no, MVC rechaza el request con "The X field is required"
builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
})
.ConfigureApiBehaviorOptions(options =>
{
    // NUEVO respecto de la V2: body mal formado o con tipos incorrectos → 400 con cResponse
    // (la V2 devolvía el formato estándar de ASP.NET: title, status, errors)
    options.InvalidModelStateResponseFactory = context =>
    {
        // Campos que fallaron, ej. "$[0].numero", "$.fechaDesde", "pageSize"
        List<string> lstCampos = context.ModelState
                                        .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                                        .Select(x => string.IsNullOrEmpty(x.Key) ? "body" : x.Key)
                                        .ToList();

        string mensaje = "El JSON del body o los parámetros no son válidos. Revisar: " + string.Join(", ", lstCampos);

        // El detalle técnico (en inglés, con línea y posición) va solo al log
        string detalle = string.Join(" | ", context.ModelState
                                                   .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                                                   .Select(x => x.Key + ": " + string.Join("; ", x.Value!.Errors.Select(e => e.ErrorMessage != "" ? e.ErrorMessage : e.Exception?.Message))));

        HttpContext http = context.HttpContext;
        IApiSessionManager sesion = http.RequestServices.GetRequiredService<IApiSessionManager>();
        IApiLogger apiLogger = http.RequestServices.GetRequiredService<IApiLogger>();

        apiLogger.LoguearSucesosAPI("Modelo no válido: " + detalle, tTiposDeError.tError,
                                    sesion.Iniciada ? sesion.Usuario : "", http.Request.Path.Value,
                                    sesion.Iniciada ? sesion.EmpresaID : 0, sesion.Iniciada ? sesion.SucursalID : 0,
                                    codigoErrorInterno: (int)cCodigosError.cModeloNoValido,
                                    parametros: http.Request.QueryString.ToString());

        cResponse oRespuesta = new cResponse();
        oRespuesta.success = false;
        oRespuesta.error = new cError((int)cCodigosError.cModeloNoValido, mensaje, ApiErrors.errJsonInvalido);

        return new BadRequestObjectResult(oRespuesta);
    };
});
// =========================================================
// SWAGGER (igual que la V2)
// =========================================================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);

    // Guía para el programador externo (autenticación, encabezados, errores, paginación).
    // Se muestra arriba de todo en /swagger; se edita en Documentacion\GuiaDelProgramador.md
    string rutaGuia = Path.Combine(AppContext.BaseDirectory, "Documentacion", "GuiaDelProgramador.md");

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API GESI",
        Description = File.Exists(rutaGuia) ? File.ReadAllText(rutaGuia) : null,
        Contact = new OpenApiContact
        {
            Name = "Soporte tecnico API",
            Url = new Uri("https://soporteapi.grupoesi.com.ar"),
            Email = "back.dev@grupoesi.com"
        }
    });

    // AUTENTICACIÓN JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"Autenticación mediante JWT Bearer.

Ingrese el token JWT.
Ejemplo:
Bearer eyJhbGciOiJIUzI1NiIs...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // AUTENTICACIÓN API KEY
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = ApiKeyAuthenticationOptions.HeaderName,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Ingrese su API Key. Alternativa a JWT."
    });

    // HEADER GrupoESI
    options.AddSecurityDefinition("GrupoESI", new OpenApiSecurityScheme
    {
        Name = "GrupoESI",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Ingrese el valor obligatorio del encabezado GrupoESI."
    });

    // SEGURIDAD: GrupoESI + JWT  ó  GrupoESI + API KEY
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("GrupoESI", document)] = [],
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("GrupoESI", document)] = [],
        [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
    });

    // XML COMMENTS: todos los .xml de documentación que quedan junto a la API (API y BO)
    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);

    foreach (var xmlFile in xmlFiles)
    {
        options.IncludeXmlComments(xmlFile);
    }
});


builder.Services.AddApiNegocio(builder.Configuration);
builder.Services.Configure<AuthenticationSettings>(builder.Configuration.GetSection(AuthenticationSettings.Seccion));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<GESI.CORE.API.Web.V3.Services.JwtEventos>();

var auth = builder.Configuration.GetSection(AuthenticationSettings.Seccion).Get<AuthenticationSettings>()
           ?? throw new InvalidOperationException("Falta la sección AuthenticationSettings.");

builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultScheme = "smart";            // decide JWT o ApiKey en cada request
        o.DefaultChallengeScheme = "smart";
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = auth.Issuer,
            ValidateAudience = true,
            ValidAudience = auth.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(auth.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true,
            NameClaimType = "sub",
            RoleClaimType = "roles",
        };
        o.EventsType = typeof(JwtEventos);    // tu respuesta cResponse en el 401
    })
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.SchemeName, _ => { })
    .AddPolicyScheme("smart", "JWT o ApiKey", o =>
    {
        o.ForwardDefaultSelector = ctx =>
            ctx.Request.Headers.ContainsKey(ApiKeyAuthenticationOptions.HeaderName)
                ? ApiKeyAuthenticationOptions.SchemeName
                : JwtBearerDefaults.AuthenticationScheme;
    });

// Todo endpoint exige autenticación, salvo [AllowAnonymous]
builder.Services.AddAuthorization(o =>
    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddProblemDetails();

// Límite de solicitudes por minuto por IP (appsettings.json → "RateLimit": { "SolicitudesPorMinuto": ... })
builder.Services.AddLimiteDeSolicitudes(builder.Configuration);

var myCorsPolicy = "MyCorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myCorsPolicy,
       policy =>
       {
           policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("MiHeaderPersonalizado"); ;
       });
});

var app = builder.Build();

app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

// Va antes que todo lo demás: una solicitud que excede el límite se rechaza sin tocar la base
app.UseRateLimiter();

// Igual que la V2: Swagger en todos los entornos (/swagger y /swagger/v1/swagger.json).
// Va antes del middleware de sesión, así que no pide GrupoESI ni token.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DefaultModelsExpandDepth(-1); // Disable swagger schemas at bottom
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
});

app.UseHttpsRedirection();
// app.UseAuthentication();   // cuando tengas auth, va antes del middleware de sesión
app.UseMiddleware<ERPSessionMiddleware>();
app.UseAuthorization();

app.MapControllers();

// NUEVO respecto de la V2: si se modifica el App.config, la API se detiene para tomar el cambio
// (el Core guarda la conexión en memoria). En IIS se vuelve a levantar sola con el próximo request.
using FileSystemWatcher vigiaAppConfig = new FileSystemWatcher(AppContext.BaseDirectory, "App.config");
vigiaAppConfig.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName;
vigiaAppConfig.Changed += (sender, e) => DetenerPorCambioDeAppConfig(app);
vigiaAppConfig.Created += (sender, e) => DetenerPorCambioDeAppConfig(app);
vigiaAppConfig.Renamed += (sender, e) => DetenerPorCambioDeAppConfig(app);
vigiaAppConfig.EnableRaisingEvents = File.Exists(rutaAppConfig);

app.Run();

static void DetenerPorCambioDeAppConfig(WebApplication app)
{
    app.Logger.LogWarning("Se modificó el App.config: la API se detiene para tomar la nueva configuración.");
    app.Lifetime.StopApplication();
}
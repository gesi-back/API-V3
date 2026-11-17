using API_con_Core_V3.Handler;
using API_con_Core_V3.Middleware;
using API_con_Core_V3.Services;
using GESI.CORE.API.BLL.V3;
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

var builder = WebApplication.CreateBuilder(args);

// Igual que la V2: las propiedades string no anulables de las clases del Core (ej. dClavesComprobante.TipoEntidad)
// no se vuelven obligatorias en los bodies; si no, MVC rechaza el request con "The X field is required"
builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
// =========================================================
// SWAGGER (igual que la V2)
// =========================================================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API GESI",
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
builder.Services.AddScoped<API_con_Core_V3.Services.JwtEventos>();

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
app.Run();
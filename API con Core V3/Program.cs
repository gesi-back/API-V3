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
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();


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

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
// app.UseAuthentication();   // cuando tengas auth, va antes del middleware de sesión
app.UseMiddleware<ERPSessionMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.Run();
using MathApp.Api.Data;
using MathApp.Api.Infraestructura;
using MathApp.Api.Servicios;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Railway indica el puerto en la variable PORT; hay que escuchar en todas las interfaces.
var puerto = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(puerto))
    builder.WebHost.UseUrls($"http://0.0.0.0:{puerto}");

// Cadena de conexión: user-secrets en desarrollo,
// variable de entorno ConnectionStrings__MathApp en Railway.
var cadenaConexion = builder.Configuration.GetConnectionString("MathApp")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:MathApp'.");

builder.Services.AddAccesoDatos(cadenaConexion);

// Autenticación con Firebase: toda la API exige token salvo /health.
builder.Services.AddAutenticacionFirebase(builder.Configuration["Firebase:ProjectId"] ?? "");
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();

// Errores → JSON con código HTTP adecuado + registro en log.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorErrores>();

// /health comprueba la conexión a MySQL (lo usa Railway).
builder.Services.AddHealthChecks().AddCheck<BaseDatosHealthCheck>("mysql");

// CORS: orígenes del frontend React separados por coma en Cors__Origenes,
// p. ej. "https://mathapp.up.railway.app,http://localhost:5173".
var origenes = (builder.Configuration["Cors:Origenes"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod()));

// Railway termina HTTPS en su proxy y reenvía la petición por HTTP;
// estas cabeceras le dicen a ASP.NET cuál era el esquema/IP original.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Botón "Authorize" en Swagger para probar con un token de Firebase.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega aquí el ID token de Firebase (sin la palabra Bearer).",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            Array.Empty<string>()
        },
    });
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection(); // en Railway el HTTPS lo maneja su proxy
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace MathApp.Api.Infraestructura;

public static class AutenticacionFirebase
{
    /// <summary>
    /// Valida el token de Firebase (ID token) que el frontend envía en la cabecera
    /// <c>Authorization: Bearer &lt;token&gt;</c>. Las claves públicas de Google se
    /// descargan y renuevan automáticamente desde la configuración OpenID del proyecto,
    /// por lo que no hace falta ninguna clave privada ni cuenta de servicio.
    /// </summary>
    public static IServiceCollection AddAutenticacionFirebase(this IServiceCollection services, string projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            throw new ArgumentException("Falta el Project ID de Firebase ('Firebase:ProjectId').", nameof(projectId));

        var emisor = $"https://securetoken.google.com/{projectId}";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.Authority = emisor;
                o.MapInboundClaims = false; // conserva los nombres originales: "sub", "name", "email"
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = emisor,
                    ValidateAudience = true,
                    ValidAudience = projectId,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "name",
                };
            });

        // Toda la API exige sesión iniciada salvo lo marcado con [AllowAnonymous]
        // (o .AllowAnonymous()), así un endpoint nuevo nunca queda abierto por olvido.
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }
}

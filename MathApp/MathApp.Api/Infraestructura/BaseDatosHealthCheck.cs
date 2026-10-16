using MathApp.Api.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MathApp.Api.Infraestructura;

/// <summary>
/// Verifica que la API pueda conectarse a MySQL. Railway usa el endpoint /health
/// para saber si un despliegue nuevo está listo antes de enviarle tráfico.
/// </summary>
public sealed class BaseDatosHealthCheck(IDbConnectionFactory fabrica) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            await using var conexion = await fabrica.CrearConexionAsync(ct);
            await using var comando = conexion.CreateCommand();
            comando.CommandText = "SELECT 1";
            await comando.ExecuteScalarAsync(ct);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Sin conexión a la base de datos.", ex);
        }
    }
}

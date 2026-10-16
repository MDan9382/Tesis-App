using System.Data.Common;
using MySqlConnector;

namespace MathApp.Api.Data;

/// <summary>
/// Fábrica de conexiones MySQL. MySqlConnector mantiene un pool interno, por lo
/// que abrir y cerrar una conexión por operación es barato y es la práctica recomendada.
/// </summary>
public sealed class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _cadenaConexion;

    public MySqlConnectionFactory(string cadenaConexion)
    {
        if (string.IsNullOrWhiteSpace(cadenaConexion))
            throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(cadenaConexion));
        _cadenaConexion = cadenaConexion;
    }

    public async Task<DbConnection> CrearConexionAsync(CancellationToken ct = default)
    {
        var conexion = new MySqlConnection(_cadenaConexion);
        await conexion.OpenAsync(ct);
        return conexion;
    }
}

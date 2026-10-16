using System.Data.Common;

namespace MathApp.Api.Data;

/// <summary>Crea conexiones abiertas a la base de datos.</summary>
/// <remarks>
/// Los DAO dependen de esta interfaz y no de MySQL directamente, así es fácil
/// cambiar la cadena de conexión por ambiente o sustituirla en pruebas.
/// </remarks>
public interface IDbConnectionFactory
{
    Task<DbConnection> CrearConexionAsync(CancellationToken ct = default);
}

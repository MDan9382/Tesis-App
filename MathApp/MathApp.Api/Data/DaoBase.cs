using System.Data.Common;

namespace MathApp.Api.Data;

/// <summary>
/// Funcionalidad común de todos los DAO: abrir la conexión, crear el comando con
/// parámetros y ejecutar. Todas las consultas usan parámetros (@nombre), nunca
/// concatenación de texto, lo que protege contra inyección SQL.
/// </summary>
public abstract class DaoBase
{
    private readonly IDbConnectionFactory _fabrica;

    protected DaoBase(IDbConnectionFactory fabrica) => _fabrica = fabrica;

    /// <summary>Ejecuta un SELECT y convierte cada fila con <paramref name="mapear"/>.</summary>
    protected async Task<List<T>> ConsultarAsync<T>(
        string sql,
        Func<DbDataReader, T> mapear,
        IEnumerable<(string Nombre, object? Valor)>? parametros = null,
        CancellationToken ct = default)
    {
        await using var conexion = await _fabrica.CrearConexionAsync(ct);
        await using var comando = CrearComando(conexion, sql, parametros);
        await using var lector = await comando.ExecuteReaderAsync(ct);

        var resultado = new List<T>();
        while (await lector.ReadAsync(ct))
            resultado.Add(mapear(lector));
        return resultado;
    }

    /// <summary>Ejecuta un SELECT y devuelve la primera fila, o <c>null</c> si no hay.</summary>
    protected async Task<T?> ConsultarUnoAsync<T>(
        string sql,
        Func<DbDataReader, T> mapear,
        IEnumerable<(string Nombre, object? Valor)>? parametros = null,
        CancellationToken ct = default) where T : class
    {
        await using var conexion = await _fabrica.CrearConexionAsync(ct);
        await using var comando = CrearComando(conexion, sql, parametros);
        await using var lector = await comando.ExecuteReaderAsync(ct);
        return await lector.ReadAsync(ct) ? mapear(lector) : null;
    }

    /// <summary>Ejecuta una consulta que devuelve un solo valor (COUNT, MAX, etc.).</summary>
    protected async Task<object?> EscalarAsync(
        string sql,
        IEnumerable<(string Nombre, object? Valor)>? parametros = null,
        CancellationToken ct = default)
    {
        await using var conexion = await _fabrica.CrearConexionAsync(ct);
        await using var comando = CrearComando(conexion, sql, parametros);
        var valor = await comando.ExecuteScalarAsync(ct);
        return valor is DBNull ? null : valor;
    }

    /// <summary>Ejecuta INSERT/UPDATE/DELETE y devuelve el número de filas afectadas.</summary>
    protected async Task<int> EjecutarAsync(
        string sql,
        IEnumerable<(string Nombre, object? Valor)>? parametros = null,
        CancellationToken ct = default)
    {
        await using var conexion = await _fabrica.CrearConexionAsync(ct);
        await using var comando = CrearComando(conexion, sql, parametros);
        return await comando.ExecuteNonQueryAsync(ct);
    }

    /// <summary>
    /// Ejecuta un INSERT y devuelve el Id AUTO_INCREMENT generado.
    /// Se lee LAST_INSERT_ID() en la misma conexión, por lo que es seguro con
    /// varios usuarios insertando al mismo tiempo.
    /// </summary>
    protected async Task<int> InsertarYObtenerIdAsync(
        string sql,
        IEnumerable<(string Nombre, object? Valor)>? parametros = null,
        CancellationToken ct = default)
    {
        await using var conexion = await _fabrica.CrearConexionAsync(ct);
        await using var comando = CrearComando(conexion, sql + "; SELECT LAST_INSERT_ID();", parametros);
        var id = await comando.ExecuteScalarAsync(ct);
        return Convert.ToInt32(id);
    }

    private static DbCommand CrearComando(
        DbConnection conexion,
        string sql,
        IEnumerable<(string Nombre, object? Valor)>? parametros)
    {
        var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        if (parametros is null) return comando;

        foreach (var (nombre, valor) in parametros)
        {
            var p = comando.CreateParameter();
            p.ParameterName = nombre;
            p.Value = valor ?? DBNull.Value;
            comando.Parameters.Add(p);
        }
        return comando;
    }
}

using System.Data.Common;
using MathApp.Api.Models;

namespace MathApp.Api.Data.Daos;

public interface IResultadoPruebaDao
{
    Task<ResultadoPrueba?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<List<ResultadoPrueba>> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken ct = default);
    Task<List<ResultadoPrueba>> ObtenerIntentosAsync(int usuarioId, int pruebaId, CancellationToken ct = default);
    Task<int?> ObtenerMejorPuntajeAsync(int usuarioId, int pruebaId, CancellationToken ct = default);
    Task<int> RegistrarAsync(ResultadoPrueba resultado, CancellationToken ct = default);
    Task<bool> EliminarAsync(int id, CancellationToken ct = default);
}

public class ResultadoPruebaDao : DaoBase, IResultadoPruebaDao
{
    private const string Columnas = "RP_Id, RP_Puntaje, RP_Fecha_Completado, Prueba_Id, Usuario_Id";

    public ResultadoPruebaDao(IDbConnectionFactory fabrica) : base(fabrica) { }

    public Task<ResultadoPrueba?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        ConsultarUnoAsync($"SELECT {Columnas} FROM Resultados_Pruebas WHERE RP_Id = @id",
            Mapear, [("@id", id)], ct);

    /// <summary>Todos los resultados del usuario, del más reciente al más antiguo (para su perfil).</summary>
    public Task<List<ResultadoPrueba>> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken ct = default) =>
        ConsultarAsync(
            $"SELECT {Columnas} FROM Resultados_Pruebas WHERE Usuario_Id = @usuarioId ORDER BY RP_Fecha_Completado DESC, RP_Id DESC",
            Mapear, [("@usuarioId", usuarioId)], ct);

    /// <summary>Historial de intentos de un usuario en una prueba.</summary>
    public Task<List<ResultadoPrueba>> ObtenerIntentosAsync(int usuarioId, int pruebaId, CancellationToken ct = default) =>
        ConsultarAsync(
            $"SELECT {Columnas} FROM Resultados_Pruebas WHERE Usuario_Id = @usuarioId AND Prueba_Id = @pruebaId ORDER BY RP_Id",
            Mapear, [("@usuarioId", usuarioId), ("@pruebaId", pruebaId)], ct);

    public async Task<int?> ObtenerMejorPuntajeAsync(int usuarioId, int pruebaId, CancellationToken ct = default)
    {
        var valor = await EscalarAsync(
            "SELECT MAX(RP_Puntaje) FROM Resultados_Pruebas WHERE Usuario_Id = @usuarioId AND Prueba_Id = @pruebaId",
            [("@usuarioId", usuarioId), ("@pruebaId", pruebaId)], ct);
        return valor is null ? null : Convert.ToInt32(valor);
    }

    /// <summary>
    /// Registra un intento nuevo. Si no se indica fecha se usa la del servidor de BD.
    /// </summary>
    public async Task<int> RegistrarAsync(ResultadoPrueba resultado, CancellationToken ct = default)
    {
        resultado.Id = await InsertarYObtenerIdAsync(
            """
            INSERT INTO Resultados_Pruebas (RP_Puntaje, RP_Fecha_Completado, Prueba_Id, Usuario_Id)
            VALUES (@puntaje, COALESCE(@fecha, CURRENT_TIMESTAMP), @pruebaId, @usuarioId)
            """,
            [("@puntaje", resultado.Puntaje), ("@fecha", resultado.FechaCompletado),
             ("@pruebaId", resultado.PruebaId), ("@usuarioId", resultado.UsuarioId)], ct);
        return resultado.Id;
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken ct = default) =>
        await EjecutarAsync("DELETE FROM Resultados_Pruebas WHERE RP_Id = @id", [("@id", id)], ct) > 0;

    private static ResultadoPrueba Mapear(DbDataReader r) => new()
    {
        Id = r.GetInt32("RP_Id"),
        Puntaje = r.GetNullableInt32("RP_Puntaje"),
        FechaCompletado = r.GetNullableDateTime("RP_Fecha_Completado"),
        PruebaId = r.GetInt32("Prueba_Id"),
        UsuarioId = r.GetInt32("Usuario_Id"),
    };
}

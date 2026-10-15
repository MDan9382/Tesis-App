using System.Data.Common;
using MathApp.Api.Models;

namespace MathApp.Api.Data.Daos;

public interface IResultadoEjercicioDao
{
    Task<ResultadoEjercicio?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<List<ResultadoEjercicio>> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken ct = default);
    Task<List<ResultadoEjercicio>> ObtenerIntentosAsync(int usuarioId, int ejercicioId, CancellationToken ct = default);
    Task<int?> ObtenerMejorPuntajeAsync(int usuarioId, int ejercicioId, CancellationToken ct = default);
    Task<int> RegistrarAsync(ResultadoEjercicio resultado, CancellationToken ct = default);
    Task<bool> EliminarAsync(int id, CancellationToken ct = default);
}

public class ResultadoEjercicioDao : DaoBase, IResultadoEjercicioDao
{
    private const string Columnas = "RE_Id, RE_Puntaje, RE_Fecha_Completado, Ejercicio_Id, Usuario_Id";

    public ResultadoEjercicioDao(IDbConnectionFactory fabrica) : base(fabrica) { }

    public Task<ResultadoEjercicio?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        ConsultarUnoAsync($"SELECT {Columnas} FROM Resultados_Ejercicios WHERE RE_Id = @id",
            Mapear, [("@id", id)], ct);

    /// <summary>Todos los resultados del usuario, del más reciente al más antiguo (para su perfil).</summary>
    public Task<List<ResultadoEjercicio>> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken ct = default) =>
        ConsultarAsync(
            $"SELECT {Columnas} FROM Resultados_Ejercicios WHERE Usuario_Id = @usuarioId ORDER BY RE_Fecha_Completado DESC, RE_Id DESC",
            Mapear, [("@usuarioId", usuarioId)], ct);

    /// <summary>Historial de intentos de un usuario en un ejercicio.</summary>
    public Task<List<ResultadoEjercicio>> ObtenerIntentosAsync(int usuarioId, int ejercicioId, CancellationToken ct = default) =>
        ConsultarAsync(
            $"SELECT {Columnas} FROM Resultados_Ejercicios WHERE Usuario_Id = @usuarioId AND Ejercicio_Id = @ejercicioId ORDER BY RE_Id",
            Mapear, [("@usuarioId", usuarioId), ("@ejercicioId", ejercicioId)], ct);

    public async Task<int?> ObtenerMejorPuntajeAsync(int usuarioId, int ejercicioId, CancellationToken ct = default)
    {
        var valor = await EscalarAsync(
            "SELECT MAX(RE_Puntaje) FROM Resultados_Ejercicios WHERE Usuario_Id = @usuarioId AND Ejercicio_Id = @ejercicioId",
            [("@usuarioId", usuarioId), ("@ejercicioId", ejercicioId)], ct);
        return valor is null ? null : Convert.ToInt32(valor);
    }

    /// <summary>
    /// Registra un intento nuevo. Si no se indica fecha se usa la del servidor de BD.
    /// </summary>
    public async Task<int> RegistrarAsync(ResultadoEjercicio resultado, CancellationToken ct = default)
    {
        resultado.Id = await InsertarYObtenerIdAsync(
            """
            INSERT INTO Resultados_Ejercicios (RE_Puntaje, RE_Fecha_Completado, Ejercicio_Id, Usuario_Id)
            VALUES (@puntaje, COALESCE(@fecha, CURRENT_TIMESTAMP), @ejercicioId, @usuarioId)
            """,
            [("@puntaje", resultado.Puntaje), ("@fecha", resultado.FechaCompletado),
             ("@ejercicioId", resultado.EjercicioId), ("@usuarioId", resultado.UsuarioId)], ct);
        return resultado.Id;
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken ct = default) =>
        await EjecutarAsync("DELETE FROM Resultados_Ejercicios WHERE RE_Id = @id", [("@id", id)], ct) > 0;

    private static ResultadoEjercicio Mapear(DbDataReader r) => new()
    {
        Id = r.GetInt32("RE_Id"),
        Puntaje = r.GetNullableInt32("RE_Puntaje"),
        FechaCompletado = r.GetNullableDateTime("RE_Fecha_Completado"),
        EjercicioId = r.GetInt32("Ejercicio_Id"),
        UsuarioId = r.GetInt32("Usuario_Id"),
    };
}

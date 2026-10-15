using System.Data.Common;
using MathApp.Api.Models;

namespace MathApp.Api.Data.Daos;

public interface IProgresoLeccionDao
{
    Task<List<ProgresoLeccion>> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken ct = default);
    Task<ProgresoLeccion?> ObtenerAsync(int usuarioId, int leccionId, CancellationToken ct = default);
    Task<List<ProgresoLeccion>> ObtenerPorUsuarioYTemaAsync(int usuarioId, int temaId, CancellationToken ct = default);
    Task<int> MarcarCompletadaAsync(int usuarioId, int leccionId, CancellationToken ct = default);
    Task<bool> EliminarAsync(int id, CancellationToken ct = default);
}

public class ProgresoLeccionDao : DaoBase, IProgresoLeccionDao
{
    private const string Columnas = "p.PL_Id, p.PL_Fecha_Completada, p.Usuario_Id, p.Leccion_Id";

    public ProgresoLeccionDao(IDbConnectionFactory fabrica) : base(fabrica) { }

    /// <summary>Todo el progreso de un usuario (para su perfil).</summary>
    public Task<List<ProgresoLeccion>> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken ct = default) =>
        ConsultarAsync(
            $"SELECT {Columnas} FROM Progreso_Lecciones p WHERE p.Usuario_Id = @usuarioId ORDER BY p.PL_Fecha_Completada DESC",
            Mapear, [("@usuarioId", usuarioId)], ct);

    public Task<ProgresoLeccion?> ObtenerAsync(int usuarioId, int leccionId, CancellationToken ct = default) =>
        ConsultarUnoAsync(
            $"SELECT {Columnas} FROM Progreso_Lecciones p WHERE p.Usuario_Id = @usuarioId AND p.Leccion_Id = @leccionId ORDER BY p.PL_Id DESC LIMIT 1",
            Mapear, [("@usuarioId", usuarioId), ("@leccionId", leccionId)], ct);

    /// <summary>Progreso del usuario dentro de un tema (para la ruta de aprendizaje).</summary>
    public Task<List<ProgresoLeccion>> ObtenerPorUsuarioYTemaAsync(int usuarioId, int temaId, CancellationToken ct = default) =>
        ConsultarAsync(
            $"""
            SELECT {Columnas} FROM Progreso_Lecciones p
            INNER JOIN Lecciones l ON l.Leccion_Id = p.Leccion_Id
            WHERE p.Usuario_Id = @usuarioId AND l.Tema_Id = @temaId
            ORDER BY l.Leccion_Orden, p.Leccion_Id
            """,
            Mapear, [("@usuarioId", usuarioId), ("@temaId", temaId)], ct);

    /// <summary>
    /// Marca la lección como completada (requisito 9). Si el usuario ya tenía un
    /// registro para esa lección se actualiza la fecha (repetir lecciones, requisito 12);
    /// si no, se crea. Devuelve el PL_Id.
    /// </summary>
    public async Task<int> MarcarCompletadaAsync(int usuarioId, int leccionId, CancellationToken ct = default)
    {
        var existente = await ObtenerAsync(usuarioId, leccionId, ct);
        if (existente is not null)
        {
            await EjecutarAsync(
                "UPDATE Progreso_Lecciones SET PL_Fecha_Completada = CURRENT_TIMESTAMP WHERE PL_Id = @id",
                [("@id", existente.Id)], ct);
            return existente.Id;
        }

        return await InsertarYObtenerIdAsync(
            "INSERT INTO Progreso_Lecciones (PL_Fecha_Completada, Usuario_Id, Leccion_Id) VALUES (CURRENT_TIMESTAMP, @usuarioId, @leccionId)",
            [("@usuarioId", usuarioId), ("@leccionId", leccionId)], ct);
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken ct = default) =>
        await EjecutarAsync("DELETE FROM Progreso_Lecciones WHERE PL_Id = @id", [("@id", id)], ct) > 0;

    private static ProgresoLeccion Mapear(DbDataReader r) => new()
    {
        Id = r.GetInt32("PL_Id"),
        FechaCompletada = r.GetNullableDateTime("PL_Fecha_Completada"),
        UsuarioId = r.GetInt32("Usuario_Id"),
        LeccionId = r.GetInt32("Leccion_Id"),
    };
}

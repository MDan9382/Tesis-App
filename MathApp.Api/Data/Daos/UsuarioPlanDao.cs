using System.Data.Common;
using MathApp.Api.Models;

namespace MathApp.Api.Data.Daos;

public interface IUsuarioPlanDao
{
    Task<UsuarioPlan?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<List<UsuarioPlan>> ObtenerPorUsuarioAsync(int usuarioId, bool soloActivos = true, CancellationToken ct = default);
    Task<UsuarioPlan?> ObtenerAsync(int usuarioId, int planId, CancellationToken ct = default);
    Task<int> ActivarAsync(int usuarioId, int planId, CancellationToken ct = default);
    Task<bool> DesactivarAsync(int usuarioId, int planId, CancellationToken ct = default);
}

public class UsuarioPlanDao : DaoBase, IUsuarioPlanDao
{
    private const string Columnas = "UP_Id, UP_Fecha_Activacion, UP_Activo, Plan_Id, Usuario_Id";

    public UsuarioPlanDao(IDbConnectionFactory fabrica) : base(fabrica) { }

    public Task<UsuarioPlan?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        ConsultarUnoAsync($"SELECT {Columnas} FROM Usuario_Planes WHERE UP_Id = @id",
            Mapear, [("@id", id)], ct);

    public Task<List<UsuarioPlan>> ObtenerPorUsuarioAsync(int usuarioId, bool soloActivos = true, CancellationToken ct = default) =>
        ConsultarAsync(
            $"""
            SELECT {Columnas} FROM Usuario_Planes
            WHERE Usuario_Id = @usuarioId AND (@soloActivos = FALSE OR UP_Activo = TRUE)
            ORDER BY UP_Fecha_Activacion DESC
            """,
            Mapear, [("@usuarioId", usuarioId), ("@soloActivos", soloActivos)], ct);

    /// <summary>Registro más reciente del usuario para un plan, activo o no.</summary>
    public Task<UsuarioPlan?> ObtenerAsync(int usuarioId, int planId, CancellationToken ct = default) =>
        ConsultarUnoAsync(
            $"""
            SELECT {Columnas} FROM Usuario_Planes
            WHERE Usuario_Id = @usuarioId AND Plan_Id = @planId
            ORDER BY UP_Id DESC LIMIT 1
            """,
            Mapear, [("@usuarioId", usuarioId), ("@planId", planId)], ct);

    /// <summary>
    /// Activa un plan para el usuario. Si ya tenía un registro para ese plan lo
    /// reactiva (y actualiza la fecha); si no, crea uno nuevo. Devuelve el UP_Id.
    /// </summary>
    public async Task<int> ActivarAsync(int usuarioId, int planId, CancellationToken ct = default)
    {
        var existente = await ObtenerAsync(usuarioId, planId, ct);
        if (existente is not null)
        {
            await EjecutarAsync(
                "UPDATE Usuario_Planes SET UP_Activo = TRUE, UP_Fecha_Activacion = CURRENT_TIMESTAMP WHERE UP_Id = @id",
                [("@id", existente.Id)], ct);
            return existente.Id;
        }

        return await InsertarYObtenerIdAsync(
            "INSERT INTO Usuario_Planes (Usuario_Id, Plan_Id) VALUES (@usuarioId, @planId)",
            [("@usuarioId", usuarioId), ("@planId", planId)], ct);
    }

    public async Task<bool> DesactivarAsync(int usuarioId, int planId, CancellationToken ct = default) =>
        await EjecutarAsync(
            "UPDATE Usuario_Planes SET UP_Activo = FALSE WHERE Usuario_Id = @usuarioId AND Plan_Id = @planId",
            [("@usuarioId", usuarioId), ("@planId", planId)], ct) > 0;

    private static UsuarioPlan Mapear(DbDataReader r) => new()
    {
        Id = r.GetInt32("UP_Id"),
        FechaActivacion = r.GetDateTime("UP_Fecha_Activacion"),
        Activo = r.GetNullableBoolean("UP_Activo") ?? false, // NULL se interpreta como inactivo
        PlanId = r.GetInt32("Plan_Id"),
        UsuarioId = r.GetInt32("Usuario_Id"),
    };
}

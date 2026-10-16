using System.Data.Common;
using MathApp.Api.Models;

namespace MathApp.Api.Data.Daos;

public interface ITemaDao
{
    Task<List<Tema>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<List<Tema>> ObtenerPorPlanAsync(int planId, CancellationToken ct = default);
    Task<Tema?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<int> InsertarAsync(Tema tema, CancellationToken ct = default);
    Task<bool> ActualizarAsync(Tema tema, CancellationToken ct = default);
    Task<bool> EliminarAsync(int id, CancellationToken ct = default);
}

public class TemaDao : DaoBase, ITemaDao
{
    private const string Columnas = "Tema_Id, Tema_Nombre, Tema_Orden, Plan_Id";

    public TemaDao(IDbConnectionFactory fabrica) : base(fabrica) { }

    public Task<List<Tema>> ObtenerTodosAsync(CancellationToken ct = default) =>
        ConsultarAsync($"SELECT {Columnas} FROM Temas ORDER BY Plan_Id, Tema_Orden, Tema_Id", Mapear, ct: ct);

    /// <summary>Temas de un plan en el orden de la ruta de aprendizaje.</summary>
    public Task<List<Tema>> ObtenerPorPlanAsync(int planId, CancellationToken ct = default) =>
        ConsultarAsync($"SELECT {Columnas} FROM Temas WHERE Plan_Id = @planId ORDER BY Tema_Orden, Tema_Id",
            Mapear, [("@planId", planId)], ct);

    public Task<Tema?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        ConsultarUnoAsync($"SELECT {Columnas} FROM Temas WHERE Tema_Id = @id",
            Mapear, [("@id", id)], ct);

    public async Task<int> InsertarAsync(Tema tema, CancellationToken ct = default)
    {
        tema.Id = await InsertarYObtenerIdAsync(
            "INSERT INTO Temas (Tema_Nombre, Tema_Orden, Plan_Id) VALUES (@nombre, @orden, @planId)",
            [("@nombre", tema.Nombre), ("@orden", tema.Orden), ("@planId", tema.PlanId)], ct);
        return tema.Id;
    }

    public async Task<bool> ActualizarAsync(Tema tema, CancellationToken ct = default) =>
        await EjecutarAsync("UPDATE Temas SET Tema_Nombre = @nombre, Tema_Orden = @orden, Plan_Id = @planId WHERE Tema_Id = @id",
            [("@nombre", tema.Nombre), ("@orden", tema.Orden), ("@planId", tema.PlanId), ("@id", tema.Id)], ct) > 0;

    /// <remarks>Falla (error MySQL 1451) si el tema todavía tiene lecciones.</remarks>
    public async Task<bool> EliminarAsync(int id, CancellationToken ct = default) =>
        await EjecutarAsync("DELETE FROM Temas WHERE Tema_Id = @id", [("@id", id)], ct) > 0;

    private static Tema Mapear(DbDataReader r) => new()
    {
        Id = r.GetInt32("Tema_Id"),
        Nombre = r.GetString("Tema_Nombre"),
        Orden = r.GetInt32("Tema_Orden"),
        PlanId = r.GetInt32("Plan_Id"),
    };
}

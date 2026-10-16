using System.Data.Common;
using MathApp.Api.Models;

namespace MathApp.Api.Data.Daos;

public interface IPlanEstudioDao
{
    Task<List<PlanEstudio>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<PlanEstudio?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<int> InsertarAsync(PlanEstudio plan, CancellationToken ct = default);
    Task<bool> ActualizarAsync(PlanEstudio plan, CancellationToken ct = default);
    Task<bool> EliminarAsync(int id, CancellationToken ct = default);
}

public class PlanEstudioDao : DaoBase, IPlanEstudioDao
{
    private const string Columnas = "Plan_Id, Plan_Nombre, Plan_Descripcion";

    public PlanEstudioDao(IDbConnectionFactory fabrica) : base(fabrica) { }

    public Task<List<PlanEstudio>> ObtenerTodosAsync(CancellationToken ct = default) =>
        ConsultarAsync($"SELECT {Columnas} FROM Planes_Estudio ORDER BY Plan_Id", Mapear, ct: ct);

    public Task<PlanEstudio?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        ConsultarUnoAsync($"SELECT {Columnas} FROM Planes_Estudio WHERE Plan_Id = @id",
            Mapear, [("@id", id)], ct);

    public async Task<int> InsertarAsync(PlanEstudio plan, CancellationToken ct = default)
    {
        plan.Id = await InsertarYObtenerIdAsync(
            "INSERT INTO Planes_Estudio (Plan_Nombre, Plan_Descripcion) VALUES (@nombre, @descripcion)",
            [("@nombre", plan.Nombre), ("@descripcion", plan.Descripcion)], ct);
        return plan.Id;
    }

    public async Task<bool> ActualizarAsync(PlanEstudio plan, CancellationToken ct = default) =>
        await EjecutarAsync(
            "UPDATE Planes_Estudio SET Plan_Nombre = @nombre, Plan_Descripcion = @descripcion WHERE Plan_Id = @id",
            [("@nombre", plan.Nombre), ("@descripcion", plan.Descripcion), ("@id", plan.Id)], ct) > 0;

    /// <remarks>Falla (error MySQL 1451) si el plan todavía tiene temas o usuarios asociados.</remarks>
    public async Task<bool> EliminarAsync(int id, CancellationToken ct = default) =>
        await EjecutarAsync("DELETE FROM Planes_Estudio WHERE Plan_Id = @id", [("@id", id)], ct) > 0;

    private static PlanEstudio Mapear(DbDataReader r) => new()
    {
        Id = r.GetInt32("Plan_Id"),
        Nombre = r.GetString("Plan_Nombre"),
        Descripcion = r.GetString("Plan_Descripcion"),
    };
}

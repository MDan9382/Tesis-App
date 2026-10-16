using System.Data.Common;
using MathApp.Api.Models;

namespace MathApp.Api.Data.Daos;

public interface IPruebaDao
{
    Task<Prueba?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<List<Prueba>> ObtenerPorTemaAsync(int temaId, bool? esPrueba = null, CancellationToken ct = default);
    Task<int> InsertarAsync(Prueba prueba, CancellationToken ct = default);
    Task<bool> ActualizarAsync(Prueba prueba, CancellationToken ct = default);
    Task<bool> EliminarAsync(int id, CancellationToken ct = default);
}

public class PruebaDao : DaoBase, IPruebaDao
{
    private const string Columnas = "Prueba_Id, Prueba_Contenido, Prueba_EsPrueba, Tema_Id";

    public PruebaDao(IDbConnectionFactory fabrica) : base(fabrica) { }

    public Task<Prueba?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        ConsultarUnoAsync($"SELECT {Columnas} FROM Pruebas WHERE Prueba_Id = @id",
            Mapear, [("@id", id)], ct);

    /// <param name="esPrueba"><c>null</c> = todas; <c>true</c>/<c>false</c> filtra por Prueba_EsPrueba.</param>
    public Task<List<Prueba>> ObtenerPorTemaAsync(int temaId, bool? esPrueba = null, CancellationToken ct = default) =>
        ConsultarAsync(
            $"""
            SELECT {Columnas} FROM Pruebas
            WHERE Tema_Id = @temaId
              AND (@esPrueba IS NULL OR Prueba_EsPrueba = @esPrueba)
            ORDER BY Prueba_Id
            """,
            Mapear, [("@temaId", temaId), ("@esPrueba", esPrueba)], ct);

    public async Task<int> InsertarAsync(Prueba prueba, CancellationToken ct = default)
    {
        prueba.Id = await InsertarYObtenerIdAsync(
            "INSERT INTO Pruebas (Prueba_Contenido, Prueba_EsPrueba, Tema_Id) VALUES (@contenido, @esPrueba, @temaId)",
            [("@contenido", prueba.Contenido), ("@esPrueba", prueba.EsPrueba), ("@temaId", prueba.TemaId)], ct);
        return prueba.Id;
    }

    public async Task<bool> ActualizarAsync(Prueba prueba, CancellationToken ct = default) =>
        await EjecutarAsync(
            """
            UPDATE Pruebas
            SET Prueba_Contenido = @contenido, Prueba_EsPrueba = @esPrueba, Tema_Id = @temaId
            WHERE Prueba_Id = @id
            """,
            [("@contenido", prueba.Contenido), ("@esPrueba", prueba.EsPrueba),
             ("@temaId", prueba.TemaId), ("@id", prueba.Id)], ct) > 0;

    /// <remarks>Falla (error MySQL 1451) si la prueba tiene resultados registrados.</remarks>
    public async Task<bool> EliminarAsync(int id, CancellationToken ct = default) =>
        await EjecutarAsync("DELETE FROM Pruebas WHERE Prueba_Id = @id", [("@id", id)], ct) > 0;

    private static Prueba Mapear(DbDataReader r) => new()
    {
        Id = r.GetInt32("Prueba_Id"),
        Contenido = r.GetNullableString("Prueba_Contenido"),
        EsPrueba = r.GetBoolean("Prueba_EsPrueba"),
        TemaId = r.GetInt32("Tema_Id"),
    };
}

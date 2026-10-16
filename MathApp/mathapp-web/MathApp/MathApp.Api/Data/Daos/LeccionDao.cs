using System.Data.Common;
using MathApp.Api.Models;

namespace MathApp.Api.Data.Daos;

public interface ILeccionDao
{
    Task<Leccion?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<List<LeccionResumen>> ObtenerPorTemaAsync(int temaId, CancellationToken ct = default);
    Task<List<LeccionResumen>> BuscarPorNombreAsync(string texto, int limite = 20, CancellationToken ct = default);
    Task<int> InsertarAsync(Leccion leccion, CancellationToken ct = default);
    Task<bool> ActualizarAsync(Leccion leccion, CancellationToken ct = default);
    Task<bool> EliminarAsync(int id, CancellationToken ct = default);
}

public class LeccionDao : DaoBase, ILeccionDao
{
    public LeccionDao(IDbConnectionFactory fabrica) : base(fabrica) { }

    /// <summary>Lección completa, incluido el contenido (para mostrarla).</summary>
    public Task<Leccion?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        ConsultarUnoAsync(
            "SELECT Leccion_Id, Leccion_Nombre, Leccion_Contenido, Leccion_Orden, Tema_Id FROM Lecciones WHERE Leccion_Id = @id",
            MapearCompleta, [("@id", id)], ct);

    /// <summary>Lecciones de un tema en el orden de la ruta, sin contenido (para menús y listados).</summary>
    public Task<List<LeccionResumen>> ObtenerPorTemaAsync(int temaId, CancellationToken ct = default) =>
        ConsultarAsync(
            "SELECT Leccion_Id, Leccion_Nombre, Leccion_Orden, Tema_Id FROM Lecciones WHERE Tema_Id = @temaId ORDER BY Leccion_Orden, Leccion_Id",
            MapearResumen, [("@temaId", temaId)], ct);

    /// <summary>
    /// Búsqueda de lecciones por nombre (requisito funcional 8). Los caracteres
    /// comodín que escriba el usuario (% y _) se escapan para que se busquen literalmente.
    /// </summary>
    public Task<List<LeccionResumen>> BuscarPorNombreAsync(string texto, int limite = 20, CancellationToken ct = default)
    {
        var patron = "%" + EscaparLike(texto.Trim()) + "%";
        return ConsultarAsync(
            """
            SELECT Leccion_Id, Leccion_Nombre, Leccion_Orden, Tema_Id FROM Lecciones
            WHERE Leccion_Nombre LIKE @patron
            ORDER BY Leccion_Nombre
            LIMIT @limite
            """,
            MapearResumen, [("@patron", patron), ("@limite", limite)], ct);
    }

    public async Task<int> InsertarAsync(Leccion leccion, CancellationToken ct = default)
    {
        leccion.Id = await InsertarYObtenerIdAsync(
            "INSERT INTO Lecciones (Leccion_Nombre, Leccion_Contenido, Leccion_Orden, Tema_Id) VALUES (@nombre, @contenido, @orden, @temaId)",
            [("@nombre", leccion.Nombre), ("@contenido", leccion.Contenido), ("@orden", leccion.Orden), ("@temaId", leccion.TemaId)], ct);
        return leccion.Id;
    }

    public async Task<bool> ActualizarAsync(Leccion leccion, CancellationToken ct = default) =>
        await EjecutarAsync(
            """
            UPDATE Lecciones
            SET Leccion_Nombre = @nombre, Leccion_Contenido = @contenido, Leccion_Orden = @orden, Tema_Id = @temaId
            WHERE Leccion_Id = @id
            """,
            [("@nombre", leccion.Nombre), ("@contenido", leccion.Contenido), ("@orden", leccion.Orden),
             ("@temaId", leccion.TemaId), ("@id", leccion.Id)], ct) > 0;

    /// <remarks>Falla (error MySQL 1451) si la lección tiene ejercicios o progreso registrado.</remarks>
    public async Task<bool> EliminarAsync(int id, CancellationToken ct = default) =>
        await EjecutarAsync("DELETE FROM Lecciones WHERE Leccion_Id = @id", [("@id", id)], ct) > 0;

    private static string EscaparLike(string texto) =>
        texto.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");

    private static Leccion MapearCompleta(DbDataReader r) => new()
    {
        Id = r.GetInt32("Leccion_Id"),
        Nombre = r.GetString("Leccion_Nombre"),
        Contenido = r.GetString("Leccion_Contenido"),
        Orden = r.GetInt32("Leccion_Orden"),
        TemaId = r.GetInt32("Tema_Id"),
    };

    private static LeccionResumen MapearResumen(DbDataReader r) => new()
    {
        Id = r.GetInt32("Leccion_Id"),
        Nombre = r.GetString("Leccion_Nombre"),
        Orden = r.GetInt32("Leccion_Orden"),
        TemaId = r.GetInt32("Tema_Id"),
    };
}

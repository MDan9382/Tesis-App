using System.Data.Common;
using MathApp.Api.Models;

namespace MathApp.Api.Data.Daos;

public interface IUsuarioDao
{
    Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<Usuario?> ObtenerPorFirebaseUidAsync(string firebaseUid, CancellationToken ct = default);
    Task<int> InsertarAsync(Usuario usuario, CancellationToken ct = default);
    Task<Usuario> ObtenerOCrearAsync(string firebaseUid, string? nombre, CancellationToken ct = default);
    Task<bool> ActualizarNombreAsync(int id, string? nombre, CancellationToken ct = default);
    Task<bool> EliminarAsync(int id, CancellationToken ct = default);
}

public class UsuarioDao : DaoBase, IUsuarioDao
{
    private const string Columnas = "Usuario_Id, Usuario_FirebaseUID, Usuario_Nombre";

    public UsuarioDao(IDbConnectionFactory fabrica) : base(fabrica) { }

    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        ConsultarUnoAsync($"SELECT {Columnas} FROM Usuarios WHERE Usuario_Id = @id",
            Mapear, [("@id", id)], ct);

    public Task<Usuario?> ObtenerPorFirebaseUidAsync(string firebaseUid, CancellationToken ct = default) =>
        ConsultarUnoAsync($"SELECT {Columnas} FROM Usuarios WHERE Usuario_FirebaseUID = @uid",
            Mapear, [("@uid", firebaseUid)], ct);

    public async Task<int> InsertarAsync(Usuario usuario, CancellationToken ct = default)
    {
        usuario.Id = await InsertarYObtenerIdAsync(
            "INSERT INTO Usuarios (Usuario_FirebaseUID, Usuario_Nombre) VALUES (@uid, @nombre)",
            [("@uid", usuario.FirebaseUid), ("@nombre", usuario.Nombre)], ct);
        return usuario.Id;
    }

    /// <summary>
    /// Pensado para el inicio de sesión con Firebase: si el UID ya existe devuelve
    /// ese usuario, si no, lo crea. Se hace en una sola sentencia atómica apoyándose
    /// en la restricción UNIQUE de Usuario_FirebaseUID, así dos peticiones simultáneas
    /// del mismo usuario no generan un duplicado ni un error.
    /// </summary>
    public async Task<Usuario> ObtenerOCrearAsync(string firebaseUid, string? nombre, CancellationToken ct = default)
    {
        var id = await InsertarYObtenerIdAsync(
            """
            INSERT INTO Usuarios (Usuario_FirebaseUID, Usuario_Nombre) VALUES (@uid, @nombre)
            ON DUPLICATE KEY UPDATE Usuario_Id = LAST_INSERT_ID(Usuario_Id)
            """,
            [("@uid", firebaseUid), ("@nombre", nombre)], ct);

        return (await ObtenerPorIdAsync(id, ct))!;
    }

    public async Task<bool> ActualizarNombreAsync(int id, string? nombre, CancellationToken ct = default) =>
        await EjecutarAsync("UPDATE Usuarios SET Usuario_Nombre = @nombre WHERE Usuario_Id = @id",
            [("@nombre", nombre), ("@id", id)], ct) > 0;

    /// <remarks>Falla (error MySQL 1451) si el usuario tiene progreso, resultados o planes registrados.</remarks>
    public async Task<bool> EliminarAsync(int id, CancellationToken ct = default) =>
        await EjecutarAsync("DELETE FROM Usuarios WHERE Usuario_Id = @id", [("@id", id)], ct) > 0;

    private static Usuario Mapear(DbDataReader r) => new()
    {
        Id = r.GetInt32("Usuario_Id"),
        FirebaseUid = r.GetString("Usuario_FirebaseUID"),
        Nombre = r.GetNullableString("Usuario_Nombre"),
    };
}

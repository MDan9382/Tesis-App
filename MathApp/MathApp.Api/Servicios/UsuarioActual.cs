using MathApp.Api.Data.Daos;
using MathApp.Api.Models;

namespace MathApp.Api.Servicios;

/// <summary>
/// Da acceso al usuario que hace la petición. La identidad sale SIEMPRE del token
/// de Firebase validado, nunca de datos que envíe el cliente.
/// </summary>
public interface IUsuarioActual
{
    /// <summary>UID de Firebase del token.</summary>
    string FirebaseUid { get; }

    /// <summary>
    /// Fila de <c>Usuarios</c> del usuario actual. La primera vez que alguien
    /// inicia sesión se registra automáticamente.
    /// </summary>
    Task<Usuario> ObtenerAsync(CancellationToken ct = default);
}

public sealed class UsuarioActual(IHttpContextAccessor http, IUsuarioDao usuarios) : IUsuarioActual
{
    private Usuario? _cache; // una consulta por petición como máximo

    public string FirebaseUid =>
        http.HttpContext?.User.FindFirst("sub")?.Value
        ?? throw new InvalidOperationException("La petición no tiene un usuario autenticado.");

    public async Task<Usuario> ObtenerAsync(CancellationToken ct = default)
    {
        if (_cache is not null) return _cache;

        var uid = FirebaseUid;
        _cache = await usuarios.ObtenerPorFirebaseUidAsync(uid, ct);
        if (_cache is null)
        {
            var nombre = http.HttpContext?.User.FindFirst("name")?.Value;
            if (nombre is { Length: > 100 }) nombre = nombre[..100];
            _cache = await usuarios.ObtenerOCrearAsync(uid, nombre, ct);
        }
        return _cache;
    }
}

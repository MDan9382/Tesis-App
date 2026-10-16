using MathApp.Api.Data.Daos;
using MathApp.Api.Dtos;
using MathApp.Api.Models;
using MathApp.Api.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace MathApp.Api.Controllers;

/// <summary>Datos del usuario que inició sesión ("yo").</summary>
[ApiController]
[Route("api/yo")]
public class UsuarioController(
    IUsuarioActual usuarioActual,
    IUsuarioDao usuarios,
    IUsuarioPlanDao usuarioPlanes,
    IPlanEstudioDao planes,
    IProgresoLeccionDao progreso,
    IResultadoEjercicioDao resultadosEjercicios,
    IResultadoPruebaDao resultadosPruebas) : ControllerBase
{
    /// <summary>
    /// Devuelve el usuario actual. El frontend debe llamarlo justo después de
    /// iniciar sesión: la primera vez registra al usuario en la base de datos.
    /// </summary>
    [HttpGet]
    public Task<Usuario> Obtener(CancellationToken ct) => usuarioActual.ObtenerAsync(ct);

    [HttpPut("nombre")]
    public async Task<Usuario> CambiarNombre(ActualizarNombreRequest req, CancellationToken ct)
    {
        var u = await usuarioActual.ObtenerAsync(ct);
        var nombre = string.IsNullOrWhiteSpace(req.Nombre) ? null : req.Nombre.Trim();
        await usuarios.ActualizarNombreAsync(u.Id, nombre, ct);
        u.Nombre = nombre;
        return u;
    }

    /// <summary>Todo lo necesario para la página de perfil (requisito 7).</summary>
    [HttpGet("perfil")]
    public async Task<Perfil> Perfil(CancellationToken ct)
    {
        var u = await usuarioActual.ObtenerAsync(ct);
        return new Perfil(
            u,
            await usuarioPlanes.ObtenerPorUsuarioAsync(u.Id, soloActivos: true, ct),
            await progreso.ObtenerPorUsuarioAsync(u.Id, ct),
            await resultadosEjercicios.ObtenerPorUsuarioAsync(u.Id, ct),
            await resultadosPruebas.ObtenerPorUsuarioAsync(u.Id, ct));
    }

    [HttpGet("planes")]
    public async Task<List<UsuarioPlan>> MisPlanes([FromQuery] bool soloActivos = true, CancellationToken ct = default)
    {
        var u = await usuarioActual.ObtenerAsync(ct);
        return await usuarioPlanes.ObtenerPorUsuarioAsync(u.Id, soloActivos, ct);
    }

    [HttpPost("planes/{planId:int}")]
    public async Task<ActionResult<UsuarioPlan>> ActivarPlan(int planId, CancellationToken ct)
    {
        if (await planes.ObtenerPorIdAsync(planId, ct) is null) return NotFound();
        var u = await usuarioActual.ObtenerAsync(ct);
        var id = await usuarioPlanes.ActivarAsync(u.Id, planId, ct);
        return (await usuarioPlanes.ObtenerPorIdAsync(id, ct))!;
    }

    [HttpDelete("planes/{planId:int}")]
    public async Task<IActionResult> DesactivarPlan(int planId, CancellationToken ct)
    {
        var u = await usuarioActual.ObtenerAsync(ct);
        return await usuarioPlanes.DesactivarAsync(u.Id, planId, ct) ? NoContent() : NotFound();
    }
}

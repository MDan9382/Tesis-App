using System.ComponentModel.DataAnnotations;
using MathApp.Api.Data.Daos;
using MathApp.Api.Models;
using MathApp.Api.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace MathApp.Api.Controllers;

[ApiController]
[Route("api/lecciones")]
public class LeccionesController(
    ILeccionDao lecciones,
    IEjercicioDao ejercicios,
    IProgresoLeccionDao progreso,
    IUsuarioActual usuarioActual) : ControllerBase
{
    /// <summary>Búsqueda por nombre (requisito 8). Ej.: /api/lecciones/buscar?q=fracc</summary>
    [HttpGet("buscar")]
    public Task<List<LeccionResumen>> Buscar(
        [FromQuery, Required, MinLength(2), MaxLength(50)] string q, CancellationToken ct) =>
        lecciones.BuscarPorNombreAsync(q, 20, ct);

    /// <summary>Lección completa con su contenido.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Leccion>> Obtener(int id, CancellationToken ct) =>
        await lecciones.ObtenerPorIdAsync(id, ct) is { } l ? l : NotFound();

    /// <summary>Ejercicios de práctica de la lección.</summary>
    [HttpGet("{id:int}/ejercicios")]
    public async Task<ActionResult<List<Ejercicio>>> Ejercicios(int id, CancellationToken ct)
    {
        if (await lecciones.ObtenerPorIdAsync(id, ct) is null) return NotFound();
        return await ejercicios.ObtenerPorLeccionAsync(id, esPrueba: null, ct);
    }

    /// <summary>
    /// Marca la lección como completada (requisito 9). Si ya estaba completada
    /// actualiza la fecha, así se puede repetir.
    /// </summary>
    [HttpPost("{id:int}/completar")]
    public async Task<ActionResult<ProgresoLeccion>> Completar(int id, CancellationToken ct)
    {
        if (await lecciones.ObtenerPorIdAsync(id, ct) is null) return NotFound();
        var u = await usuarioActual.ObtenerAsync(ct);
        await progreso.MarcarCompletadaAsync(u.Id, id, ct);
        return (await progreso.ObtenerAsync(u.Id, id, ct))!;
    }
}

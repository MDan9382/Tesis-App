using MathApp.Api.Data.Daos;
using MathApp.Api.Dtos;
using MathApp.Api.Models;
using MathApp.Api.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace MathApp.Api.Controllers;

[ApiController]
[Route("api/ejercicios")]
public class EjerciciosController(
    IEjercicioDao ejercicios,
    IResultadoEjercicioDao resultados,
    IUsuarioActual usuarioActual) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Ejercicio>> Obtener(int id, CancellationToken ct) =>
        await ejercicios.ObtenerPorIdAsync(id, ct) is { } e ? e : NotFound();

    /// <summary>Guarda un intento del usuario actual (cada intento es una fila nueva).</summary>
    [HttpPost("{id:int}/resultados")]
    public async Task<ActionResult<ResultadoEjercicio>> Registrar(int id, RegistrarResultadoRequest req, CancellationToken ct)
    {
        if (await ejercicios.ObtenerPorIdAsync(id, ct) is null) return NotFound();
        var u = await usuarioActual.ObtenerAsync(ct);
        var r = new ResultadoEjercicio { Puntaje = req.Puntaje, EjercicioId = id, UsuarioId = u.Id };
        await resultados.RegistrarAsync(r, ct);
        return CreatedAtAction(nameof(MisResultados), new { id }, await resultados.ObtenerPorIdAsync(r.Id, ct));
    }

    /// <summary>Intentos del usuario actual en este ejercicio y su mejor puntaje.</summary>
    [HttpGet("{id:int}/resultados")]
    public async Task<ResumenIntentos<ResultadoEjercicio>> MisResultados(int id, CancellationToken ct)
    {
        var u = await usuarioActual.ObtenerAsync(ct);
        return new(await resultados.ObtenerMejorPuntajeAsync(u.Id, id, ct),
                   await resultados.ObtenerIntentosAsync(u.Id, id, ct));
    }
}

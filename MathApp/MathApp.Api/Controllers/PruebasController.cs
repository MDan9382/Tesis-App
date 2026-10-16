using MathApp.Api.Data.Daos;
using MathApp.Api.Dtos;
using MathApp.Api.Models;
using MathApp.Api.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace MathApp.Api.Controllers;

[ApiController]
[Route("api/pruebas")]
public class PruebasController(
    IPruebaDao pruebas,
    IResultadoPruebaDao resultados,
    IUsuarioActual usuarioActual) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Prueba>> Obtener(int id, CancellationToken ct) =>
        await pruebas.ObtenerPorIdAsync(id, ct) is { } p ? p : NotFound();

    /// <summary>Guarda un intento del usuario actual (cada intento es una fila nueva).</summary>
    [HttpPost("{id:int}/resultados")]
    public async Task<ActionResult<ResultadoPrueba>> Registrar(int id, RegistrarResultadoRequest req, CancellationToken ct)
    {
        if (await pruebas.ObtenerPorIdAsync(id, ct) is null) return NotFound();
        var u = await usuarioActual.ObtenerAsync(ct);
        var r = new ResultadoPrueba { Puntaje = req.Puntaje, PruebaId = id, UsuarioId = u.Id };
        await resultados.RegistrarAsync(r, ct);
        return CreatedAtAction(nameof(MisResultados), new { id }, await resultados.ObtenerPorIdAsync(r.Id, ct));
    }

    /// <summary>Intentos del usuario actual en esta prueba y su mejor puntaje.</summary>
    [HttpGet("{id:int}/resultados")]
    public async Task<ResumenIntentos<ResultadoPrueba>> MisResultados(int id, CancellationToken ct)
    {
        var u = await usuarioActual.ObtenerAsync(ct);
        return new(await resultados.ObtenerMejorPuntajeAsync(u.Id, id, ct),
                   await resultados.ObtenerIntentosAsync(u.Id, id, ct));
    }
}

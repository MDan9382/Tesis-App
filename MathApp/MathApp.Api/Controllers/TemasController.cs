using MathApp.Api.Data.Daos;
using MathApp.Api.Dtos;
using MathApp.Api.Models;
using MathApp.Api.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace MathApp.Api.Controllers;

[ApiController]
[Route("api/temas")]
public class TemasController(
    ITemaDao temas,
    ILeccionDao lecciones,
    IPruebaDao pruebas,
    IProgresoLeccionDao progreso,
    IUsuarioActual usuarioActual) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Tema>> Obtener(int id, CancellationToken ct) =>
        await temas.ObtenerPorIdAsync(id, ct) is { } t ? t : NotFound();

    /// <summary>
    /// Ruta de aprendizaje del tema (requisito 10): lecciones en orden, cada una
    /// marcada como completada o no para el usuario actual.
    /// </summary>
    [HttpGet("{id:int}/lecciones")]
    public async Task<ActionResult<List<LeccionEnRuta>>> Lecciones(int id, CancellationToken ct)
    {
        if (await temas.ObtenerPorIdAsync(id, ct) is null) return NotFound();
        var u = await usuarioActual.ObtenerAsync(ct);

        var lista = await lecciones.ObtenerPorTemaAsync(id, ct);
        var hechas = (await progreso.ObtenerPorUsuarioYTemaAsync(u.Id, id, ct))
            .ToDictionary(p => p.LeccionId, p => p.FechaCompletada);

        return lista.Select(l =>
        {
            hechas.TryGetValue(l.Id, out var fecha);
            return new LeccionEnRuta(l.Id, l.Nombre, l.Orden, fecha.HasValue, fecha);
        }).ToList();
    }

    /// <summary>Pruebas del tema.</summary>
    [HttpGet("{id:int}/pruebas")]
    public async Task<ActionResult<List<Prueba>>> Pruebas(int id, [FromQuery] bool? esPrueba, CancellationToken ct)
    {
        if (await temas.ObtenerPorIdAsync(id, ct) is null) return NotFound();
        return await pruebas.ObtenerPorTemaAsync(id, esPrueba, ct);
    }
}

using MathApp.Api.Data.Daos;
using MathApp.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace MathApp.Api.Controllers;

[ApiController]
[Route("api/planes")]
public class PlanesController(IPlanEstudioDao planes, ITemaDao temas) : ControllerBase
{
    [HttpGet]
    public Task<List<PlanEstudio>> Listar(CancellationToken ct) => planes.ObtenerTodosAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlanEstudio>> Obtener(int id, CancellationToken ct) =>
        await planes.ObtenerPorIdAsync(id, ct) is { } p ? p : NotFound();

    /// <summary>Temas del plan en el orden de la ruta de aprendizaje.</summary>
    [HttpGet("{id:int}/temas")]
    public async Task<ActionResult<List<Tema>>> Temas(int id, CancellationToken ct)
    {
        if (await planes.ObtenerPorIdAsync(id, ct) is null) return NotFound();
        return await temas.ObtenerPorPlanAsync(id, ct);
    }
}

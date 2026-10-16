using System.ComponentModel.DataAnnotations;

namespace MathApp.Api.Models;

/// <summary>Fila de la tabla <c>Temas</c>. Un plan de estudio tiene varios temas.</summary>
public class Tema
{
    /// <summary>Tema_Id (PK, AUTO_INCREMENT).</summary>
    public int Id { get; set; }

    /// <summary>Tema_Nombre VARCHAR(25) NOT NULL.</summary>
    [Required, StringLength(25)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Tema_Orden INT NOT NULL DEFAULT 0. Posición dentro del plan (ruta de aprendizaje).</summary>
    public int Orden { get; set; }

    /// <summary>Plan_Id (FK → Planes_Estudio).</summary>
    public int PlanId { get; set; }
}

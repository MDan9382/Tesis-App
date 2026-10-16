using System.ComponentModel.DataAnnotations;

namespace MathApp.Api.Models;

/// <summary>Fila de la tabla <c>Planes_Estudio</c>.</summary>
public class PlanEstudio
{
    /// <summary>Plan_Id (PK, AUTO_INCREMENT).</summary>
    public int Id { get; set; }

    /// <summary>Plan_Nombre VARCHAR(35) NOT NULL.</summary>
    [Required, StringLength(35)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Plan_Descripcion VARCHAR(100) NOT NULL.</summary>
    [Required, StringLength(100)]
    public string Descripcion { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace MathApp.Api.Models;

/// <summary>Fila completa de la tabla <c>Lecciones</c>, incluido el contenido.</summary>
public class Leccion
{
    /// <summary>Leccion_Id (PK, AUTO_INCREMENT).</summary>
    public int Id { get; set; }

    /// <summary>Leccion_Nombre VARCHAR(25) NOT NULL.</summary>
    [Required, StringLength(25)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Leccion_Contenido LONGTEXT NOT NULL.</summary>
    [Required]
    public string Contenido { get; set; } = string.Empty;

    /// <summary>Leccion_Orden INT NOT NULL DEFAULT 0. Posición dentro del tema (ruta de aprendizaje).</summary>
    public int Orden { get; set; }

    /// <summary>Tema_Id (FK → Temas).</summary>
    public int TemaId { get; set; }
}

/// <summary>
/// Versión ligera de una lección (sin <see cref="Leccion.Contenido"/>) para listados
/// y búsquedas, así no se transfiere el LONGTEXT cuando no hace falta.
/// </summary>
public class LeccionResumen
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public int TemaId { get; set; }
}

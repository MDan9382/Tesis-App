namespace MathApp.Api.Models;

/// <summary>
/// Fila de la tabla <c>Pruebas</c>. Una prueba evalúa un tema completo; su contenido
/// (los ejercicios) se guarda en Markdown y el frontend lo presenta y controla el tiempo.
/// </summary>
public class Prueba
{
    /// <summary>Prueba_Id (PK, AUTO_INCREMENT).</summary>
    public int Id { get; set; }

    /// <summary>Prueba_Contenido LONGTEXT NULL (Markdown).</summary>
    public string? Contenido { get; set; }

    /// <summary>Prueba_EsPrueba BOOLEAN NOT NULL DEFAULT FALSE.</summary>
    public bool EsPrueba { get; set; }

    /// <summary>Tema_Id (FK → Temas).</summary>
    public int TemaId { get; set; }
}

namespace MathApp.Api.Models;

/// <summary>Fila de la tabla <c>Ejercicios</c>. Un ejercicio pertenece a una lección.</summary>
public class Ejercicio
{
    /// <summary>Ejercicio_Id (PK, AUTO_INCREMENT).</summary>
    public int Id { get; set; }

    /// <summary>
    /// Ejercicio_Contenido LONGTEXT NULL. Se recomienda guardar aquí un JSON con la
    /// plantilla del ejercicio (enunciado, rangos de valores, fórmula de la respuesta)
    /// para poder generar valores distintos en cada intento.
    /// </summary>
    public string? Contenido { get; set; }

    /// <summary>Ejercicio_EsPrueba BOOLEAN NOT NULL DEFAULT FALSE.</summary>
    public bool EsPrueba { get; set; }

    /// <summary>Leccion_Id (FK → Lecciones).</summary>
    public int LeccionId { get; set; }
}

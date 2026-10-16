namespace MathApp.Api.Models;

/// <summary>
/// Fila de la tabla <c>Resultados_Ejercicios</c>. Cada intento de un ejercicio o
/// prueba genera una fila nueva, así se conserva el historial de reintentos.
/// </summary>
public class ResultadoEjercicio
{
    /// <summary>RE_Id (PK, AUTO_INCREMENT).</summary>
    public int Id { get; set; }

    /// <summary>RE_Puntaje INT NULL.</summary>
    public int? Puntaje { get; set; }

    /// <summary>RE_Fecha_Completado DATETIME NULL.</summary>
    public DateTime? FechaCompletado { get; set; }

    /// <summary>Ejercicio_Id (FK → Ejercicios).</summary>
    public int EjercicioId { get; set; }

    /// <summary>Usuario_Id (FK → Usuarios).</summary>
    public int UsuarioId { get; set; }
}

namespace MathApp.Api.Models;

/// <summary>
/// Fila de la tabla <c>Resultados_Pruebas</c>. Cada intento genera una fila nueva,
/// así se conserva el historial de reintentos.
/// </summary>
public class ResultadoPrueba
{
    /// <summary>RP_Id (PK, AUTO_INCREMENT).</summary>
    public int Id { get; set; }

    /// <summary>RP_Puntaje INT NULL.</summary>
    public int? Puntaje { get; set; }

    /// <summary>RP_Fecha_Completado DATETIME NULL.</summary>
    public DateTime? FechaCompletado { get; set; }

    /// <summary>Prueba_Id (FK → Pruebas).</summary>
    public int PruebaId { get; set; }

    /// <summary>Usuario_Id (FK → Usuarios).</summary>
    public int UsuarioId { get; set; }
}

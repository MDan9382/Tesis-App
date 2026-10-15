namespace MathApp.Api.Models;

/// <summary>Fila de la tabla <c>Progreso_Lecciones</c>: lecciones tomadas por un usuario.</summary>
public class ProgresoLeccion
{
    /// <summary>PL_Id (PK, AUTO_INCREMENT).</summary>
    public int Id { get; set; }

    /// <summary>PL_Fecha_Completada DATETIME NULL. <c>null</c> = iniciada pero no completada.</summary>
    public DateTime? FechaCompletada { get; set; }

    /// <summary>Usuario_Id (FK → Usuarios).</summary>
    public int UsuarioId { get; set; }

    /// <summary>Leccion_Id (FK → Lecciones).</summary>
    public int LeccionId { get; set; }

    public bool Completada => FechaCompletada.HasValue;
}

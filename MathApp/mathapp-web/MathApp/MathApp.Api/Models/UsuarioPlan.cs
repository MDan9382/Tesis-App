namespace MathApp.Api.Models;

/// <summary>Fila de la tabla <c>Usuario_Planes</c>: planes que un usuario ha activado.</summary>
public class UsuarioPlan
{
    /// <summary>UP_Id (PK, AUTO_INCREMENT).</summary>
    public int Id { get; set; }

    /// <summary>UP_Fecha_Activacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP.</summary>
    public DateTime FechaActivacion { get; set; }

    /// <summary>UP_Activo BOOLEAN NOT NULL DEFAULT TRUE (ver sugerencias_bd.sql).</summary>
    public bool Activo { get; set; }

    /// <summary>Plan_Id (FK → Planes_Estudio).</summary>
    public int PlanId { get; set; }

    /// <summary>Usuario_Id (FK → Usuarios).</summary>
    public int UsuarioId { get; set; }
}

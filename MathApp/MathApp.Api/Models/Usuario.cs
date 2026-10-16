using System.ComponentModel.DataAnnotations;

namespace MathApp.Api.Models;

/// <summary>Fila de la tabla <c>Usuarios</c>.</summary>
/// <remarks>
/// Las credenciales las maneja Firebase; aquí solo se guarda el UID para
/// relacionar al usuario autenticado con su progreso.
/// </remarks>
public class Usuario
{
    /// <summary>Usuario_Id (PK, AUTO_INCREMENT).</summary>
    public int Id { get; set; }

    /// <summary>Usuario_FirebaseUID VARCHAR(128) NOT NULL, UNIQUE.</summary>
    [Required, StringLength(128)]
    public string FirebaseUid { get; set; } = string.Empty;

    /// <summary>Usuario_Nombre VARCHAR(100) NULL.</summary>
    [StringLength(100)]
    public string? Nombre { get; set; }
}

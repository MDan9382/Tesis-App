using System.ComponentModel.DataAnnotations;
using MathApp.Api.Models;

namespace MathApp.Api.Dtos;

/// <summary>Cuerpo para cambiar el nombre visible del usuario.</summary>
public record ActualizarNombreRequest(
    [StringLength(100)] string? Nombre);

/// <summary>
/// Cuerpo para registrar el resultado de un ejercicio o prueba. No incluye el
/// usuario: se toma del token.
/// </summary>
public record RegistrarResultadoRequest(
    [Required, Range(0, 100)] int? Puntaje);

/// <summary>Lección dentro de la ruta de aprendizaje, con el estado del usuario.</summary>
public record LeccionEnRuta(int Id, string Nombre, int Orden, bool Completada, DateTime? FechaCompletada);

/// <summary>Historial de intentos de un ejercicio o prueba.</summary>
public record ResumenIntentos<T>(int? MejorPuntaje, List<T> Intentos);

/// <summary>Datos para la página de perfil.</summary>
public record Perfil(
    Usuario Usuario,
    List<UsuarioPlan> Planes,
    List<ProgresoLeccion> Lecciones,
    List<ResultadoEjercicio> Ejercicios,
    List<ResultadoPrueba> Pruebas);

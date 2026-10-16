using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace MathApp.Api.Infraestructura;

/// <summary>
/// Convierte las excepciones no controladas en respuestas JSON (ProblemDetails)
/// y las registra en el log. Los errores de restricciones de MySQL se traducen
/// a códigos HTTP útiles para el frontend en lugar de un 500 genérico.
/// Nunca se envía al cliente el mensaje interno de la excepción.
/// </summary>
public sealed class ManejadorErrores(ILogger<ManejadorErrores> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception ex, CancellationToken ct)
    {
        var (estado, titulo) = ex switch
        {
            MySqlException { ErrorCode: MySqlErrorCode.DuplicateKeyEntry } =>
                (StatusCodes.Status409Conflict, "El registro ya existe."),
            MySqlException { ErrorCode: MySqlErrorCode.RowIsReferenced2 } =>
                (StatusCodes.Status409Conflict, "No se puede eliminar: hay registros que dependen de este."),
            MySqlException { ErrorCode: MySqlErrorCode.NoReferencedRow2 } =>
                (StatusCodes.Status400BadRequest, "Se hace referencia a un registro que no existe."),
            OperationCanceledException when ct.IsCancellationRequested =>
                (499, "Solicitud cancelada por el cliente."),
            _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado."),
        };

        if (estado >= 500)
            logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", http.Request.Method, http.Request.Path);
        else
            logger.LogWarning("{Titulo} ({Metodo} {Ruta}): {Mensaje}", titulo, http.Request.Method, http.Request.Path, ex.Message);

        http.Response.StatusCode = estado;
        await http.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = estado,
            Title = titulo,
            Instance = http.Request.Path,
        }, ct);
        return true;
    }
}

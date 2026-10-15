using System.Data.Common;

namespace MathApp.Api.Data;

/// <summary>Lectura de columnas por nombre, manejando valores NULL.</summary>
internal static class DataReaderExtensions
{
    public static int GetInt32(this DbDataReader r, string columna) =>
        r.GetInt32(r.GetOrdinal(columna));

    public static int? GetNullableInt32(this DbDataReader r, string columna)
    {
        var i = r.GetOrdinal(columna);
        return r.IsDBNull(i) ? null : r.GetInt32(i);
    }

    public static string GetString(this DbDataReader r, string columna) =>
        r.GetString(r.GetOrdinal(columna));

    public static string? GetNullableString(this DbDataReader r, string columna)
    {
        var i = r.GetOrdinal(columna);
        return r.IsDBNull(i) ? null : r.GetString(i);
    }

    public static bool GetBoolean(this DbDataReader r, string columna) =>
        r.GetBoolean(r.GetOrdinal(columna));

    public static bool? GetNullableBoolean(this DbDataReader r, string columna)
    {
        var i = r.GetOrdinal(columna);
        return r.IsDBNull(i) ? null : r.GetBoolean(i);
    }

    public static DateTime GetDateTime(this DbDataReader r, string columna) =>
        r.GetDateTime(r.GetOrdinal(columna));

    public static DateTime? GetNullableDateTime(this DbDataReader r, string columna)
    {
        var i = r.GetOrdinal(columna);
        return r.IsDBNull(i) ? null : r.GetDateTime(i);
    }
}

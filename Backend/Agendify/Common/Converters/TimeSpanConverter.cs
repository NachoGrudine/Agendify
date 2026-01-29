using System.Text.Json;
using System.Text.Json.Serialization;

namespace Agendify.Common.Converters;

/// <summary>
/// Converter personalizado para TimeSpan que maneja el formato "HH:mm:ss" o "HH:mm".
///
/// System.Text.Json NO soporta TimeSpan por defecto desde strings.
/// Este converter permite deserializar strings como "09:00:00" o "09:00" a TimeSpan.
/// </summary>
public class TimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var timeString = reader.GetString();

        if (string.IsNullOrEmpty(timeString))
        {
            return TimeSpan.Zero;
        }

        // Intentar parsear en diferentes formatos
        // Formato 1: "HH:mm:ss" (ej: "09:00:00")
        // Formato 2: "HH:mm" (ej: "09:00")
        // Formato 3: "d.HH:mm:ss" (ej: "0.09:00:00") - formato ISO de TimeSpan

        if (TimeSpan.TryParse(timeString, out var result))
        {
            return result;
        }

        // Si tiene formato "HH:mm", agregar segundos
        if (timeString.Length == 5 && timeString.Contains(':'))
        {
            if (TimeSpan.TryParse(timeString + ":00", out result))
            {
                return result;
            }
        }

        throw new JsonException($"No se pudo parsear TimeSpan: {timeString}");
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        // Serializar en formato "HH:mm:ss" para consistencia
        writer.WriteStringValue(value.ToString(@"hh\:mm\:ss"));
    }
}

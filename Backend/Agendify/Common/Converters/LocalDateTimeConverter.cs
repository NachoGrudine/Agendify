using System.Text.Json;
using System.Text.Json.Serialization;

namespace Agendify.Common.Converters;

/// <summary>
/// Converter personalizado para DateTime que trata todas las fechas como hora local.
/// Evita conversiones UTC no deseadas para sistemas de turnos locales.
///
/// - Al deserializar: Si viene con "Z" (UTC), convierte a hora local. Si no tiene zona, la toma como local.
/// - Al serializar: Siempre serializa sin indicador de zona horaria (formato ISO sin Z).
/// </summary>
public class LocalDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();

        if (string.IsNullOrEmpty(dateString))
        {
            return DateTime.MinValue;
        }

        // Parsear la fecha
        if (DateTime.TryParse(dateString, out var parsedDate))
        {
            // Si la fecha viene como UTC (termina en Z o tiene offset), convertir a local
            if (parsedDate.Kind == DateTimeKind.Utc)
            {
                return parsedDate.ToLocalTime();
            }

            // Si viene como Local o Unspecified, asumir que ya es hora local
            return DateTime.SpecifyKind(parsedDate, DateTimeKind.Local);
        }

        throw new JsonException($"No se pudo parsear la fecha: {dateString}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Serializar siempre en formato local sin indicador de zona
        // Formato: yyyy-MM-ddTHH:mm:ss (sin Z)
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}

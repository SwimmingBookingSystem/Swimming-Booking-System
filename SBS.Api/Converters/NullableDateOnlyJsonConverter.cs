using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SBS.Api.Converters;

public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            if (DateOnly.TryParse(value, out var date))
            {
                return date;
            }
            
            // Trả về DateOnly.MaxValue để biểu thị ngày sai định dạng (tránh throw JsonException gây ngắt debugger)
            return DateOnly.MaxValue;
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return DateOnly.MaxValue;
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd"));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

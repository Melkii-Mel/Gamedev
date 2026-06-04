using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utils.IO.SerializerConverters;

public class SmartReadConverter : JsonConverter<object>
{
    public delegate object? Converter(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options);

    private readonly List<Converter> _alternatives;

    public SmartReadConverter(IEnumerable<Converter> alternatives)
    {
        _alternatives = [..alternatives];
    }

    public override bool CanConvert(Type typeToConvert) => true; 

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var readerCopy = reader;

        try
        {
            return JsonSerializer.Deserialize(ref reader, typeToConvert, options);
        }
        catch
        {
            reader = readerCopy; 
        }

        foreach (var alt in _alternatives)
        {
            try
            {
                return alt(ref reader, typeToConvert, options);
            }
            catch
            {
                reader = readerCopy; 
            }
        }

        throw new JsonException($"Could not deserialize JSON to {typeToConvert}");
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}

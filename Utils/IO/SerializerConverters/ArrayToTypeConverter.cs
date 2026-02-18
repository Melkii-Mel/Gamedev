using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utils.IO.SerializerConverters;

public static class ArrayToType
{
    public static object Deserialize(ref Utf8JsonReader reader, Type targetType, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected JSON array");

        var ctor = targetType.GetConstructors()[0];
        var parameters = ctor.GetParameters();
        var args = new object?[parameters.Length];

        reader.Read();
        for (var i = 0; i < parameters.Length; i++)
        {
            args[i] = JsonSerializer.Deserialize(ref reader, parameters[i].ParameterType, options);
            reader.Read();
        }

        if (reader.TokenType != JsonTokenType.EndArray)
            throw new JsonException("Expected EndArray");

        return ctor.Invoke(args);
    }
}

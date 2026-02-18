using System;
using System.Text.Json;
using Utils.IO.SerializerConverters;

namespace Utils.IO;

public static class Serializer
{
    /// <summary>
    ///     Optimized for saving SaveData defined as simple `record` DTOs.
    /// </summary>
    public static JsonSerializerOptions DefaultSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            IncludeFields = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
        };
    }

    public static string Serialize<T>(T value, JsonSerializerOptions? optionsOverride = null) where T : notnull
    {
        return JsonSerializer.Serialize(value, optionsOverride ?? DefaultSerializerOptions());
    }

    public static T? Deserialize<T>(string json, SmartReadConverter.Converter[]? converters = null,
        JsonSerializerOptions? optionsOverride = null) where T : notnull
    {
        var options = optionsOverride ?? DefaultSerializerOptions();
        if (converters != null) options.Converters.Add(new SmartReadConverter(converters));

        return JsonSerializer.Deserialize<T>(json, optionsOverride ?? DefaultSerializerOptions());
    }
}

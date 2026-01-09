using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utils.IO;

public static class Serializer
{
    /// <summary>
    /// Optimized for saving save data defined as simple `record` DTOs.
    /// </summary>
    public static JsonSerializerOptions DefaultSerializerOptions = new()
    {
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = true,
    };

    public static string Serialize<T>(T value, JsonSerializerOptions? optionsOverride = null) where T : notnull
    {
        return JsonSerializer.Serialize(value, optionsOverride ?? DefaultSerializerOptions);
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, DefaultSerializerOptions);
    }
}

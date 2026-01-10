using System.Text.Json;

namespace Utils.IO;

public static class Serializer
{
    /// <summary>
    ///     Optimized for saving SaveData defined as simple `record` DTOs.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        IncludeFields = true,
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

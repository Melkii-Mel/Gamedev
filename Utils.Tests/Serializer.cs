using Newtonsoft.Json;

namespace Utils.Tests;

public static class Serializer
{
    public static string Serialize(object? value)
    {
        return JsonConvert.SerializeObject(value,
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
    }
}

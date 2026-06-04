using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Utils.IO.SerializerConverters;

namespace Utils.IO;

public class GameDataLoader
{
    public static T LoadData<T>(string fileName, SmartReadConverter.Converter[]? converters = null,
        JsonSerializerOptions? optionsOverride = null) where T : notnull
    {
        return Serializer.Deserialize<T>(FileLoader.LoadTextFile(fileName) ?? throw LoadException(), converters,
                   optionsOverride) ??
               throw LoadException();

        Exception LoadException() =>
            new IOException($"Failed to load game data file: {Path.Combine(AppContext.BaseDirectory, fileName)}");
    }

    public static IEnumerable<(string fileName, T data)> LoadAllFromDirectory<T>(string directory,
        Func<string, bool> isValidFile,
        SmartReadConverter.Converter[]? converters = null,
        JsonSerializerOptions? optionsOverride = null) where T : notnull
    {
        var files = Directory.EnumerateFiles(directory);
        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);
            if (isValidFile(fileName))
                yield return (fileName, LoadData<T>(filePath, converters, optionsOverride));
        }
    }
}

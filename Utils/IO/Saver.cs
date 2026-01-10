using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utils.IO;

/// <summary>
///     Saver saves saves
/// </summary>
public record Saver(
    string AppName,
    string? CompanyName = null,
    string? SaveDirectory = "Save",
    bool Backup = true,
    string? BackupFileExtension = ".bak"
)
{
    private const string DefaultSaveName = "save.json";

    public void Save<T>(T data, string saveName = DefaultSaveName) where T : notnull
    {
        var json = Serializer.Serialize(data);
        var filePath = PrepareSavePath(saveName);
        File.WriteAllText(filePath, json);
        if (Backup) CopyToBackup(filePath);
    }

    public T? Load<T>(string saveName = DefaultSaveName) where T : notnull
    {
        var savePath = PrepareSavePath(saveName);
        if (!File.Exists(savePath)) return default;
        var exceptions = new List<Exception>();
        try
        {
            var data = LoadFileIfExists<T>(savePath);
            if (Backup && data != null) CopyToBackup(savePath);
            return data;
        }
        catch (Exception e)
        {
            exceptions.Add(e);
        }

        if (Backup)
            try
            {
                return LoadFileIfExists<T>(savePath + BackupFileExtension);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }

        if (exceptions.Count == 0) return default;

        throw new AggregateException(
            "Could not load neither a save file nor a backup file, multiple exceptions occurred.",
            exceptions
        );
    }

    private void CopyToBackup(string filePath)
    {
        File.Copy(filePath, filePath + BackupFileExtension);
    }

    private static T? LoadFileIfExists<T>(string fileName) where T : notnull
    {
        if (!File.Exists(fileName)) return default;
        var json = File.ReadAllText(fileName);
        return Serializer.Deserialize<T>(json);
    }

    private string PrepareSavePath(string saveName)
    {
        ThrowIfNullOrEmpty(saveName, nameof(saveName));
        var dirPath = BuildDir();
        return Path.Combine(dirPath, saveName);
    }

    private string BuildDir()
    {
        ThrowIfNullOrEmpty(AppName, nameof(AppName));
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return CombinePathNullable(appData, CompanyName, AppName, SaveDirectory);
    }

    private static string CombinePathNullable(string basePath, params string?[] args)
    {
        return args.Where(arg => !string.IsNullOrEmpty(arg)).Aggregate(basePath, Path.Combine);
    }

    private static void ThrowIfNullOrEmpty(string path, string varName)
    {
        if (string.IsNullOrEmpty(path)) throw new InvalidOperationException($"{varName} must not be null or empty.");
    }
}

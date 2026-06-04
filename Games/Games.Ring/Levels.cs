using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils.Collections.AdaptiveCollectionInternals;
using Utils.IO;
using Utils.IO.SerializerConverters;

namespace Games.Ring;

public static class Levels
{
    private static readonly Dictionary<string, Level> IdLevelMap = [];
    private static readonly Dictionary<string, DateTime> LastWriteTimes = [];
    private static readonly SwapAndPopDictList<string> LoadedIds = [];

    public static IEnumerable<Level> LoadAll()
    {
        foreach (var (fileName, data) in GameDataLoader.LoadAllFromDirectory<Level>(Game.Data.LevelsDirectory,
                     s => System.IO.Path.GetExtension(s) == ".json", [ArrayToType.Deserialize]).OrderBy(p =>
                     p.fileName.Substring(0, p.fileName.IndexOf('.'))))
        {
            var id = fileName.Substring(0,
                fileName.IndexOf('.', fileName.IndexOf('.') + 1));
            yield return Reload(id);
        }
    }

    public static Level[] GetAllLoaded()
    {
        return LoadedIds.Select(id => IdLevelMap[id]).ToArray();
    }

    public static string Path(string id)
    {
        return System.IO.Path.Combine(Game.Data.LevelsDirectory, id + ".json");
    }

    public static bool IsLoaded(string id)
    {
        return LastWriteTimes.ContainsKey(id);
    }

    public static bool HasChanged(string id)
    {
        return LastWriteTimes[id] == File.GetLastWriteTime(Path(id));
    }

    public static Level Reload(string id)
    {
        var path = Path(id);
        var level = GameDataLoader.LoadData<Level>(path, [ArrayToType.Deserialize]);
        var lastWrite = File.GetLastWriteTime(path);
        LastWriteTimes[id] = lastWrite;
        if (!LoadedIds.Contains(id)) LoadedIds.Add(id);
        IdLevelMap[id] = level;
        return level;
    }

    public static Level GetFresh(string id)
    {
        if (!IsLoaded(id) || HasChanged(id))
            Reload(id);
        return IdLevelMap[id];
    }
}

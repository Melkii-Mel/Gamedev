using System;
using System.Collections.Generic;
using System.IO;
using LanguageExt;
using Utils.IO;
using static LanguageExt.Prelude;

// TODO: When abstracting: abstract file loading to default to the application root; 

namespace Games.Ring;

public class Directory<TItem, TIndex> where TItem : notnull where TIndex : notnull
{
    public TIndex? Header { get; private set; }

    public List<Either<Directory<TItem, TIndex>, TItem>> Items { get; private set; } = [];
    public Utils.Collections.BiMap<string, TItem> IdItemBiMap { get; private set; } = new();

    public static Directory<TItem, TIndex> LoadFromFileSystem(string root, string indexFileName = "_index.json",
        Func<string, TItem>? loadItem = default, Func<string, TIndex>? loadIndex = default,
        Func<TItem, string>? getItemId = default)
    {
        loadItem ??= f => GameDataLoader.LoadData<TItem>(f);
        loadIndex ??= f => GameDataLoader.LoadData<TIndex>(f);

        var directory = new Directory<TItem, TIndex>();
        var files = Directory.GetFiles(root);

        foreach (var file in files)
        {
            if (Path.GetFileName(file) == indexFileName)
                directory.Header = loadIndex(file);
            var item = loadItem(file);
            directory.Items.Add(Right(item));
            var id = getItemId != null ? getItemId(item) : Path.GetFileNameWithoutExtension(file);
            directory.IdItemBiMap.Add(id, item);
        }

        foreach (var dir in Directory.GetDirectories(root))
        {
            directory.Items.Add(Left(LoadFromFileSystem(dir, indexFileName, loadItem, loadIndex)));
        }

        return directory;
    }
}

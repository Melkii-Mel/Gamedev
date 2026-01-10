using System.Collections.Generic;
using System.Linq;
using Silk.NET.Maths;
using Utils;
using Utils.DataStructures;
using Utils.IO;

namespace Gamedev.Resources;

#region API

public interface IResources
{
    ITextureLoader TextureLoader { get; }
}

public interface ITextureLoader
{
    ITexture FromBytes(byte[] bytes);
}

public interface ITexture
{
    Vector2D<int> Size { get; }
}

#endregion

#region Wrapper

public class Resources
{
    public Resources(IResources resources)
    {
        TextureLoader = new TextureLoader(resources.TextureLoader);
        FontRegistry = new FontRegistry();
    }

    public TextureLoader TextureLoader { get; }
    public FontRegistry FontRegistry { get; }
}

public class FontRegistry
{
    private readonly MultiIndexStore<string, FontData> _multiIndexStore = new();
    private readonly Index<string, FontData> _pathIndex;

    public FontRegistry()
    {
        _pathIndex = _multiIndexStore.AddOneToManyIndexFixed(fontData => fontData.Path);
    }

    public FontData DefaultFont { get; } = new("Arial", "./Assets/Fonts/Arial.ttf");

    public void Add(FontData fontData)
    {
        _multiIndexStore.Add(fontData);
    }

    public void Add(IEnumerable<FontData> fontData)
    {
        foreach (var item in fontData) _multiIndexStore.Add(item);
    }

    public FontData? ByName(string name)
    {
        return _multiIndexStore.PrimaryIndex.TryGetValue(name, out var value) ? value : null;
    }

    public FontData? ByPath(string path)
    {
        return _pathIndex[path].FirstOrDefault();
    }
}

public record FontData(string Name, string Path) : IHasId<string>
{
    public string Id => Name;
}

public class TextureLoader
{
    private readonly ITextureLoader _textureLoader;

    public TextureLoader(ITextureLoader textureLoader)
    {
        _textureLoader = textureLoader;
    }

    public ITexture? FromFileName(string path, bool shouldCache = true, bool tryGetCached = true)
    {
        return Cacher.GetOrCache(path, shouldCache, tryGetCached, () =>
        {
            var bytes = FileLoader.LoadByteFile(path);
            return bytes == null ? null : _textureLoader.FromBytes(bytes);
        });
    }
}

#endregion

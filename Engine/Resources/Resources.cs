using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Silk.NET.Maths;
using Utils;
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
    public Resources(ITextureLoader textureLoader)
    {
        TextureLoader = new TextureLoader(textureLoader);
    }

    public TextureLoader TextureLoader { get; }
}

public class TextureLoader
{
    private readonly ITextureLoader _textureLoader;

    public TextureLoader(ITextureLoader textureLoader)
    {
        _textureLoader = textureLoader;
    }

    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

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

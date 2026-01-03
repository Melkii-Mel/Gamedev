using System;
using Microsoft.Extensions.Caching.Memory;

namespace Utils;

public static class Cacher
{
    private static readonly MemoryCache Cache = new(new MemoryCacheOptions());

    public static T? GetOrCache<T>(string cacheKey, bool shouldCache, bool tryGetCached, Func<T> provider)
    {
        if (tryGetCached)
        {
            var cached = Cache.Get<T>(cacheKey);
            if (cached != null) return cached;
        }

        var result = provider();

        if (shouldCache)
        {
            Cache.Set(cacheKey, result);
        }

        return result;
    }
}

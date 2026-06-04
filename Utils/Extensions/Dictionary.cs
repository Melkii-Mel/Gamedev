using System;
using System.Collections.Generic;

namespace Utils.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetOrInit<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue initialValue)
    {
        if (dict.TryGetValue(key, out var value)) return value;
        value = initialValue;
        dict[key] = value;
        return value;
    }

    public static TValue GetOrInit<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key,
        Func<TValue> valueInitializer)
    {
        if (dict.TryGetValue(key, out var value)) return value;
        value = valueInitializer();
        dict[key] = value;
        return value;
    }

    public static List<TValue> GetOrInit<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key)
    {
        if (dict.TryGetValue(key, out var value)) return value;
        value = [];
        dict[key] = value;
        return value;
    }
}
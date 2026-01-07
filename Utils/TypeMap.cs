using System.Collections.Generic;
using System;

namespace Utils;

public class TypeMap
{
    private readonly Dictionary<Type, object> _map = [];

    public T Set<T>(T value) where T : notnull
    {
        _map.Add(typeof(T), value);
        return value;
    }

    public T? TryGet<T>() where T : notnull
    {
        return _map.TryGetValue(typeof(T), out var value) ? (T)value : default;
    }

    public T GetOrCreate<T>(Func<T> callback) where T : notnull
    {
        return TryGet<T>() ?? Set(callback());
    }

    public T GetOrCreate<T>() where T : notnull, new()
    {
        return GetOrCreate(() => new T());
    }
}
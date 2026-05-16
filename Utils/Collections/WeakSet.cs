using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Utils.Collections;

public sealed class WeakSet<T> : IEnumerable<T>
    where T : class
{
    private readonly HashSet<WeakReference<T>> _set = [];
    private readonly ConditionalWeakTable<T, WeakReference<T>> _table
        = new();

    public void Add(T item)
    {
        var wr = new WeakReference<T>(item);
        _table.GetValue(item, _ => wr);
        _set.Add(wr);
    }

    public bool Contains(T item)
    {
        return _table.TryGetValue(item, out _);
    }

    public void Remove(T item)
    {
        _set.Remove(_table.GetOrCreateValue(item));
        _table.Remove(item);
    }

    public IEnumerator<T> GetEnumerator()
    {
        var dead = new List<WeakReference<T>>();
        foreach (var wr in _set)
        {
            if (wr.TryGetTarget(out var t)) yield return t;
            else dead.Add(wr);
        }

        foreach (var wr in dead) _set.Remove(wr);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

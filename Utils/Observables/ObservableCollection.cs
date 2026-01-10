using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Observables;

public class ObservableCollection<T> : ICollection<T>
{
    private List<T> _values;

    public ObservableCollection()
    {
        _values = [];
    }

    public ObservableCollection(IList<T> values)
    {
        _values = [.. values];
    }

    public int Count => _values.Count;

    public bool IsReadOnly => false;

    public void Add(T item)
    {
        _values.Add(item);
        CollectionChanged?.Invoke(new CollectionChangedEventArgs<T>(null, [item]));
    }

    public bool Remove(T item)
    {
        var removed = _values.Remove(item);
        if (removed) CollectionChanged?.Invoke(new CollectionChangedEventArgs<T>([item], null));
        return removed;
    }

    public void Clear()
    {
        var values = _values;
        _values = [];
        CollectionChanged?.Invoke(new CollectionChangedEventArgs<T>(null, values));
    }

    public bool Contains(T item)
    {
        return _values.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _values.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public event Action<CollectionChangedEventArgs<T>>? CollectionChanged;

    public void Add(IEnumerable<T> items)
    {
        var collection = items as T[] ?? items.ToArray();
        _values.AddRange(collection);
        CollectionChanged?.Invoke(new CollectionChangedEventArgs<T>(null, collection));
    }

    public IEnumerable<T> Remove(IEnumerable<T> items)
    {
        var values = YieldRemove().ToList();
        CollectionChanged?.Invoke(new CollectionChangedEventArgs<T>(null, values));
        return values;

        IEnumerable<T> YieldRemove()
        {
            foreach (var item in items)
                if (_values.Remove(item))
                    yield return item;
        }
    }
}

public record CollectionChangedEventArgs<T>(IEnumerable<T>? Old, IEnumerable<T>? New);

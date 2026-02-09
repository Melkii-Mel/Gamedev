using System.Collections;
using System.Collections.Generic;

namespace Utils.Collections.AdaptiveCollectionInternals;

public partial class SwapAndPopDictList<T> : IList<T>
{
    private readonly List<T> _list;
    private readonly Dictionary<T, int> _dictionary;

    public SwapAndPopDictList()
    {
        _list = [];
        _dictionary = [];
    }

    public SwapAndPopDictList(IEnumerable<T> values)
    {
        _list = [..values];
        _dictionary = new Dictionary<T, int>(_list.Count);
        for (var i = 0; i < _list.Count; i++)
        {
            _dictionary[_list[i]] = i;
        }
    }

    public SwapAndPopDictList(int size)
    {
        _list = new List<T>(size);
        _dictionary = new Dictionary<T, int>(size);
    }

    public void Add(T item)
    {
        _list.Add(item);
        _dictionary[item] = _list.Count - 1;
    }

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Clear()
    {
        _list.Clear();
        _dictionary.Clear();
    }

    public bool Contains(T item)
    {
        return _dictionary.ContainsKey(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        if (!_dictionary.TryGetValue(item, out var itemIndex)) return false;
        var lastIndex = _list.Count - 1;
        var last = _list[lastIndex];
        _list[itemIndex] = last;
        _dictionary[last] = itemIndex;
        _list.RemoveAt(lastIndex);
        return true;
    }

    public int Count => _list.Count;
    public bool IsReadOnly => false;

    public int IndexOf(T item) => !_dictionary.TryGetValue(item, out var i) ? -1 : i;

    public void Insert(int index, T item)
    {
        Add(item);
    }

    public void RemoveAt(int index)
    {
        Remove(_list[index]);
    }

    public T this[int index]
    {
        get => _list[index];
        set
        {
            var old = _list[index];
            _list[index] = value;
            _dictionary.Remove(old);
            _dictionary[value] = index;
        }
    }
}

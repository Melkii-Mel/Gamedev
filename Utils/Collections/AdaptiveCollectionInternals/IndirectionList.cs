using System;
using System.Collections.Generic;
using Attributes;

namespace Utils.Collections.AdaptiveCollectionInternals;

[DelegateImplementation(typeof(IList<>), nameof(_data), DelegationStyle.Implicit,
    ["Add", "Clear", "Remove", "Insert", "RemoveAt", "IsReadOnly"])]
[IgnoredIndexer([typeof(int)])]
public partial class IndirectionList<T> : IList<T>
{
    private readonly List<T> _data;
    private readonly List<int> _indexes;
    private readonly Stack<int> _free = [];


    public IndirectionList()
    {
        _data = [];
        _indexes = [];
    }

    public IndirectionList(int size)
    {
        _data = new List<T>(size);
        _indexes = new List<int>(size);
    }

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    public int Add(T item)
    {
        var dataCount = _data.Count;
        var physical = _free.Count > 0 ? _free.Pop() : dataCount;
        if (physical == dataCount) _data.Add(item);
        else _data[physical] = item;
        _indexes.Add(physical);
        return _indexes.Count - 1;
    }

    public void Clear()
    {
        _data.Clear();
        _indexes.Clear();
        _free.Clear();
    }

    public bool Remove(T item)
    {
        var index = _data.IndexOf(item);
        if (index == -1) return false;
        RemoveAt(index);
        return true;
    }

    public bool IsReadOnly => false;

    public void Insert(int index, T item)
    {
        throw new NotSupportedException(
            "Insertion is not supported for IndirectionList as it would violate indexes stability.");
    }

    public void RemoveAt(int index)
    {
        var phys = _indexes[index];
        _indexes[index] = -1;
        _data[phys] = default!;
        _free.Push(phys);
    }

    public T this[int index]
    {
        get => _data[_indexes[index]];
        set => _data[_indexes[index]] = value;
    }
}

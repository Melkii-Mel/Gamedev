using System;
using System.Collections.Generic;
using Attributes;

namespace Utils.Collections;

[DelegateImplementation(typeof(IList<>), nameof(_data), DelegationStyle.Implicit,
    ["Add", "Remove", "RemoveAt", "Insert", "IsReadOnly"])]
public partial class SwapAndPopList<T> : IList<T>
{
    private readonly List<int> _indexes;
    private readonly List<int> _ids;
    private readonly List<T> _data;

    public bool IsReadOnly => false;

    public SwapAndPopList(int size)
    {
        _indexes = new List<int>(size);
        _ids = new List<int>(size);
        _data = new List<T>(size);
    }

    public SwapAndPopList()
    {
        _indexes = [];
        _ids = [];
        _data = [];
    }

    public int Add(T item)
    {
        var dataLen = _data.Count;
        var idLen = _ids.Count;
        if (idLen > dataLen)
        {
            _data.Add(item);
            return _ids[dataLen];
        }

        _ids.Add(idLen);
        _indexes.Add(idLen);
        _data.Add(item);
        return idLen;
    }

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    public bool Remove(T item)
    {
        var actualIndex = _data.IndexOf(item);
        if (actualIndex == -1) return false;
        RemoveAt(_ids[actualIndex]);
        return true;
    }

    public void Insert(int index, T item)
    {
        throw new NotSupportedException("Insertion is not supported as it would violate index stability.");
    }

    public void RemoveAt(int logicalIndex)
    {
        var actualIndex = _indexes[logicalIndex];
        var dataIndexLast = _data.Count - 1;
        _data[actualIndex] = _data[dataIndexLast];
        _data.RemoveAt(dataIndexLast);
        var idLast = _ids[dataIndexLast];
        var idLogical = _ids[actualIndex];
        _ids[dataIndexLast] = idLogical;
        _ids[actualIndex] = idLast;
        _indexes[idLast] = actualIndex;
        _indexes[idLogical] = dataIndexLast;
    }
}

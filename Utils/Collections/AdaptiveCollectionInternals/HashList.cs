using System;
using System.Collections.Generic;
using Attributes;

namespace Utils.Collections.AdaptiveCollectionInternals;

/// <summary>
/// Implements IList<see cref="IList{T}"/> for compatibility but does not support index-based operations.
/// <para>
/// Intended only to be used internally in <see cref="AdaptiveCollection{T}"/>>
/// </para>
/// </summary>
/// <typeparam name="T">Collection item type</typeparam>
[DelegateImplementation(typeof(IList<>), nameof(_internal), DelegationStyle.Implicit,
    ["IndexOf", "Insert", "RemoveAt", "IsReadOnly"])]
[IgnoredIndexer([typeof(int)])]
public partial class HashList<T> : IList<T>
{
    private HashSet<T> _internal;
    public HashList() => _internal = [];
    public HashList(HashSet<T> @internal) => _internal = @internal;
    public bool IsReadOnly => false;
    public int IndexOf(T item) => throw new NotSupportedException();
    public void Insert(int index, T item) => throw new NotSupportedException();
    public void RemoveAt(int index) => throw new NotSupportedException();

    public T this[int index]
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
}

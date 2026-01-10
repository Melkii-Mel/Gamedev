using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Utils.DataStructures;

public class BiMap<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _ktv = [];
    private readonly Dictionary<TValue, TKey> _vtks = [];

    public BiMap()
    {
        KeyToValue = new ReadOnlyDictionary<TKey, TValue>(_ktv);
        ValueToKeys = new ReadOnlyDictionary<TValue, TKey>(_vtks);
    }

    public ReadOnlyDictionary<TKey, TValue> KeyToValue { get; }
    public ReadOnlyDictionary<TValue, TKey> ValueToKeys { get; }

    public void Add(TKey key, TValue value)
    {
        _ktv.Add(key, value);
        _vtks.Add(value, key);
    }
}

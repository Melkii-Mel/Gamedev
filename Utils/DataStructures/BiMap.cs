using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Utils.DataStructures;

public class BiMap<TKey, TValue>
{
    public ReadOnlyDictionary<TKey, TValue> KeyToValue { get; }
    public ReadOnlyDictionary<TValue, TKey> ValueToKeys { get; }

    private Dictionary<TKey, TValue> ktv = [];
    private Dictionary<TValue, TKey> vtks = [];

    public BiMap()
    {
        KeyToValue = new(ktv);
        ValueToKeys = new(vtks);
    }

    public void Add(TKey key, TValue value)
    {
        ktv.Add(key, value);
        vtks.Add(value, key);
    }
}
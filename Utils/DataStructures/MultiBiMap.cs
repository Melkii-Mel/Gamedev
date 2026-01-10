using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Utils.DataStructures;

public class MultiBiMap<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _ktv = [];
    private readonly Dictionary<TValue, List<TKey>> _vtks = [];

    public MultiBiMap()
    {
        KeyToValue = new ReadOnlyDictionary<TKey, TValue>(_ktv);
        ValueToKeys = new ReadOnlyDictionary<TValue, List<TKey>>(_vtks);
    }

    public ReadOnlyDictionary<TKey, TValue> KeyToValue { get; }
    public ReadOnlyDictionary<TValue, List<TKey>> ValueToKeys { get; }

    public void Add(TKey key, TValue value)
    {
        _ktv.Add(key, value);
        if (_vtks.TryGetValue(value, out var keys))
            keys.Add(key);
        else
            _vtks.Add(value, [key]);
    }
}

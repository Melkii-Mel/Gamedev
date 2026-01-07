using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Utils.DataStructures;

public class MultiBiMap<TKey, TValue>
{
    public ReadOnlyDictionary<TKey, TValue> KeyToValue { get; }
    public ReadOnlyDictionary<TValue, List<TKey>> ValueToKeys { get; }
    
    private Dictionary<TKey, TValue> ktv = [];
    private Dictionary<TValue, List<TKey>> vtks = [];

    public MultiBiMap()
    {
        KeyToValue = new(ktv);
        ValueToKeys = new(vtks);
    }

    public void Add(TKey key, TValue value)
    {
        ktv.Add(key, value);
        if (vtks.TryGetValue(value, out var keys))
        {
            keys.Add(key);
        }
        else
        {
            vtks.Add(value, [key]);
        }
    }
}

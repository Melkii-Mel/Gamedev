using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Utils.Observables;

namespace Utils.DataStructures;

public class MultiIndexStore<TPKey, TValue> where TValue : IHasId<TPKey>
{
    public ReadOnlyDictionary<TPKey, TValue> PrimaryIndex { get; }

    private readonly Dictionary<TPKey, TValue> _primaryIndex = [];
    private readonly List<Action<TValue>> _indexUpdates = [];

    public MultiIndexStore()
    {
        PrimaryIndex = new(_primaryIndex);
    }

    public void Add(TValue value)
    {
        _primaryIndex.Add(value.Id, value);
        _indexUpdates.ForEach(update => update(value));
    }

    public Index<TIndexKey, TValue> AddOTMIndex<TIndexKey>(Func<TValue, ObservableProperty<TIndexKey>> property)
    {
        return AddAbstractIndex<ObservableProperty<TIndexKey>, TIndexKey>(
            property, 
            (prop, action) => action(prop.Value),
            (value, property, dict, add, remove) =>
            {
                property.PropertyChanged += args =>
                {
                    remove(args.Old);
                    add(args.New);
                };
            }
        );
    }

    public Index<TIndexKey, TValue> AddOTMIndexFixed<TIndexKey>(Func<TValue, TIndexKey> property) where TIndexKey : notnull
    {
        return AddAbstractIndex<TIndexKey, TIndexKey>(
            property,
            (prop, action) => action(prop),
            null
        );
    }

    public Index<TIndexKey, TValue> AddMTMIndex<TIndexKey, TCollectionItem>(Func<TValue, Observables.ObservableCollection<TCollectionItem>> property, Func<TCollectionItem, TIndexKey> convertItemFormat) where TIndexKey : notnull
    {
        return AddAbstractIndex<Observables.ObservableCollection<TCollectionItem>, TIndexKey>(
            property, 
            (prop, action) => 
            {
                foreach (var key in prop) 
                { 
                    action(convertItemFormat(key));
                }
            }, 
            (value, property, dict, add, remove) => 
            {
                property.CollectionChanged += args =>
                {
                    foreach (var key in args.Old ?? [])
                    {
                        remove(convertItemFormat(key));
                    }
                    foreach (var key in args.New ?? [])
                    {
                        add(convertItemFormat(key));
                    }
                };
            }
        );
    }

    public Index<TIndexKey, TValue> AddMTMIndexFixed<TIndexKey, TCollection, TCollectionItem>(Func<TValue, TCollection> property, Func<TCollectionItem, TIndexKey> convertItemFormat)
    where TCollection : ICollection<TCollectionItem> where TIndexKey : notnull
    {
        return AddAbstractIndex<TCollection, TIndexKey>(
            property,
            (prop, action) =>
            {
                foreach (var key in prop)
                {
                    action(convertItemFormat(key));
                }
            },
            null
        );
    }

    private Index<TIndexKey, TValue> AddAbstractIndex<TProp, TIndexKey>(
        Func<TValue, TProp> getProperty, 
        Action<TProp, Action<TIndexKey>> emitKeys,
        Action<TValue, TProp, Index<TIndexKey, TValue>, Action<TIndexKey>, Action<TIndexKey>>? propChangeBinder) where TIndexKey : notnull
    {
        var dict = new Dictionary<TIndexKey, HashSet<TValue>>();
        var index = new Index<TIndexKey, TValue>(dict);

        if (PrimaryIndex.Values != null)
        {
            foreach (var value in PrimaryIndex.Values)
            {
                IndexValue(value);
            }
        }

        _indexUpdates.Add(IndexValue);

        return new Index<TIndexKey, TValue>(dict);

        void IndexValue(TValue value)
        {
            var property = getProperty(value);
            emitKeys(property, key =>
            {
                Add(key);
                propChangeBinder?.Invoke(
                    value, 
                    property, 
                    index,
                    Add,
                    Remove
                );
            });

            return;

            void Add(TIndexKey key)
            {
                if (!dict.TryGetValue(key, out var set))
                {
                    dict[key] = set = new HashSet<TValue>(); 
                }
                (set as HashSet<TValue>)!.Add(value);
            }

            void Remove(TIndexKey key)
            {
                if (dict.TryGetValue(key, out var values))
                {
                    (values as HashSet<TValue>)!.Remove(value);
                    if (values.Count == 0)
                    {
                        dict.Remove(key);
                    }
                }
            }
        }
    }
}

public interface IHasId<TKey>
{
    TKey Id { get; }
}

public class Index<TKey, TValue>(Dictionary<TKey, HashSet<TValue>> values) where TKey : notnull
{
    public IReadOnlyCollection<TValue> this[TKey index]
    {
        get
        {
            if (values.TryGetValue(index, out var item))
            {
                return item;
            }
            return values[index] = [];
        }
    }
}

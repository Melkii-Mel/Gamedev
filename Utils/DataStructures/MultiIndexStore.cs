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

    public ReadOnlyDictionary<TIndexKey, HashSet<TValue>> AddOTMIndex<TIndexKey>(Func<TValue, ObservableProperty<TIndexKey>> property)
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

    public ReadOnlyDictionary<TIndexKey, HashSet<TValue>> AddMTMIndex<TIndexKey, TCollectionItem>(Func<TValue, Observables.ObservableCollection<TCollectionItem>> property, Func<TCollectionItem, TIndexKey> convertItemFormat)
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

    private ReadOnlyDictionary<TIndexKey, HashSet<TValue>> AddAbstractIndex<TProp, TIndexKey>(
        Func<TValue, TProp> getProperty, 
        Action<TProp, Action<TIndexKey>> emitKeys,
        Action<TValue, TProp, Dictionary<TIndexKey, HashSet<TValue>>, Action<TIndexKey>, Action<TIndexKey>> propChangeBinder)
    {
        var dict = new Dictionary<TIndexKey, HashSet<TValue>>();

        foreach (var value in PrimaryIndex.Values)
        {
            IndexValue(value);
        }

        _indexUpdates.Add(IndexValue);

        return new(dict);

        void IndexValue(TValue value)
        {
            var property = getProperty(value);
            emitKeys(property, key =>
            {
                Add(key);
                propChangeBinder(
                    value, 
                    property, 
                    dict,
                    Add,
                    Remove
                );
            });

            return;

            void Add(TIndexKey key)
            {
                if (!dict.TryGetValue(key, out var set))
                {
                    dict[key] = set = []; 
                }
                set.Add(value);
            }

            void Remove(TIndexKey key)
            {
                if (dict.TryGetValue(key, out var values))
                {
                    values.Remove(value);
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

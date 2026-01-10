using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Utils.Observables;

namespace Utils.DataStructures;

public class MultiIndexStore<TPKey, TValue> where TValue : IHasId<TPKey>
{
    private readonly List<Action<TValue>> _indexUpdates = [];

    private readonly Dictionary<TPKey, TValue> _primaryIndex = [];

    public MultiIndexStore()
    {
        PrimaryIndex = new ReadOnlyDictionary<TPKey, TValue>(_primaryIndex);
    }

    public ReadOnlyDictionary<TPKey, TValue> PrimaryIndex { get; }

    public void Add(TValue value)
    {
        _primaryIndex.Add(value.Id, value);
        _indexUpdates.ForEach(update => update(value));
    }

    public Index<TIndexKey, TValue> AddOneToManyIndex<TIndexKey>(Func<TValue, ObservableProperty<TIndexKey>> property)
        where TIndexKey : notnull
    {
        return AddAbstractIndex<ObservableProperty<TIndexKey>, TIndexKey>(
            property,
            (prop, action) => action(prop.Value),
            (_, prop, _, add, remove) =>
            {
                prop.PropertyChanged += args =>
                {
                    remove(args.Old);
                    add(args.New);
                };
            }
        );
    }

    public Index<TIndexKey, TValue> AddOneToManyIndexFixed<TIndexKey>(Func<TValue, TIndexKey> property)
        where TIndexKey : notnull
    {
        return AddAbstractIndex<TIndexKey, TIndexKey>(
            property,
            (prop, action) => action(prop),
            null
        );
    }

    public Index<TIndexKey, TValue> AddManyToManyIndex<TIndexKey, TCollectionItem>(
        Func<TValue, Observables.ObservableCollection<TCollectionItem>> property,
        Func<TCollectionItem, TIndexKey> convertItemFormat) where TIndexKey : notnull
    {
        return AddAbstractIndex<Observables.ObservableCollection<TCollectionItem>, TIndexKey>(
            property,
            (prop, action) =>
            {
                foreach (var key in prop) action(convertItemFormat(key));
            },
            (_, prop, _, add, remove) =>
            {
                prop.CollectionChanged += args =>
                {
                    foreach (var key in args.Old ?? []) remove(convertItemFormat(key));
                    foreach (var key in args.New ?? []) add(convertItemFormat(key));
                };
            }
        );
    }

    public Index<TIndexKey, TValue> AddManyToManyIndexFixed<TIndexKey, TCollection, TCollectionItem>(
        Func<TValue, TCollection> property, Func<TCollectionItem, TIndexKey> convertItemFormat)
        where TCollection : ICollection<TCollectionItem> where TIndexKey : notnull
    {
        return AddAbstractIndex<TCollection, TIndexKey>(
            property,
            (prop, action) =>
            {
                foreach (var key in prop) action(convertItemFormat(key));
            },
            null
        );
    }

    private Index<TIndexKey, TValue> AddAbstractIndex<TProp, TIndexKey>(
        Func<TValue, TProp> getProperty,
        Action<TProp, Action<TIndexKey>> emitKeys,
        Action<TValue, TProp, Index<TIndexKey, TValue>, Action<TIndexKey>, Action<TIndexKey>>? propChangeBinder)
        where TIndexKey : notnull
    {
        var dict = new Dictionary<TIndexKey, HashSet<TValue>>();
        var index = new Index<TIndexKey, TValue>(dict);

        if (PrimaryIndex.Values != null)
            foreach (var value in PrimaryIndex.Values)
                IndexValue(value);

        _indexUpdates.Add(IndexValue);

        return new Index<TIndexKey, TValue>(dict);

        void IndexValue(TValue value)
        {
            var property = getProperty(value);
            emitKeys(property, key =>
            {
                AddKey(key);
                propChangeBinder?.Invoke(
                    value,
                    property,
                    index,
                    AddKey,
                    Remove
                );
            });

            return;

            void AddKey(TIndexKey key)
            {
                if (!dict.TryGetValue(key, out var set)) dict[key] = set = [];
                set.Add(value);
            }

            void Remove(TIndexKey key)
            {
                if (!dict.TryGetValue(key, out var values)) return;
                values!.Remove(value);
                if (values.Count == 0) dict.Remove(key);
            }
        }
    }
}

public interface IHasId<out TKey>
{
    TKey Id { get; }
}

public class Index<TKey, TValue>(Dictionary<TKey, HashSet<TValue>> values) where TKey : notnull
{
    public IReadOnlyCollection<TValue> this[TKey index]
    {
        get
        {
            if (values.TryGetValue(index, out var item)) return item;
            return values[index] = [];
        }
    }
}

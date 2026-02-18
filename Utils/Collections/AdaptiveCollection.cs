using System;
using System.Collections.Generic;
using Attributes;
using Utils.Collections.AdaptiveCollectionInternals;
using Utils.Extensions;
using static Utils.Collections.AdaptiveCollectionOptions;

// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable InvertIf

namespace Utils.Collections;

[DelegateImplementation(typeof(IList<>), nameof(_inner))]
public partial class AdaptiveCollection<T> : IList<T>
{
    private IList<T> _inner;

    public AdaptiveCollection(AdaptiveCollectionOptions options, int size = 0)
    {
        _inner = CreateCollectionFor(options, size);
    }

    public AdaptiveCollection(IList<T> inner)
    {
        _inner = inner;
    }

    public static IList<T> CreateCollectionFor(AdaptiveCollectionOptions options, int size)
    {
        var enforceUniqueness = options.IsSet(EnforceUniqueness);
        var stableIndexes = options.IsSet(StableIndexes);
        var stableOrder = options.IsSet(StableOrder);

        var fastAddRemove = options.IsSet(FastAddRemove);
        var fastContains = options.IsSet(FastContains);

        var allowNoDuplicates = options.IsSet(AllowNoDuplicatesSupport);
        var allowNoLookup = options.IsSet(AllowNoLookup);
        var allowSlowIteration = options.IsSet(AllowSlowIteration);
        var allowSlowLookup = options.IsSet(AllowSlowLookup);
        var supportDuplicates = !allowNoDuplicates;

        if (supportDuplicates && fastContains)
            throw new NotSupportedException("Can't have fast lookup while allowing duplicates.");
        if (enforceUniqueness && supportDuplicates)
            throw new NotSupportedException(
                $"{nameof(EnforceUniqueness)} and {nameof(AllowNoDuplicatesSupport)} are incompatible.");
        if (stableIndexes && allowNoLookup)
            throw new NotSupportedException($"{nameof(StableIndexes)} and {nameof(AllowNoLookup)} are incompatible.");
        if (enforceUniqueness) throw new NotImplementedException();
        if (fastContains && !allowNoDuplicates)
            throw new NotSupportedException("FastContains requires elements uniqueness");
        if (stableIndexes)
        {
            if (fastAddRemove && (stableOrder || fastContains))
                throw new NotSupportedException("Cannot combine fast add/remove with stable order or fast contains");
            if (stableOrder) return new IndirectionList<T>(size);
            if (fastAddRemove) return new SwapAndPopList<T>(size);
            if (fastContains) throw new NotImplementedException();
            return new SwapAndPopList<T>(size);
        }

        if (fastContains && allowNoDuplicates)
        {
            if (allowNoLookup && allowSlowIteration && allowNoDuplicates) return new HashList<T>();
            if (allowNoDuplicates) return new SwapAndPopDictList<T>(size);
        }

        if (fastAddRemove) return new SwapAndPopList<T>(size);
        return new List<T>(size);
    }
}

[Flags]
public enum AdaptiveCollectionOptions
{
    None = 0,
    StableIndexes = 1 << 0,
    StableOrder = 1 << 1,
    AllowSlowLookup = 1 << 2,
    FastContains = 1 << 3,
    FastAddRemove = 1 << 4,
    AllowNoDuplicatesSupport = 1 << 5,
    EnforceUniqueness = 1 << 6,
    AllowNoLookup = 1 << 7,
    AllowSlowIteration = 1 << 8,
}

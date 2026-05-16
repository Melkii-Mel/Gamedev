using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sall.Evaluation;
using Sall.Lowering;

namespace Sall;

public class MarkerIdIndex
{
    private readonly ValueSet<StringId>[] _markers;
    public ImmutableDictionary<StringId, int> IdCountMap { get; private set; }
    public ImmutableArray<int> Counts { get; private set; }

    private Dictionary<ValueSet<StringId>, ValueSet<StringId>> _cache = [];

    public MarkerIdIndex(ValueSet<StringId>[] markers)
    {
        _markers = markers;
        var idCountMapBuilder = ImmutableDictionary.CreateBuilder<StringId, int>();
        foreach (var markerSet in markers)
        {
            foreach (var marker in markerSet)
            {
                idCountMapBuilder.TryGetValue(marker, out var c);
                idCountMapBuilder[marker] = c + 1;
            }
        }

        IdCountMap = idCountMapBuilder.ToImmutable();
        Counts = [.. IdCountMap.Values.OrderBy(v => v)];
    }

    public IOrderedEnumerable<StringId> OrderByFrequency(ValueSet<StringId> markers)
    {
        return markers.ToArray().OrderBy(m => IdCountMap[m]);
    }
}

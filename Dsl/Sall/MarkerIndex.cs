using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sall.Evaluation;
using Sall.Lowering;

namespace Sall;

/// <summary>
/// Contains class markers and groups them, allowing faster and easier class querying
/// </summary>
public class MarkerIndex
{
    private readonly Dictionary<ValueSet<StringId>, NormalizedClass[]> _markersClassMap;
    private readonly Dictionary<ValueSet<StringId>, List<NormalizedClass>> _markersClassCache = [];
    public ImmutableDictionary<StringId, int> IdCountMap { get; }
    public ImmutableArray<int> Counts { get; private set; }

    public MarkerIndex(IEnumerable<NormalizedClass> classes)
    {
        _markersClassMap = classes.GroupBy(c => c.Markers).Select(c => c.ToArray())
            .ToDictionary(c => c.First().Markers);
        var idCountMapBuilder = ImmutableDictionary.CreateBuilder<StringId, int>();
        foreach (var marker in _markersClassMap.Keys.SelectMany(markerSet => markerSet))
        {
            idCountMapBuilder.TryGetValue(marker, out var c);
            idCountMapBuilder[marker] = c + 1;
        }

        IdCountMap = idCountMapBuilder.ToImmutable();
        Counts = [.. IdCountMap.Values.OrderBy(v => v)];
    }

    public IOrderedEnumerable<StringId> OrderByFrequency(ValueSet<StringId> markers)
    {
        return markers.ToArray().OrderBy(m => IdCountMap[m]);
    }

    public IReadOnlyList<NormalizedClass> GetClassesFor(IEnumerable<StringId> nodeMarkers)
    {
        var vs = ValueSet.From(nodeMarkers);
        if (_markersClassCache.TryGetValue(vs, out var classes)) return classes;
        var matches = new List<NormalizedClass>();
        foreach (var normalizedClasses in _markersClassMap.Where(normalizedClasses =>
                     vs.IsSubsetOf(normalizedClasses.Key))) matches.AddRange(normalizedClasses.Value);
        _markersClassCache[vs] = matches;
        return matches;
    }
}

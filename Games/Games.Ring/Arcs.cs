#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Games.Ring;

public class Arcs
{
    private readonly List<Arc> _raw;
    public ArcsNormalized Normalized { get; private set; }

    public Arcs(Level level)
    {
        _raw = level.Ring.Arcs.ToList();
        Normalized = UpdateNormalized();
    }

    public delegate void ArcChange(ref Arc arc);

    public void ChangeArc(int index, ArcChange change)
    {
        var arc = _raw[index];
        change(ref arc);
        _raw[index] = arc;
        UpdateNormalized();
    }

    public void DeleteArc(int index)
    {
        _raw.RemoveAt(index);
        UpdateNormalized();
    }

    public void AddArc(Arc arc)
    {
        _raw.Add(arc);
        UpdateNormalized();
    }

    private ArcsNormalized UpdateNormalized()
    {
        Normalized = new ArcsNormalized(Normalize(_raw),
            Normalize(_raw.Select(a => a.Expand(Game.Data.Rules.GoodTolerance))));
        return Normalized;

        Arc[] Normalize(IEnumerable<Arc> arcs)
        {
            var ordered = arcs.OrderBy(a => a.StartAngle).ToList();
            for (var i = ordered.Count - 1; i >= 1; i--)
            {
                var prev = ordered[i - 1];
                var curr = ordered[i];
                if (!(prev.EndAngle > curr.StartAngle)) continue;
                prev.EndAngle = MathF.Max(curr.EndAngle, prev.EndAngle);
                ordered.RemoveAt(i);
                i++;
            }

            return ordered.ToArray();
        }
    }

    public record struct ArcsNormalized(Arc[] Normal, Arc[] Expanded);
}

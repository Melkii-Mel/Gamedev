using System;
using System.Collections.Generic;
using System.Linq;
using Sall;

namespace UiCore;

public record Node(
    Type Type,
    string[] Classes,
    Node[] Children
);

public record NormalizedClass(ValueSet<string> Markers, NormalizedProperty[] Properties);

public record NormalizedProperty(Scope Scope, string Ident, Expr Expr);

// TODO: Error handling (severity levels, debug printing instead of throwing)
// TODO: Weight
public class Engine
{
    private List<NormalizedClass> _normalizedClasses = [];
    private Dictionary<ValueSet<string>, NormalizedClass> _classesCache = [];

    public Engine(Node root, Stylespace stylespace)
    {
        foreach (var selectorClass in stylespace.Classes)
        {
            var markers = new ValueSet<string>();
            foreach (var selectorsItem in selectorClass.Key.Selectors.Items)
            {
                if (selectorsItem is not AtomSelectorExpr
                    {
                        SelectorExprOrSelector: MarkerSelector markerSelector
                    }) throw new NotSupportedException();
                markers.Add(markerSelector.Ident);
            }
            _normalizedClasses.Add(new NormalizedClass(markers, selectorClass.Value.Properties));
        }

        IEnumerable<NormalizedProperty> MergeClassProperties(Class c, Scope scope)
        {
            IEnumerable<NormalizedProperty> properties = NormalizeProperties(c.Properties);
            foreach (var inheritance in c.Parents)
            {
                // TODO: Error message on invalid call
                var parent = stylespace.NamedClasses[inheritance.Ident];
                var localScope = new Scope(scope, new Dictionary<string, ISymbol>());
                
                properties = properties.Union(MergeClassProperties(parent));
            }
            return properties;

            IEnumerable<NormalizedProperty> NormalizeProperties(IEnumerable<Property> ps)
            {
                return ps.Select(p => new NormalizedProperty(scope, p.Ident, p.Expr));
            }
        }
    }

    public void AddStylesheet(Stylesheet stylesheet)
    {
    }

    private void Apply(Node node)
    {
        node
    }
}

public class ValueSet<T> : HashSet<T>
{
    public override int GetHashCode()
    {
        return this.Sum(i => i?.GetHashCode() ?? 0);
    }

    public bool Equals(ValueSet<T>? other)
    {
        if (other is null) return false;
        return GetHashCode() == other.GetHashCode();
    }
}
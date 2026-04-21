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

public record NormalizedNamedClass(string Name, NormalizedProperty[] Properties) : NormalizedClass([Name], Properties);

public record NormalizedProperty(Scope Scope, string Ident, Expr Expr);

// TODO: Error handling (severity levels, debug printing instead of throwing)
// TODO: Weight
public class Engine
{
    private List<NormalizedClass> _normalizedClasses = [];
    private Dictionary<ValueSet<string>, NormalizedClass> _classesCache = [];

    public Engine(Node root, Stylespace stylespace)
    {
        var normalizedNamedClasses = NormalizeNamedClasses(stylespace.NamedClasses);

        return;

        Dictionary<string, NormalizedClass> NormalizeNamedClasses(Dictionary<string, NamedClass> namedClasses)
        {
            var scope = new Scope(null, []);
            var result = new Dictionary<string, NormalizedClass>(namedClasses.Count);
            var classDeps = new ClassDeps();
            foreach (var namedClass in namedClasses)
            {
                classDeps.Clear();
                NormalizeClass(namedClass.Key, null);
            }

            void NormalizeClass(string className, Args? args)
            {
                classDeps.Add(className);

                var (_, @params, parents, properties, anonymousClasses) = namedClasses[className];
                if (classDeps.Contains(className))

                    foreach (var parent in parents)
                    {
                        NormalizeClass(parent.Ident, parent.Args);
                    }

                var normalizedClass = new NormalizedNamedClass(className,);
            }
        }

        List<NormalizedClass> NormalizeClasses(Dictionary<SelectorChain, Class> classes)
        {
            foreach (var kvp in classes)
            {
                var selectors = kvp.Key.Selectors;
                var @class = kvp.Value;
                var markers = new ValueSet<string>();
                foreach (var selectorsItem in selectors.Items)
                {
                    if (selectorsItem is not AtomSelectorExpr
                        {
                            SelectorExprOrSelector: MarkerSelector markerSelector,
                        }) throw new NotSupportedException();
                    markers.Add(markerSelector.Ident);
                }

                _normalizedClasses.Add(new NormalizedClass(markers, MergeClassProperties(@class, scope).ToArray()));
            }
        }

        IEnumerable<NormalizedProperty> MergeClassProperties(Class c, Scope s)
        {
            var properties = NormalizeProperties(c.Properties);
            foreach (var inheritance in c.Parents)
            {
                // TODO: Error message on invalid call
                var parent = stylespace.NamedClasses[inheritance.Ident];
                var localScope = new Scope(s, new Dictionary<string, ISymbol>());
                localScope.AddParamsToScope(parent.Params, inheritance);
                foreach (var param in parent.Params)
                {
                    localScope.Add(param.Ident,);
                }

                properties = properties.Union(MergeClassProperties(parent, localScope));
            }

            return properties;

            IEnumerable<NormalizedProperty> NormalizeProperties(IEnumerable<Property> ps)
            {
                return ps.Select(p => new NormalizedProperty(s, p.Ident, p.Expr));
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

public enum ClassProcessing
{
    Started,
    Finished
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

public class ClassDeps : HashSet<string>
{
    public new void Add(string s)
    {
        // TODO: Exception type and message
        if (Contains(s))
            throw new InvalidOperationException($"Cyclic reference detected for class {s}");
        base.Add(s);
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Attributes;
using Sall.Api;
using Sall.Ast;
using Sall.Evaluation;

namespace Sall.Lowering;

public record NormalizedClass(ValueSet<StringId> Markers, Dictionary<StringId, NormalizedProperty> Properties);

public record NormalizedNamedClass(
    string Name,
    Dictionary<StringId, NormalizedProperty> Properties
) : NormalizedClass(new ValueSet<StringId>([Name]), Properties);

public record NormalizedProperty(Scope Scope, string Ident, Expr Expr);

public class ClassNormalizer
{
    private ClassDeps _classDeps = [];
    private readonly Stylespace _stylespace;
    private readonly Scope _scope;

    public ClassNormalizer(Stylespace stylespace, Scope scope)
    {
        _stylespace = stylespace;
        _scope = scope;
    }

    public NormalizedClasses NormalizeClasses()
    {
        var normalizedClasses = new NormalizedClasses();
        normalizedClasses.RegisterClassBundles(NormalizeNamedClasses(_stylespace.NamedClasses));
        normalizedClasses.RegisterClassBundles(NormalizeAnonymousClasses(_stylespace.AnonymousClasses.Values));
        return normalizedClasses;
    }

    private IEnumerable<ClassBundle> NormalizeNamedClasses(Dictionary<string, NamedClass> namedClasses)
    {
        _classDeps = [];
        foreach (var namedClass in namedClasses)
        {
            _classDeps.Clear();
            yield return NormalizeNamedClass(namedClass.Key, _stylespace.Scope, null);
        }
    }

    private IEnumerable<ClassBundle> NormalizeAnonymousClasses(IEnumerable<AnonymousClass> anonymousClasses)
    {
        return anonymousClasses.Select(anonymousClass => NormalizeAnonymousClass(anonymousClass, _scope));
    }

    private ClassBundle NormalizeAnonymousClass(AnonymousClass anonymousClass, Scope scope,
        ValueSet<StringId>? parentMarkers = null)
    {
        var selectorChain = SelectorChainToMarkers(anonymousClass.SelectorChain);
        parentMarkers?.AddTo(selectorChain);
        var (normalizedProperties, normalizedClasses) = SharedNormalizationStep(anonymousClass, scope);
        normalizedClasses.Add(new NormalizedClass(selectorChain, normalizedProperties));

        return new ClassBundle(null, normalizedClasses);
    }

    private (Dictionary<StringId, NormalizedProperty> NormalizedProperties, List<NormalizedClass> NestedClasses)
        SharedNormalizationStep(Class c, Scope localScope)
    {
        var (parents, properties, subClasses) = c;
        var normalizedProperties = NormalizeProperties(properties, localScope);
        var nestedClasses = new List<NormalizedClass>();
        foreach (var cb in subClasses.Select(sc => NormalizeAnonymousClass(sc, localScope)))
            nestedClasses.AddRange(cb.AnonymousClasses);

        foreach (var parent in parents)
        {
            var parentBundle = NormalizeNamedClass(parent.Ident, localScope, parent.Args);
            nestedClasses.AddRange(parentBundle.AnonymousClasses);
            if (parentBundle.NamedClass != null)
                MergeProperties(parentBundle.NamedClass.Properties, normalizedProperties);
        }

        return (normalizedProperties, nestedClasses);
    }

    private ClassBundle NormalizeNamedClass(string className, Scope parentScope, Args? parentArgs)
    {
        _classDeps.Add(className);

        var (_, @params, parents, properties, anonymousClasses) = _stylespace.NamedClasses[className];

        var localScope = new Scope(parentScope, []);
        localScope.AddParamsToScope(@params, parentArgs);

        var normalizedProperties = NormalizeProperties(properties, localScope);
        var nestedClasses = new List<NormalizedClass>();

        foreach (var parent in parents)
        {
            var cb = NormalizeNamedClass(parent.Ident, localScope, parent.Args);
            if (cb.NamedClass != null) MergeProperties(cb.NamedClass.Properties, normalizedProperties);
        }

        foreach (var classes in anonymousClasses.Select(ac =>
                     NormalizeAnonymousClass(ac, localScope, new ValueSet<StringId>([className])).AnonymousClasses))
        {
            nestedClasses.AddRange(classes);
        }

        var normalizedClass = new NormalizedNamedClass(className, normalizedProperties);

        return new ClassBundle(normalizedClass, nestedClasses.ToList());
    }

    private static Dictionary<StringId, NormalizedProperty> NormalizeProperties(IEnumerable<Property> p, Scope s)
    {
        var result = new Dictionary<StringId, NormalizedProperty>();
        foreach (Property property in p) result[property.Ident] = NormalizeProperty(property, s);
        return result;
    }

    // TODO: Generalize
    private static NormalizedProperty NormalizeProperty(Property property, Scope scope)
    {
        return new NormalizedProperty(scope, property.Ident, property.Expr);
    }

    private static HashSet<StringId> SelectorChainToMarkers(SelectorChain selectorChain)
    {
        var selectors = selectorChain.Selectors;
        var markersSet = new HashSet<StringId>();
        foreach (var selectorsItem in selectors.Items)
        {
            if (selectorsItem is not AtomSelectorExpr
                {
                    SelectorExprOrSelector: MarkerSelector markerSelector,
                }) throw new NotSupportedException();
            markersSet.Add(markerSelector.Ident);
        }

        return markersSet;
    }

    // TODO: Generalize
    /// <summary>
    /// Adds properties to <see cref="b"/> from <see cref="a"/> if they're not already present in <see cref="b"/>
    /// </summary>
    private static void MergeProperties<TKey, TValue>(Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b)
    {
        foreach (var kvp in a.Where(kvp => !b.ContainsKey(kvp.Key))) b[kvp.Key] = kvp.Value;
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

[DelegateImplementation(typeof(IImmutableSet<>), nameof(_set))]
public class ValueSet<T> : IImmutableSet<T>, IEnumerable<T>
{
    private readonly HashSet<T> _set;

    public static implicit operator ValueSet<T>(HashSet<T> set) => new(set);
    public ValueSet(HashSet<T> set) => _set = [..set];

    public void AddTo(HashSet<T> other)
    {
        foreach (var t in _set) other.Add(t);
    }

    public static ValueSet<T> From(IEnumerable<T> enumerable)
    {
        var result = new HashSet<T>();
        foreach (var item in enumerable)
        {
            result.Add(item);
        }

        return result;
    }

    public override int GetHashCode()
    {
        return this.Sum(i => i?.GetHashCode() ?? 0);
    }

    public bool Equals(ValueSet<T>? other)
    {
        if (other is null) return false;
        return GetHashCode() == other.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is ValueSet<T> vs && Equals(vs);
    }
}

public class NormalizedClasses : Dictionary<ValueSet<StringId>, List<NormalizedClass>>
{
    // TODO: Consider deleting
    // ReSharper disable once UnusedMember.Global
    public void Merge(NormalizedClasses other)
    {
        foreach (var kvp in other) this[kvp.Key] = kvp.Value;
    }

    public void RegisterClassBundle(ClassBundle classBundle)
    {
        if (classBundle.NamedClass != null) (this[classBundle.NamedClass.Markers] ??= []).Add(classBundle.NamedClass);
        foreach (var c in classBundle.AnonymousClasses) (this[c.Markers] ??= []).Add(c);
    }

    public void RegisterClassBundles(
        IEnumerable<ClassBundle> normalizeNamedClasses)
    {
        foreach (var namedClassBundles in normalizeNamedClasses)
            RegisterClassBundle(namedClassBundles);
    }
}

public record ClassBundle(NormalizedNamedClass? NamedClass, List<NormalizedClass> AnonymousClasses);

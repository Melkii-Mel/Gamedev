using System;
using System.Collections.Generic;
using System.Linq;
using Sall.Ast;
using Sall.Evaluation;
using Sall.Lowering;

namespace Sall.Api;

/// <summary>
/// A collection of classes local to a certain Scope
/// </summary>
/// <param name="Classes"></param>
public record Stylespace(
    StringId Name,
    Dictionary<string, Variable> Variables,
    Dictionary<string, NamedClass> NamedClasses,
    Dictionary<SelectorChain, Class> Classes,
    Dictionary<SelectorChain, AnonymousClass> AnonymousClasses
)
{
    private Scope? _scope;
    public Scope Scope => _scope ??= Scope.FromStylespace(this);

    public void Append(Stylespace other)
    {
        JoinDict(NamedClasses, other.NamedClasses);
        JoinDict(AnonymousClasses, other.AnonymousClasses);
        JoinDict(Classes, other.Classes);
        JoinDict(Variables, other.Variables);
        Scope.AddSymbolsFrom(other.Scope);

        return;

        void JoinDict<TKey, TValue>(Dictionary<TKey, TValue> to, Dictionary<TKey, TValue> from)
        {
            foreach (var kvp in from)
            {
                // TODO (later): Message and exception type
                if (to.ContainsKey(kvp.Key))
                {
                    throw new Exception();
                }

                to.Add(kvp.Key, kvp.Value);
            }
        }
    }
}

public static class StylesheetExt
{
    public static IEnumerable<Stylespace> ToApiStylespaces(this Stylesheet stylesheet)
    {
        var stylespaceNames = new List<string>();
        var stylespaces = new Dictionary<string, Stylespace>();
        foreach (var astStylespace in stylesheet.AstStylespaces)
        {
            var ss = new Stylespace(astStylespace.Ident, [], [], [], []);
            foreach (var variable in astStylespace.Variables)
            {
                ss.Variables.Add(variable.Ident, variable);
            }

            foreach (var anonymousClass in astStylespace.AnonymousClasses)
            {
                ss.Classes.Add(anonymousClass.SelectorChain, anonymousClass);
                ss.AnonymousClasses.Add(anonymousClass.SelectorChain, anonymousClass);
            }

            foreach (var namedClass in astStylespace.NamedClasses)
            {
                ss.Classes.Add(
                    new SelectorChain(new ValueArray<SelectorExpr>([
                        new AtomSelectorExpr(new MarkerSelector(namedClass.Ident)),
                    ])),
                    namedClass);
                ss.NamedClasses.Add(namedClass.Ident, namedClass);
            }

            if (stylespaces.TryGetValue(ss.Name, out var otherSs))
            {
                otherSs.Append(ss);
            }
            else
            {
                stylespaceNames.Add(ss.Name);
                stylespaces.Add(ss.Name, ss);
            }
        }

        return stylespaceNames.Select(s => stylespaces[s]);
    }

    /// <summary>
    /// Converts each stylesheet into a collection of Stylespaces and then merges these collections
    /// </summary>
    /// <param name="stylesheets"></param>
    /// <returns></returns>
    public static IEnumerable<Stylespace> ToApiStylespaces(this IEnumerable<Stylesheet> stylesheets)
    {
        return stylesheets.Select(ss => ss.ToApiStylespaces()).Merge();
    }

    /// <summary>
    /// Merges multiple collections of <see cref="Stylespace"/> into a single collection,
    /// combining entries with the same identifier.
    /// </summary>
    /// <param name="stylespaceGroups">A sequence of Stylespace collections to merge.</param>
    /// <returns>A merged sequence of Stylespace objects.</returns>
    public static IEnumerable<Stylespace> Merge(this IEnumerable<IEnumerable<Stylespace>> stylespaceGroups)
    {
        List<Stylespace> mergedList = [];
        Dictionary<string, Stylespace> mergedMap = [];
        foreach (var stylespaces in stylespaceGroups)
        {
            foreach (var stylespace in stylespaces)
            {
                if (mergedMap.TryGetValue(stylespace.Name, out var ss))
                {
                    ss.Append(stylespace);
                }
                else
                {
                    mergedList.Add(stylespace);
                    mergedMap[stylespace.Name] = stylespace;
                }
            }
        }

        return mergedList;
    }
}

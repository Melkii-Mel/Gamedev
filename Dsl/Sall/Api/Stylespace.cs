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
    string Name,
    Dictionary<string, Variable> Variables,
    Dictionary<string, NamedClass> NamedClasses,
    Dictionary<SelectorChain, Class> Classes,
    Dictionary<SelectorChain, AnonymousClass> AnonymousClasses
)
{
    private Scope? _scope;
    public Scope Scope => _scope ??= Scope.FromStylespace(this);

    public static Stylespace FromStylesheets(Stylespace[] stylesheets)
    {
        var result = new Stylespace([], [], [], []);
        foreach (var stylesheet in stylesheets)
        {
            foreach (var variable in stylesheet.Variables)
            {
                result.Variables.Add(variable.Ident, variable);
            }

            foreach (var anonymousClass in stylesheet.AnonymousClasses)
            {
                result.Classes.Add(anonymousClass.SelectorChain, anonymousClass);
                result.AnonymousClasses.Add(anonymousClass.SelectorChain, anonymousClass);
            }

            foreach (var namedClass in stylesheet.NamedClasses)
            {
                result.Classes.Add(
                    new SelectorChain(new ValueArray<SelectorExpr>([
                        new AtomSelectorExpr(new MarkerSelector(namedClass.Ident)),
                    ])),
                    namedClass);
                result.NamedClasses.Add(namedClass.Ident, namedClass);
            }
        }

        return result;
    }

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
        foreach (var astStylespace in stylesheet.AstStylespaces)
        {
            var result = new Stylespace([], [], [], []);
            foreach (var variable in astStylespace.Variables)
            {
                result.Variables.Add(variable.Ident, variable);
            }

            foreach (var anonymousClass in astStylespace.AnonymousClasses)
            {
                result.Classes.Add(anonymousClass.SelectorChain, anonymousClass);
                result.AnonymousClasses.Add(anonymousClass.SelectorChain, anonymousClass);
            }

            foreach (var namedClass in astStylespace.NamedClasses)
            {
                result.Classes.Add(
                    new SelectorChain(new ValueArray<SelectorExpr>([
                        new AtomSelectorExpr(new MarkerSelector(namedClass.Ident)),
                    ])),
                    namedClass);
                result.NamedClasses.Add(namedClass.Ident, namedClass);
            }

            yield return result;
        }
    }

    public static IEnumerable<Stylespace> ToApiStylespaces(this IEnumerable<Stylesheet> stylesheets)
    {
        return stylesheets.Select(ss => ss.ToApiStylespaces()).Merge();
    }

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

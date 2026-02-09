using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DelegateImplementation.Utils;

public class AttributeProcessor(AttributeData attributeData)
{
    private readonly object?[] _args = CreateArgsEnumerable(attributeData).ToArray();
    private int _index;

    // TODO: Add recursion support;
    public IEnumerable<T> GetArgs<T>(Func<T>? itemFallback = null)
    {
        var args = GetArg<ImmutableArray<TypedConstant>>(() => []);
        if (args.IsDefault)
        {
            yield break;
        }

        var a = CreateArgsEnumerable(args).ToArray();
        for (var i = 0; i < args.Length; i++)
        {
            yield return GetArg(a, i, itemFallback);
        }
    }

    public T GetArg<T>(Func<T>? fallback = null)
    {
        return GetArg(_args, _index++, fallback);
    }

    public static AttributeProcessor? Find(Compilation compilation, ISymbol symbol,
        string targetAttributeName)
    {
        return FindMultiple(compilation, symbol, targetAttributeName).FirstOrDefault();
    }

    public static IEnumerable<AttributeProcessor> FindMultiple(Compilation compilation, ISymbol symbol,
        string targetAttributeName)
    {
        var attrSymbol = compilation.GetTypeByMetadataName(targetAttributeName);
        return symbol.GetAttributes()
            .Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrSymbol))
            .Select(ad => new AttributeProcessor(ad));
    }
    
    private static T GetArg<T>(object?[] args, int i, Func<T>? fallback)
    {
        var value = args.Length > i ? args[i] is T t ? t : throw new ArgumentException() : Fallback();
        return value;
        T Fallback() => (fallback ?? throw new ArgumentException())();
    }

    private static IEnumerable<object?> CreateArgsEnumerable(AttributeData attributeData)
    {
        var args = attributeData.ConstructorArguments;
        return CreateArgsEnumerable(args);
    }

    private static IEnumerable<object?> CreateArgsEnumerable(ImmutableArray<TypedConstant> args)
    {
        var len = args.Length;
        for (var i = 0; i < len; i++)
        {
            object? value;
            try
            {
                value = args[i].Value;
            }
            catch
            {
                try
                {
                    value = args[i].Values;
                }
                catch
                {
                    value = null;
                }
            }

            yield return value;
        }
    }
}

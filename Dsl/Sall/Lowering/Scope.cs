using System;
using System.Collections.Generic;
using System.Linq;
using Sall.Api;
using Sall.Ast;

namespace Sall.Lowering;

public record Scope(
    Scope? Parent,
    Dictionary<string, ISymbol> Symbols
)
{
    public ISymbol? GetSymbol(string ident)
    {
        return Symbols.TryGetValue(ident, out var symbol) ? symbol : Parent?.GetSymbol(ident);
    }

    public T? GetSymbol<T>(string ident) where T : class, ISymbol
    {
        return GetSymbol(ident) as T;
    }

    public Dictionary<string, ISymbol> GetAllAvailableSymbols()
    {
        var merged = new Dictionary<string, ISymbol>(Symbols);
        foreach (var symbol in
                 (Parent?.GetAllAvailableSymbols() ?? []).Where(symbol => !merged.ContainsKey(symbol.Key)))
            merged[symbol.Key] = symbol.Value;
        return merged;
    }

    public void Add(string name, ISymbol symbol)
    {
        Symbols.Add(name, symbol);
    }

    public void AddOrReplace(string name, ISymbol symbol)
    {
        Symbols[name] = symbol;
    }

    public void AddIfNotExists(string name, ISymbol symbol)
    {
        if (!Symbols.ContainsKey(name)) Symbols[name] = symbol;
    }


    // TODO: Try to allow referencing other parameters in the default parameter values
    public void AddParamsToScope(Param[] @params, Args? args)
    {
        var ignoredIndexes = new List<int>();
        args ??= new Args([], []);
        var argsNamedExpressionIdentArray = args.NamedExpressions.Keys.ToArray();
        foreach (var argIdent in argsNamedExpressionIdentArray)
        {
            var arg = args.NamedExpressions[argIdent];
            var ident = argIdent;
            var paramIndex = Array.FindIndex(@params, p => p.Ident == ident);
            if (paramIndex != -1)
            {
                ignoredIndexes.Add(paramIndex);
                Symbols.Add(argIdent, new Variable(argIdent, arg));
            }
            else
            {
                throw new ArgumentException();
            }
        }

        var posI = -1;
        foreach (var expr in args.PositionalExpressions)
        {
            posI++;
            while (ignoredIndexes.Contains(posI))
            {
                posI++;
            }

            // TODO: CAN THROW
            var param = @params[posI];
            Symbols.Add(param.Ident, new Variable(param.Ident, expr));
        }

        while (posI < @params.Length)
        {
            posI++;
            while (ignoredIndexes.Contains(posI))
            {
                posI++;
            }

            var param = @params[posI];
            Symbols.Add(param.Ident, new Variable(param.Ident, param.DefaultValue));
        }
    }

    public static Scope FromStylespace(Stylespace stylespace)
    {
        var symbols = new Dictionary<string, ISymbol>();
        foreach (var variable in stylespace.Variables)
        {
            symbols[variable.Key] = variable.Value;
        }

        foreach (var namedClass in stylespace.NamedClasses)
        {
            if (symbols.ContainsKey(namedClass.Key)) throw new Exception();
            symbols[namedClass.Key] = namedClass.Value;
        }

        return new Scope(null, symbols);
    }

    public void AddSymbolsFrom(Scope otherScope)
    {
        foreach (var symbol in otherScope.Symbols)
        {
            if (Symbols.ContainsKey(symbol.Key))
                // TODO (later): Message and exception type
                throw new Exception();
            Symbols.Add(symbol.Key, symbol.Value);
        }
    }
}

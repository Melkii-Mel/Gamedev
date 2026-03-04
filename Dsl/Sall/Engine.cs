using System;
using System.Collections.Generic;
using System.Linq;
using Primitives;

namespace Sall;

public static class Engine
{
}

public class Evaluator(Scope scope)
{
    public Value Eval(Expr expr)
    {
        return expr switch
        {
            BinaryExpr binaryExpr => EvalBinary(binaryExpr),
            AtomExpr atomExpr => atomExpr.ExprOrValue switch
            {
                Expr expr1 => Eval(expr1),
                Value value => value,
                _ => throw new ArgumentOutOfRangeException(),
            },
            UnaryExpr unaryExpr => unaryExpr.Operation switch
            {
                UnaryOperation.Negative or UnaryOperation.Not => NormalizeValue(Eval(unaryExpr.Expr)) switch
                {
                    Bool b => new Bool(!b.Value),
                    Color c => new Color(-c.Value),
                    Double d => new Double(-d.Value),
                    Size size => new Size(size.ValuePerUnit.ToDictionary(kvp => kvp.Key, kvp => -kvp.Value)),
                    _ => throw new ArgumentOutOfRangeException(),
                },
                _ => throw new ArgumentOutOfRangeException(),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(expr), expr, null),
        };
    }

    public INormalizedValue NormalizeValue(Value value)
    {
        return value switch
        {
            INormalizedValue v => v,
            Call call => NormalizeValue(Exec(call)),
            Uint u => new Double(u.Value),
            VariableRef variableRef => NormalizeValue(Deref(variableRef)),
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }

    private Value EvalBinary(BinaryExpr binaryExpr)
    {
        return binaryExpr.Operation switch
        {
            BinaryOperation.Or => BoolOp((a, b) => a || b),
            BinaryOperation.And => BoolOp((a, b) => a && b),
            BinaryOperation.Lt => BoolOpNums((a, b) => Compare(a, b) < 0),
            BinaryOperation.Le => BoolOpNums((a, b) => Compare(a, b) <= 0),
            BinaryOperation.Gt => BoolOpNums((a, b) => Compare(a, b) > 0),
            BinaryOperation.Ge => BoolOpNums((a, b) => Compare(a, b) >= 0),
            BinaryOperation.Eq => BoolOpNums((a, b) => Compare(a, b) == 0),
            BinaryOperation.Ne => BoolOpNums((a, b) => Compare(a, b) != 0),
            BinaryOperation.Add => Op(binaryExpr, BinaryOperation.Add),
            BinaryOperation.Subtract => Op(binaryExpr, BinaryOperation.Subtract),
            BinaryOperation.Multiply => Op(binaryExpr, BinaryOperation.Multiply),
            BinaryOperation.Divide => Op(binaryExpr, BinaryOperation.Divide),
            BinaryOperation.Remainder => Op(binaryExpr, BinaryOperation.Remainder),
            _ => throw new ArgumentOutOfRangeException(),
        };

        Value BoolOpNums(Func<IComparable, IComparable, bool> opFunc)
        {
            return new Bool(opFunc(ToComparable(Eval(binaryExpr.Left)), ToComparable(Eval(binaryExpr.Right))));
        }

        Value BoolOp(Func<bool, bool, bool> opFunc)
        {
            return new Bool(opFunc(ToBool(Eval(binaryExpr.Left)), ToBool(Eval(binaryExpr.Right))));
        }
    }

    private IComparable ToComparable(Value value)
    {
        return NormalizeValue(value) switch
        {
            Bool b => b.Value,
            Color color => throw new ArgumentException(),
            Double d => d.Value,
            Size size => ToAbsoluteSize(size),
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }

    private int Compare(IComparable c0, IComparable c1)
    {
        // ReSharper disable once InvertIf
        if (c0 is double d0 && c1 is double d1 && Math.Abs(d0 - d1) < Defaults.DTolerance)
            return 0;

        return c0.CompareTo(c1);
    }

    private Value Op(BinaryExpr binaryExpr, BinaryOperation op)
    {
        var a = Normalize(Eval(binaryExpr));
        var b = Normalize(Eval(binaryExpr));
        return a switch
        {
            Color ca => b switch
            {
                // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                Color cb => new Color(op switch
                {
                    BinaryOperation.Add => ca.Value + cb.Value,
                    BinaryOperation.Subtract => ca.Value - cb.Value,
                    _ => throw new ArgumentException(),
                }),

                _ => throw new NotSupportedException(),
            },
            Double da => b switch
            {
                Double db => new Double(da.Value + db.Value),
                _ => throw new NotSupportedException(),
            },
            Size sa => b switch
            {
                Size sb => sa + sb,
                _ => throw new NotSupportedException(),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(a)),
        };

        // ReSharper disable once TailRecursiveCall
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        Value Normalize(Value v)
        {
            if (v is Call call) return Normalize(Exec(call));
            if (v is VariableRef vRef) Normalize(Deref(vRef));
            if (v is Bool vb) return new Double(vb.Value ? 1 : 0);
            if (v is Uint u) return new Double(u.Value);

            return v;
        }
    }

    private Value Exec(Call call)
    {
        Scope? currentScope = scope;
        Variable? variable = null;
        while (currentScope != null)
        {
            if (scope.GetSymbol(call.Ident) is Variable v)
            {
                if (v.Params.Length != call.Args.NamedExpressions.Count + call.Args.PositionalExpressions.Length)
                    continue;
                variable = v;
                break;
            }

            currentScope = currentScope.Parent;
        }


        if (variable is null) throw new KeyNotFoundException();
        foreach (var param in variable.Params)
        {
            if (call.Args.NamedExpressions.ContainsKey(param.Ident))
            {
            }
        }
    }

    private Value Deref(VariableRef vRef)
    {
    }

    private Type GetType(Value value)
    {
        return value.GetType();
    }

    private Type GetType(Expr expr)
    {
        return GetType(Eval(expr));
    }

    private bool ToBool(Value value)
    {
        return value switch
        {
            Bool b => b.Value,
            Call call => ToBool(Exec(call)),
            Color color => !(color.Value.A == 0 || color.Value is { G: 0, B: 0, R: 0 }),
            Double d => d.Value == 0,
            Uint u => u.Value == 0,
            Size s => s.ValuePerUnit.Values.Sum() == 0,
            VariableRef variableRef => ToBool(Deref(variableRef)),
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }
}

public record Stylespace(
    Scope Scope,
    Dictionary<SelectorChain, Class> Classes
)
{
    public static Stylespace FromStylesheets(params Stylesheet[] stylesheets)
    {
        var symbols = new Dictionary<string, ISymbol>();
        var classes = new Dictionary<SelectorChain, Class>();
        foreach (var stylesheet in stylesheets)
        {
            foreach (var variable in stylesheet.Variables)
            {
                symbols.Add(variable.Ident, variable);
            }

            foreach (var anonymousClass in stylesheet.AnonymousClasses)
            {
                classes.Add(anonymousClass.SelectorChain, anonymousClass);
            }

            foreach (var namedClass in stylesheet.NamedClasses)
            {
                classes.Add(new SelectorChain(new []{new AtomSelectorExpr() namedClass.Ident}));
            }
        }
        return new Stylespace(new Scope(null, symbols), classes);
    }
}

public record Scope(
    Scope? Parent,
    Dictionary<string, ISymbol> Symbols
)
{
    public ISymbol? GetSymbol(string ident)
    {
        return Symbols.TryGetValue(ident, out var symbol) ? symbol : Parent?.GetSymbol(ident);
    }

    public Dictionary<string, ISymbol> GetAllAvailableSymbols()
    {
        var merged = new Dictionary<string, ISymbol>(Symbols);
        foreach (var symbol in
                 (Parent?.GetAllAvailableSymbols() ?? []).Where(symbol => !merged.ContainsKey(symbol.Key)))
            merged[symbol.Key] = symbol.Value;
        return merged;
    }
}

using System;
using System.Linq;
using Primitives;
using Sall.Ast;
using Sall.Lowering;
using Color = Sall.Ast.Color;
using Double = Sall.Ast.Double;

namespace Sall.Evaluation;

// TODO: Add [pw, ph, sw, sh] to the .g4
public readonly record struct EvaluationContext(Scope Scope, LayoutContext LayoutContext);

public class Evaluator
{
    public static Value Eval(Expr expr, EvaluationContext ctx)
    {
        return expr switch
        {
            BinaryExpr binaryExpr => EvalBinary(binaryExpr, ctx),
            AtomExpr atomExpr => atomExpr.ExprOrValue switch
            {
                Expr expr1 => Eval(expr1, ctx),
                Value value => value,
                _ => throw new ArgumentOutOfRangeException(),
            },
            UnaryExpr unaryExpr => unaryExpr.Operation switch
            {
                UnaryOperation.Negative or UnaryOperation.Not => NormalizeValue(Eval(unaryExpr.Expr, ctx), ctx) switch
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

    public static INormalizedValue NormalizeValue(Value value, EvaluationContext ctx)
    {
        return value switch
        {
            INormalizedValue v => v,
            Call call => NormalizeValue(Exec(call, ctx), ctx),
            Uint u => new Double(u.Value),
            VariableRef variableRef => NormalizeValue(Deref(variableRef, ctx), ctx),
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }

    private static Value EvalBinary(BinaryExpr binaryExpr, EvaluationContext ctx)
    {
        return binaryExpr.Operation switch
        {
            BinaryOperation.Or => BoolOp((a, b) => a || b),
            BinaryOperation.And => BoolOp((a, b) => a && b),
            BinaryOperation.Lt => NumComp((a, b) => Compare(a, b) < 0),
            BinaryOperation.Le => NumComp((a, b) => Compare(a, b) <= 0),
            BinaryOperation.Gt => NumComp((a, b) => Compare(a, b) > 0),
            BinaryOperation.Ge => NumComp((a, b) => Compare(a, b) >= 0),
            BinaryOperation.Eq => NumComp((a, b) => Compare(a, b) == 0),
            BinaryOperation.Ne => NumComp((a, b) => Compare(a, b) != 0),
            BinaryOperation.Add => Op(binaryExpr, BinaryOperation.Add, ctx),
            BinaryOperation.Subtract => Op(binaryExpr, BinaryOperation.Subtract, ctx),
            BinaryOperation.Multiply => Op(binaryExpr, BinaryOperation.Multiply, ctx),
            BinaryOperation.Divide => Op(binaryExpr, BinaryOperation.Divide, ctx),
            BinaryOperation.Remainder => Op(binaryExpr, BinaryOperation.Remainder, ctx),
            _ => throw new ArgumentOutOfRangeException(),
        };

        Value NumComp(Func<IComparable, IComparable, bool> opFunc)
        {
            return new Bool(opFunc(ToComparable(Eval(binaryExpr.Left, ctx), ctx),
                ToComparable(Eval(binaryExpr.Right, ctx), ctx)));
        }

        Value BoolOp(Func<bool, bool, bool> opFunc)
        {
            return new Bool(opFunc(ToBool(Eval(binaryExpr.Left, ctx), ctx), ToBool(Eval(binaryExpr.Right, ctx), ctx)));
        }
    }

    private static IComparable ToComparable(Value value, EvaluationContext ctx)
    {
        return NormalizeValue(value, ctx) switch
        {
            Bool b => b.Value,
            Color => throw new ArgumentException(),
            Double d => d.Value,
            Size size => ToAbsoluteSize(size, ctx),
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }

    private static double ToAbsoluteSize(Size size, EvaluationContext ctx)
    {
        var layoutContext = ctx.LayoutContext;
        return (from unit in size.ValuePerUnit
            let v = unit.Value
            select unit.Key switch
            {
                SizeUnit.Px => v,
                SizeUnit.Pw => layoutContext.ParentSize.X * v,
                SizeUnit.Ph => layoutContext.ParentSize.Y * v,
                SizeUnit.Vw => layoutContext.RootSize.X * v,
                SizeUnit.Vh => layoutContext.RootSize.Y * v,
                // TODO: Guard against circular dependencies
                SizeUnit.Sw => EvalDouble(layoutContext.SelfWidth) * v,
                SizeUnit.Sh => EvalDouble(layoutContext.SelfHeight) * v,
                SizeUnit.Em => layoutContext.ParentFontSize * v,
                SizeUnit.Rem => layoutContext.RootFontSize * v,
                _ => throw new ArgumentOutOfRangeException(),
            }).Sum();

        double EvalDouble(Expr expr)
        {
            return ((Double)Eval(expr, ctx)).Value;
        }
    }

    private static int Compare(IComparable c0, IComparable c1)
    {
        // ReSharper disable once InvertIf
        if (c0 is double d0 && c1 is double d1 && Math.Abs(d0 - d1) < Defaults.DTolerance)
            return 0;

        return c0.CompareTo(c1);
    }

    private static Value Op(BinaryExpr binaryExpr, BinaryOperation op, EvaluationContext ctx)
    {
        var a = Normalize(Eval(binaryExpr, ctx));
        var b = Normalize(Eval(binaryExpr, ctx));
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
            if (v is Call call) return Normalize(Exec(call, ctx));
            if (v is VariableRef vRef) Normalize(Deref(vRef, ctx));
            if (v is Bool vb) return new Double(vb.Value ? 1 : 0);
            if (v is Uint u) return new Double(u.Value);

            return v;
        }
    }

    private static Value Exec(Call call, EvaluationContext ctx)
    {
        return Deref(new VariableRef(call.Ident), ctx, call.Args);
    }

    private static Value Deref(VariableRef vRef, EvaluationContext ctx, Args? args = null)
    {
        var scope = ctx.Scope;
        var layoutContext = ctx.LayoutContext;
        var symbol = scope.GetSymbol(vRef.Ident);
        if (symbol is not Variable variable)
        {
            throw new ArgumentException();
        }

        if (variable.Params.Length > 0 && args is null) return variable;

        var variableScope = new Scope(scope, []);
        variableScope.AddParamsToScope(variable.Params, args);
        var evaluator = new EvaluationContext(variableScope, layoutContext);

        return ComputeVar(variable, evaluator, variableScope);
    }

    private static Value ComputeVar(Variable variable, EvaluationContext ctx, Scope variableScope)
    {
        foreach (var statement in variable.Statements)
        {
            switch (statement)
            {
                case VariableStatementVariable variableStatementVariable:
                    var v = variableStatementVariable.Variable;
                    if (v.Params.Length == 0)
                        variableScope.Add(v.Ident,
                            new BakedVariable(v.Ident, ComputeVar(variable, ctx, variableScope)));
                    else variableScope.Add(v.Ident, v);
                    break;
                case VariableStatementExpr expr:
                    Eval(expr.Expr, ctx);
                    break;
            }
        }

        return Eval(variable.Result, ctx);
    }

    private static Type GetType(Value value)
    {
        return value.GetType();
    }

    private static Type GetType(Expr expr, EvaluationContext ctx)
    {
        return GetType(Eval(expr, ctx));
    }

    private static bool ToBool(Value value, EvaluationContext ctx)
    {
        return value switch
        {
            Bool b => b.Value,
            Call call => ToBool(Exec(call, ctx), ctx),
            Color color => !(color.Value.A == 0 || color.Value is { G: 0, B: 0, R: 0 }),
            Double d => d.Value == 0,
            Uint u => u.Value == 0,
            Size s => s.ValuePerUnit.Values.Sum() == 0,
            VariableRef variableRef => ToBool(Deref(variableRef, ctx), ctx),
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }
}

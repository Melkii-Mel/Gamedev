using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sall;

public enum BinaryOperation
{
    Or,
    And,
    Lt,
    Le,
    Gt,
    Ge,
    Eq,
    Ne,
    Add,
    Subtract,
    Multiply,
    Divide,
    Remainder,
}

public enum UnaryOperation
{
    Negative,
    Not,
}

public enum BinarySelectorOperation
{
    Or,
    And,
}

public enum UnarySelectorOperation
{
    Not,
}

public enum SizeUnit
{
    Px,
    Pw,
    Ph,
    Sw,
    Sh,
    Em,
    Rem,
    Vh,
    Vw,
}

public enum Comp
{
    Eq,
    Ne,
    Lt,
    Gt,
    Le,
    Ge,
}

public record Stylesheet(Variable[] Variables, AnonymousClass[] AnonymousClasses, NamedClass[] NamedClasses);

public abstract record ExprOrValue;

public abstract record Expr : ExprOrValue;

public record BinaryExpr(BinaryOperation Operation, Expr Left, Expr Right) : Expr;

public record UnaryExpr(UnaryOperation Operation, UnaryOrAtomExpr Expr) : UnaryOrAtomExpr;

public record AtomExpr(ExprOrValue ExprOrValue) : UnaryOrAtomExpr;

public abstract record UnaryOrAtomExpr : Expr;

public interface INormalizedValue;

public abstract record Value : ExprOrValue;

public record Bool(bool Value) : Value, INormalizedValue;

public abstract record Number : Value;

public record Uint(uint Value) : Number;

public record Double(double Value) : Number, INormalizedValue;

public record Size(Dictionary<SizeUnit, double> ValuePerUnit) : Value, INormalizedValue
{
    public static Size operator +(Size a, Size b)
    {
        return Op(a, b, (da, db) => da + db);
    }

    public static Size operator -(Size a, Size b)
    {
        return Op(a, b, (da, db) => da - db);
    }

    private static Size Op(Size a, Size b, Func<double, double, double> op)
    {
        var result = new Dictionary<SizeUnit, double>(b.ValuePerUnit);

        foreach (var kvp in a.ValuePerUnit)
        {
            var (unit, value) = (kvp.Key, kvp.Value);
            result.TryGetValue(unit, out var existing);
            result[unit] = op(existing, value);
        }

        return new Size(result);
    }
}

public record Color(Primitives.Color Value) : Value, INormalizedValue;

public record Call(string Ident, Args Args) : Value;

public record VariableRef(string Ident) : Value;

public abstract record Class(Parent[] Parents, Property[] Properties, AnonymousClass[] SubClasses);

public record AnonymousClass(
    SelectorChain SelectorChain,
    Parent[] Parents,
    Property[] Properties,
    AnonymousClass[] SubClasses)
    : Class(Parents, Properties, SubClasses);

public record NamedClass(
    string Ident,
    Param[] Params,
    Parent[] Parents,
    Property[] Properties,
    AnonymousClass[] SubClasses)
    : Class(Parents, Properties, SubClasses), ISymbol;

public record NormalizedClass(SelectorChain SelectorChain, Dictionary<string, Value> PropertyValues);

public record Parent(string Ident, Args Args);

public record Property(string Ident, Expr Expr);

public abstract record Selector : SelectorExprOrSelector;

public abstract record SelectorExprOrSelector;

public abstract record SelectorExpr : SelectorExprOrSelector;

public abstract record UnaryOrAtomSelectorExpr : SelectorExpr;

public record BinarySelectorExpr(BinarySelectorOperation Operation, SelectorExpr Left, SelectorExpr Right)
    : SelectorExpr;

public record UnarySelectorExpr(UnarySelectorOperation Operation, UnaryOrAtomSelectorExpr UnaryOrAtomSelectorExpr)
    : UnaryOrAtomSelectorExpr;

public record AtomSelectorExpr(SelectorExprOrSelector SelectorExprOrSelector) : UnaryOrAtomSelectorExpr;

public record SelectorChain(ValueArray<SelectorExpr> Selectors) : SelectorExpr;

public record UiSelector(string Ident) : Selector;

public record MarkerSelector(string Ident) : Selector;

public record StateMapSelector(State[] StateMap) : Selector;

public abstract record AxesSelector : Selector;

public record ReverseSelector : Selector;

public record UniqueSelector : Selector;

public record ChildrenSelector(Range? Range) : AxesSelector;

public record ParentSelector : AxesSelector;

public record LeftSiblingsSelector(Range? Range) : AxesSelector;

public record RightSiblingsSelector(Range? Range) : AxesSelector;

public record SliceSelector(Range Range) : Selector;

public abstract record Range;

public record PointRange(Expr Expr) : Range;

public record RightUnboundedRange(Expr Expr) : Range;

public record LeftUnboundedRange(Expr Expr) : Range;

public record BoundedRange(Expr Left, Expr Right) : Range;

public interface ISymbol
{
    string Ident { get; }
}

public record State(string Ident, Comp? Comp, Expr? Expr);

public record Variable(string Ident, Param[] Params, VariableStatement[] Statements, Expr Result) : Value, ISymbol
{
    public Variable(string ident, Expr result) : this(ident, [], [], result)
    {
    }
}

public record BakedVariable(string Ident, Value Value) : Value, ISymbol;

public abstract record VariableStatement;

public record VariableStatementVariable(Variable Variable) : VariableStatement;

public record VariableStatementExpr(Expr Expr) : VariableStatement;

public record Args(Expr[] PositionalExpressions, Dictionary<string, Expr> NamedExpressions);

public record Param(string Ident, Expr DefaultValue);

public readonly record struct ValueArray<T>(T[] Items) where T : IEquatable<T>
{
    public bool Equals(ValueArray<T> other) =>
        Items.AsSpan().SequenceEqual(other.Items.AsSpan());

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var item in Items)
            hash.Add(item);
        return hash.ToHashCode();
    }
}

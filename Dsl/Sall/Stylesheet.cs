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
    Percent,
    Em,
    Rem,
    Vh,
    Vw,
}

public record Stylesheet(Variable[] Variables, AnonymousClass[] AnonymousClasses, NamedClass[] NamedClasses);

public abstract record ExprOrValue;

public abstract record Expr : ExprOrValue;

public record BinaryExpr(BinaryOperation Operation, Expr Left, Expr Right) : Expr;

public record UnaryExpr(UnaryOperation Operation, UnaryOrAtomExpr Expr) : UnaryOrAtomExpr;

public record AtomExpr(ExprOrValue ExprOrValue) : UnaryOrAtomExpr;

public abstract record UnaryOrAtomExpr : Expr;

public abstract record Value : ExprOrValue;

public record Bool(bool Value) : Value;

public abstract record Number : Value;

public record Uint(uint Value) : Number;

public record Double(double Value) : Number;

public record Size(double Value, SizeUnit Unit) : Value;

public record Color(Primitives.Color Value) : Value;

public record Call(string Ident, Args Args) : Value;

public record VariableRef(string Ident) : Value;

public abstract record Class(Parent[] Parents, Property[] Properties, AnonymousClass[] SubClasses);

public record AnonymousClass(
    SelectorChain SelectorChain,
    Parent[] Parents,
    Property[] Properties,
    AnonymousClass[] SubClasses)
    : Class(Parents, Properties, SubClasses);

public record NamedClass(string Name, Parent[] Parents, Property[] Properties, AnonymousClass[] SubClasses)
    : Class(Parents, Properties, SubClasses);

public record Parent(string Ident, Args Args);

public record Property(string Ident, Expr Expr);

public record SelectorChain(SelectorExpr[] Selectors) : SelectorExpr;

public abstract record SelectorExprOrSelector;

public abstract record SelectorExpr : SelectorExprOrSelector;

public abstract record UnaryOrAtomSelectorExpr : SelectorExpr;

public record BinarySelectorExpr(BinarySelectorOperation Operation, SelectorExpr Left, SelectorExpr Right)
    : SelectorExpr;

public record UnarySelectorExpr(UnarySelectorOperation Operation, UnaryOrAtomSelectorExpr UnaryOrAtomSelectorExpr)
    : UnaryOrAtomSelectorExpr;

public record AtomSelectorExpr(SelectorExprOrSelector SelectorExprOrSelector) : UnaryOrAtomSelectorExpr;

public abstract record Selector : SelectorExprOrSelector;

public record UiSelector(string Ident) : Selector;

public record MarkerSelector(string Ident) : Selector;

public record StateMapSelector(State[] StateMap) : Selector;

public abstract record AxesSelector : Selector;

public record ReverseSelector() : Selector;

public record UniqueSelector() : Selector;

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

public record State(string Ident, Expr? Expr);

public record Variable(string Name, Param[] Params, Expr Expr);

public record Args(Expr[] Expressions);

public record Param(string Ident, Expr DefaultValue);

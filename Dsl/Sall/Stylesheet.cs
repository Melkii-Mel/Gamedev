namespace Sall;

public enum BinOperation
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

public enum UnOperation
{
    Negative,
    Not,
}

public record Stylesheet(Variable[] Variables, Class[] Classes);

public abstract record ExprOrValue;

public abstract record Expr : ExprOrValue;

public record BinaryExpr(BinOperation Operation, Expr Left, Expr Right) : Expr;

public record UnaryExpr(UnOperation Operation, UnaryOrAtomExpr Expr) : UnaryOrAtomExpr;

public record AtomExpr(ExprOrValue ExprOrValue) : UnaryOrAtomExpr;

public abstract record UnaryOrAtomExpr : Expr;

public abstract record Value : ExprOrValue;

public abstract record Number : Value;

public record Uint(uint Value) : Number;

public record Double(double Value) : Number;

public record Size(double Value, SizeUnit Unit) : Value;

public record Color(Primitives.Color Value) : Value;

public record Call(string Ident, Args Args) : Expr;

public record VariableRef(string Ident) : Expr;

public record Class(Selector Selector, Parent[] Parents, Property[] Properties, Class[] SubClasses);

public record Parent(string Ident, Args Args);

public record Property(string Ident, Expr Expr);

public abstract record Selector(State[] StateMap);

public record UiSelector(string Ident, State[] StateMap) : Selector(StateMap);

public record CustomSelector(string Ident, Param[] Params, State[] StateMap) : Selector(StateMap);

public record RelationSelector(Relation Relation, State[] StateMap) : Selector(StateMap);

public enum Relation
{
    Children,
    Parent,
    Siblings,
}

public record State(string Ident, Expr? Expr);

public enum SizeUnit
{
    Px,
    Percent,
    Em,
    Rem,
    Vh,
    Vw,
}

public record Variable(string Name, Param[] Params, Expr Expr);

public record Args(Expr[] Expressions);

public record Param(string Ident, Expr DefaultValue);

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime;

namespace Sall;

public class SallVisitor
{
    public static Stylesheet Visit(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var lexer = new sallLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new sallParser(tokenStream);

        var context = parser.file();
        return VisitFile(context);
    }

    public static Stylesheet VisitFile(sallParser.FileContext context)
    {
        List<Variable> variables = [];
        List<Class> classes = [];

        foreach (var s in context.statement())
            if (s.variable() != null)
                variables.Add(VisitVariable(s.variable()));
            else if (s.classDef() != null) classes.Add(VisitClassDef(s.classDef()));

        return new Stylesheet(variables.ToArray(), classes.ToArray());
    }

    public static Variable VisitVariable(sallParser.VariableContext context)
    {
        return new Variable(context.IDENT().GetText(), VisitParams(context.@params()), VisitExpr(context.expr()));
    }

    public static Expr VisitExpr(sallParser.ExprContext context)
    {
        var precedenceChain = new List<Func<ParserRuleContext, ParserRuleContext>>
        {
            ctx => ((sallParser.L6ExprContext)ctx).l5Expr(0),
            ctx => ((sallParser.L5ExprContext)ctx).l4Expr(0),
            ctx => ((sallParser.L4ExprContext)ctx).l3Expr(0),
            ctx => ((sallParser.L3ExprContext)ctx).l2Expr(0),
            ctx => ((sallParser.L2ExprContext)ctx).l1Expr(0),
        };

        return VisitBinRecursive(context.l6Expr(), precedenceChain, 0);

        Expr VisitBinRecursive(ParserRuleContext ctx, List<Func<ParserRuleContext, ParserRuleContext>> chain, int level)
        {
            if (level == chain.Count) return VisitL1Expr((sallParser.L1ExprContext)ctx);

            var getNext = chain[level];
            var left = VisitBinRecursive(getNext(ctx), chain, level + 1);

            for (var i = 1; i < ctx.ChildCount; i += 2)
            {
                var opNode = ctx.GetChild(i);
                var rightNode = ctx.GetChild(i + 1);
                var right = VisitBinRecursive((ParserRuleContext)rightNode, chain, level + 1);
                left = new BinaryExpr(OperatorMap.BinOperations[opNode.GetText()], left, right);
            }

            return left;
        }
    }

    public static UnaryOrAtomExpr VisitL1Expr(sallParser.L1ExprContext l1)
    {
        var atom = l1.atom();
        ExprOrValue exprOrValue = atom.ChildCount == 3
            ? VisitExpr(atom.expr())
            : atom.value() switch
            {
                var c when c.@float() != null => new Double(
                    double.Parse(string.Join("", c.@float().children), CultureInfo.InvariantCulture)),
                var c when c.IDENT() != null => new VariableRef(c.IDENT().GetText()),
                var c when c.COLOR() != null => new Color(
                    Primitives.Color.ParseSmart(c.COLOR().GetText())
                ),
                var c when c.call() != null => new Call(c.call().IDENT().GetText(), VisitArgs(c.call().args())),
                var c when c.sizeValue() != null => new Size(
                    double.Parse(string.Join("", c.sizeValue().@float().children), CultureInfo.InvariantCulture),
                    c.sizeValue().UNIT().GetText() switch
                    {
                        "px" => SizeUnit.Px,
                        "%" => SizeUnit.Percent,
                        "em" => SizeUnit.Em,
                        "rem" => SizeUnit.Rem,
                        "vh" => SizeUnit.Vh,
                        "vw" => SizeUnit.Vw,
                        _ => throw new ArgumentOutOfRangeException(),
                    }),
                var c when c.@uint() != null => new Uint(uint.Parse(c.@uint().DIGITS().GetText())),
                _ => throw new ArgumentOutOfRangeException(),
            };

        var atomExpr = new AtomExpr(exprOrValue);

        return l1.l1Op()
            .Reverse()
            .Where(l1Op => l1Op.PLUS() == null)
            .Aggregate<sallParser.L1OpContext, UnaryOrAtomExpr>(atomExpr, (current, l1Op) =>
                new UnaryExpr(l1Op switch
                {
                    _ when l1Op.MINUS() != null => UnOperation.Negative,
                    _ when l1Op.EXCLAMATION() != null => UnOperation.Not,
                    _ => throw new ArgumentOutOfRangeException(),
                }, current));
    }

    public static Args VisitArgs(sallParser.ArgsContext? context)
    {
        return new Args(context?.expr()
            .Select(VisitExpr)
            .ToArray() ?? []);
    }

    public static Class VisitClassDef(sallParser.ClassDefContext context)
    {
        return VisitClassDef(Selector(context.selector()), context.classContent());

        Selector Selector(sallParser.SelectorContext ctx)
        {
            var stateMap = VisitStateMap(ctx.stateMap());
            return ctx switch
            {
                _ when ctx.uiSelector() != null => VisitUiSelector(ctx.uiSelector(), stateMap),
                _ when ctx.customSelector() != null => new CustomSelector(ctx.customSelector().IDENT().GetText(),
                    VisitParams(ctx.customSelector().@params()), stateMap),
                _ => throw new ArgumentOutOfRangeException(nameof(ctx), ctx, null),
            };
        }
    }

    public static UiSelector VisitUiSelector(sallParser.UiSelectorContext ctx, State[] stateMap)
    {
        return new UiSelector(ctx.IDENT().GetText(), stateMap);
    }

    public static State[] VisitStateMap(sallParser.StateMapContext? context)
    {
        return context?.state().Select(s =>
            new State(s.IDENT()?.GetText() ?? s.stateKvp().IDENT().GetText(),
                s.stateKvp() != null ? VisitExpr(s.stateKvp().expr()) : null)).ToArray() ?? [];
    }

    public static Class VisitClassDef(Selector selector, sallParser.ClassContentContext classContent)
    {
        var classBody = classContent.classBodyOrTerminator().classBody();
        return new Class(
            selector,
            Parents(classContent.parentsList()?.parent() ?? []),
            Properties(classBody?.classBodyItem().Select(cbi => cbi.property()).Where(p => p != null) ?? []),
            SubClasses(classBody?.classBodyItem().Select(cbi => cbi.subClassDef()).Where(scd => scd != null) ?? [])
        );

        Parent[] Parents(sallParser.ParentContext[] ctx)
        {
            return ctx.Select(p => new Parent(p.IDENT().GetText(), VisitArgs(p.args()))).ToArray();
        }

        Property[] Properties(IEnumerable<sallParser.PropertyContext> ctx)
        {
            return ctx.Select(p => new Property(p.IDENT().GetText(), VisitExpr(p.expr()))).ToArray();
        }

        Class[] SubClasses(IEnumerable<sallParser.SubClassDefContext> ctx)
        {
            return ctx.Select(sc => VisitClassDef(SubSelector(sc.subSelector()), sc.classContent())).ToArray();
        }

        Selector SubSelector(sallParser.SubSelectorContext subSelector)
        {
            var stateMap = VisitStateMap(subSelector.stateMap());
            return subSelector switch
            {
                _ when subSelector.uiSelector() != null => VisitUiSelector(subSelector.uiSelector(), stateMap),
                _ when subSelector.childrenSelector() != null => new RelationSelector(Relation.Children, stateMap),
                _ when subSelector.parentSelector() != null => new RelationSelector(Relation.Parent, stateMap),
                _ when subSelector.siblingsSelector() != null => new RelationSelector(Relation.Siblings, stateMap),
                _ => throw new ArgumentOutOfRangeException(nameof(subSelector), subSelector, null),
            };
        }
    }

    public static Param[] VisitParams(sallParser.ParamsContext? context)
    {
        return context?.paramList()?.param().Select(p => new Param(p.IDENT().GetText(), VisitExpr(p.expr())))
            .ToArray() ?? [];
    }
}

public static class OperatorMap
{
    public static readonly Dictionary<string, BinOperation> BinOperations = new()
    {
        { "||", BinOperation.Or },
        { "&&", BinOperation.And },
        { "<", BinOperation.Lt },
        { "<=", BinOperation.Le },
        { ">", BinOperation.Gt },
        { ">=", BinOperation.Ge },
        { "==", BinOperation.Eq },
        { "!=", BinOperation.Ne },
        { "+", BinOperation.Add },
        { "-", BinOperation.Subtract },
        { "*", BinOperation.Multiply },
        { "/", BinOperation.Divide },
        { "%", BinOperation.Remainder },
    };
}

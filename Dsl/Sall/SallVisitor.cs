using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

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
        List<AnonymousClass> anonymousClasses = [];
        List<NamedClass> namedClasses = [];

        foreach (var s in context.statement())
            if (s.variable() != null)
                variables.Add(VisitVariable(s.variable()));
            else if (s.anonymousClassDef() != null)
                anonymousClasses.Add(VisitAnonymousClassDef(s.anonymousClassDef()));
            else if (s.namedClassDef() != null) namedClasses.Add(VisitNamedClassDef(s.namedClassDef()));

        return new Stylesheet(variables.ToArray(), anonymousClasses.ToArray(), namedClasses.ToArray());
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

        return VisitBinRecursive<Expr>(context.l6Expr(), precedenceChain,
            ctx => VisitL1Expr((sallParser.L1ExprContext)ctx),
            (s, l, r) => new BinaryExpr(OperatorMap.BinOperations[s], l, r));
    }

    public static UnaryOrAtomExpr VisitL1Expr(sallParser.L1ExprContext l1)
    {
        return VisitUnRecursive<UnaryOrAtomExpr, UnaryOperation, sallParser.L1OpContext, AtomExpr>(
            VisitAtomExpr(l1.atom()), l1.l1Op(), [c => c.PLUS() == null], OperatorMap.UnOperations,
            (op, c) => new UnaryExpr(op, c));
    }

    public static AtomExpr VisitAtomExpr(sallParser.AtomContext ctx)
    {
        return new AtomExpr(VisitAtom<sallParser.AtomContext, ExprOrValue, Expr, Value>(ctx, c => VisitExpr(c.expr()),
            c => VisitValue(c.value())));
    }

    public static Value VisitValue(sallParser.ValueContext c)
    {
        return c switch
        {
            _ when c.@float() != null => new Double(
                double.Parse(string.Join("", c.@float().children), CultureInfo.InvariantCulture)),
            _ when c.IDENT() != null => new VariableRef(c.IDENT().GetText()),
            _ when c.COLOR() != null => new Color(
                Primitives.Color.ParseSmart(c.COLOR().GetText())
            ),
            _ when c.call() != null => new Call(c.call().IDENT().GetText(),
                VisitArgs(c.call().args())),
            _ when c.sizeValue() != null => new Size(
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
            _ when c.@uint() != null => new Uint(uint.Parse(c.@uint().DIGITS().GetText())),
            _ when c.@bool() != null => new Bool(bool.Parse(c.@bool().GetText())),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static Args VisitArgs(sallParser.ArgsContext? context)
    {
        return new Args(context?.expr()
            .Select(VisitExpr)
            .ToArray() ?? []);
    }

    public static SelectorChain VisitSelectorExpr(sallParser.SelectorExprContext ctx)
    {
        var precedenceChain = new List<Func<ParserRuleContext, ParserRuleContext>>
        {
            c => ((sallParser.L3SelContext)c).l2Sel(0),
            c => ((sallParser.L2SelContext)c).l1Sel(0),
        };

        return new SelectorChain(ctx.l4Sel().l3Sel().Select(c =>
            VisitBinRecursive<SelectorExpr>(c, precedenceChain, prc => VisitL1Selector((sallParser.L1SelContext)prc),
                (s, l, r) => new BinarySelectorExpr(OperatorMap.BinSelectorOperations[s], l, r))).ToArray());
    }

    public static UnaryOrAtomSelectorExpr VisitL1Selector(sallParser.L1SelContext ctx)
    {
        return VisitUnRecursive<UnaryOrAtomSelectorExpr, UnarySelectorOperation, sallParser.L1SelOpContext,
            AtomSelectorExpr>(
            VisitAtomSelector(ctx.selectorAtom()), ctx.l1SelOp(), [], OperatorMap.UnSelectorOperations,
            (op, c) => new UnarySelectorExpr(op, c));
    }

    public static AtomSelectorExpr VisitAtomSelector(sallParser.SelectorAtomContext selectorAtom)
    {
        return new AtomSelectorExpr(
            VisitAtom<sallParser.SelectorAtomContext, SelectorExprOrSelector, SelectorChain, Selector>(selectorAtom,
                c => VisitSelectorExpr(c.selectorExpr()),
                c => VisitSelector(c.selector())));
    }

    private static Selector VisitSelector(sallParser.SelectorContext ctx)
    {
        return ctx switch
        {
            _ when ctx.uiSelector() != null => new UiSelector(ctx.uiSelector().IDENT().GetText()),
            _ when ctx.markerSelector() != null => new MarkerSelector(ctx.markerSelector().IDENT().GetText()),
            _ when ctx.stateMapSelector() != null => new StateMapSelector(VisitStateMap(ctx.stateMapSelector())),
            _ when ctx.axesSelector() != null => ctx.axesSelector() switch
            {
                var c when c.childrenSelector() != null => new ChildrenSelector(
                    VisitSliceOption(c.childrenSelector().sliceSelector())),
                var c when c.leftSiblingsSelector() != null => new RightSiblingsSelector(
                    VisitSliceOption(c.leftSiblingsSelector().sliceSelector())),
                var c when c.rightSiblingsSelector() != null => new LeftSiblingsSelector(
                    VisitSliceOption(c.rightSiblingsSelector().sliceSelector())),
                var c when c.parentSelector() != null => new ParentSelector(),
                _ => throw new ArgumentOutOfRangeException(nameof(ctx), ctx, null),
            },
            _ when ctx.sliceSelector() != null => new SliceSelector(VisitSliceOption(ctx.sliceSelector())!),
            _ when ctx.reverseSelector() != null => new ReverseSelector(),
            _ when ctx.uniqueSelector() != null => new UniqueSelector(),
            _ => throw new ArgumentOutOfRangeException(nameof(ctx), ctx, null),
        };
    }

    private static Range? VisitSliceOption(sallParser.SliceSelectorContext? sliceSelector)
    {
        if (sliceSelector == null) return null;

        var range = sliceSelector.range();
        return range switch
        {
            _ when range.boundedRange() != null =>
                new BoundedRange(VisitExpr(range.boundedRange().expr(0)), VisitExpr(range.boundedRange().expr(1))),
            _ when range.rightUnboundedRange() != null =>
                new RightUnboundedRange(VisitExpr(range.rightUnboundedRange().expr())),
            _ when range.leftUnboundedRange() != null =>
                new LeftUnboundedRange(VisitExpr(range.leftUnboundedRange().expr())),
            _ when range.pointRange() != null =>
                new PointRange(VisitExpr(range.pointRange().expr())),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static State[] VisitStateMap(sallParser.StateMapSelectorContext? context)
    {
        return context?.state().Select(s =>
            new State(s.IDENT()?.GetText() ?? s.stateKvp().IDENT().GetText(),
                s.stateKvp() != null ? VisitExpr(s.stateKvp().expr()) : null)).ToArray() ?? [];
    }

    public static AnonymousClass VisitAnonymousClassDef(sallParser.AnonymousClassDefContext context)
    {
        return VisitAnonymousClassDef(VisitSelectorExpr(context.selectorExpr()), context.classContent());
    }

    public static NamedClass VisitNamedClassDef(sallParser.NamedClassDefContext context)
    {
        var (parents, properties, subClasses) = VisitClassContent(context.classContent());
        return new NamedClass(context.className().GetText(), parents, properties, subClasses);
    }

    public static AnonymousClass VisitAnonymousClassDef(SelectorChain selectorChain,
        sallParser.ClassContentContext classContent)
    {
        var classBody = classContent.classBodyOrTerminator().classBody();
        return new AnonymousClass(
            selectorChain,
            Parents(classContent.parentsList()?.parent() ?? []),
            Properties(classBody?.classBodyItem().Select(cbi => cbi.property()).Where(p => p != null) ?? []),
            SubClasses(
                classBody?.classBodyItem().Select(cbi => cbi.anonymousClassDef()).Where(scd => scd != null) ?? [])
        );

        Parent[] Parents(sallParser.ParentContext[] ctx)
        {
            return ctx.Select(p => new Parent(p.IDENT().GetText(), VisitArgs(p.args()))).ToArray();
        }

        Property[] Properties(IEnumerable<sallParser.PropertyContext> ctx)
        {
            return ctx.Select(p => new Property(p.IDENT().GetText(), VisitExpr(p.expr()))).ToArray();
        }

        AnonymousClass[] SubClasses(IEnumerable<sallParser.AnonymousClassDefContext> ctx)
        {
            return ctx.Select(VisitAnonymousClassDef).ToArray();
        }
    }

    public static (Parent[], Property[], AnonymousClass[]) VisitClassContent(
        sallParser.ClassContentContext classContent)
    {
        var classBody = classContent.classBodyOrTerminator().classBody();
        return (
            Parents(classContent.parentsList()?.parent() ?? []),
            Properties(classBody?.classBodyItem().Select(cbi => cbi.property()).Where(p => p != null) ?? []),
            SubClasses(
                classBody?.classBodyItem().Select(cbi => cbi.anonymousClassDef()).Where(scd => scd != null) ?? []
            )
        );

        Parent[] Parents(sallParser.ParentContext[] ctx)
        {
            return ctx.Select(p => new Parent(p.IDENT().GetText(), VisitArgs(p.args()))).ToArray();
        }

        Property[] Properties(IEnumerable<sallParser.PropertyContext> ctx)
        {
            return ctx.Select(p => new Property(p.IDENT().GetText(), VisitExpr(p.expr()))).ToArray();
        }

        AnonymousClass[] SubClasses(IEnumerable<sallParser.AnonymousClassDefContext> ctx)
        {
            return ctx.Select(VisitAnonymousClassDef).ToArray();
        }
    }

    public static Param[] VisitParams(sallParser.ParamsContext? context)
    {
        return context?.paramList()?.param().Select(p => new Param(p.IDENT().GetText(), VisitExpr(p.expr())))
            .ToArray() ?? [];
    }

    #region Helpers

    private static T VisitBinRecursive<T>(ParserRuleContext ctx, List<Func<ParserRuleContext, ParserRuleContext>> chain,
        Func<ParserRuleContext, T> lowest,
        Func<string, T, T, T> createNew, int level = 0)
    {
        if (level == chain.Count) return lowest(ctx);

        var getNext = chain[level];
        var left = VisitBinRecursive(getNext(ctx), chain, lowest, createNew, level + 1);

        for (var i = 1; i < ctx.ChildCount; i += 2)
        {
            var opNode = ctx.GetChild(i);
            var rightNode = ctx.GetChild(i + 1);
            var right = VisitBinRecursive((ParserRuleContext)rightNode, chain, lowest, createNew, level + 1);
            left = createNew(opNode.GetText(), left, right);
        }

        return left;
    }

    private static TOut VisitUnRecursive<TOut, TOp, TOpContext, TAtom>(TAtom atom, TOpContext[] ops,
        Func<TOpContext, bool>[] filters, IDictionary<string, TOp> operationsMap, Func<TOp, TOut, TOut> createNew)
        where TOpContext : IParseTree
        where TAtom : TOut
    {
        var enumerable = ops.Reverse();
        enumerable = filters.Aggregate(enumerable, (current, filter) => current.Where(filter));
        return enumerable.Aggregate<TOpContext, TOut>(atom, (current, opContext) =>
            createNew(operationsMap[opContext.GetText()], current));
    }

    private static TOut VisitAtom<T, TOut, TExpr, TValue>(T ctx, Func<T, TExpr> visitExpr, Func<T, TValue> visitValue)
        where T : IParseTree
        where TExpr : TOut
        where TValue : TOut
    {
        return ctx.ChildCount == 3
            ? visitExpr(ctx)
            : visitValue(ctx);
    }

    #endregion
}

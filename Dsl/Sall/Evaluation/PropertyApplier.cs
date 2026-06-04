using System;
using System.Collections.Generic;
using Sall.Ast;
using Sall.Lowering;

namespace Sall.Evaluation;

public class PropertyApplierRegistry
{
    public void Register<T>(Dictionary<string, >)
    {
    }
}

public readonly record struct PropertyReceiverContext(IPropertyReceiver PropertyReceiver, LayoutContext LayoutContext);

public static class PropertyApplier
{
    public static void Apply(IEnumerable<NormalizedClass> classes, PropertyReceiverContext ctx)
    {
        foreach (var normalizedClass in classes) Apply(normalizedClass, ctx);
    }
    
    public static void Apply(NormalizedClass normalizedClass, PropertyReceiverContext ctx)
    {
        var appliers = ctx.PropertyReceiver.PropertyAppliers;
        foreach (var kvp in normalizedClass.Properties)
        {
            if (!appliers.TryGetValue(kvp.Key, out var applier)) continue;
            var normalizedProperty = kvp.Value;
            var evalCtx = new EvaluationContext(normalizedProperty.Scope, ctx.LayoutContext);
            applier?.Invoke(Evaluator.Eval(normalizedProperty.Expr, evalCtx));
        }
    }
}

/// <summary>
/// Implemented by UI nodes that support applying style properties at runtime.
/// </summary>
/// <remarks>
/// <see cref="Value"/> is a dynamic type and must be cast to the expected target type.
/// Incorrect casting or applying a property to an incompatible node type may result in runtime exceptions.
/// </remarks>
public interface IPropertyReceiver
{
    /// <summary>
    /// Returns a map of property identifiers to their corresponding application functions.
    /// Each action applies a property value to the implementing UI node.
    /// </summary>
    Dictionary<StringId, Action<Value>> PropertyAppliers { get; }
}

// TODO (later): Consider generalizing
public readonly record struct StringId(int Id)
{
    private static readonly Dictionary<string, int> Map = new();
    private static int _counter;

    private static readonly object Lock = new();

    public static implicit operator StringId(string str) => FromString(str);

    public static StringId FromString(string str)
    {
        lock (Lock)
        {
            if (Map.TryGetValue(str, out var id))
                return new StringId(id);

            id = _counter++;
            Map[str] = id;
            return new StringId(id);
        }
    }
}

using System.Collections.Generic;

namespace Sall;

public static class OperatorMap
{
    public static readonly Dictionary<string, BinaryOperation> BinOperations = new()
    {
        { "||", BinaryOperation.Or },
        { "&&", BinaryOperation.And },
        { "<", BinaryOperation.Lt },
        { "<=", BinaryOperation.Le },
        { ">", BinaryOperation.Gt },
        { ">=", BinaryOperation.Ge },
        { "==", BinaryOperation.Eq },
        { "!=", BinaryOperation.Ne },
        { "+", BinaryOperation.Add },
        { "-", BinaryOperation.Subtract },
        { "*", BinaryOperation.Multiply },
        { "/", BinaryOperation.Divide },
        { "%", BinaryOperation.Remainder },
    };

    public static readonly Dictionary<string, UnaryOperation> UnOperations = new()
    {
        { "-", UnaryOperation.Negative },
        { "!", UnaryOperation.Not },
    };

    public static readonly Dictionary<string, BinarySelectorOperation> BinSelectorOperations = new()
    {
        { "||", BinarySelectorOperation.Or },
        { "&&", BinarySelectorOperation.And },
    };

    public static readonly Dictionary<string, UnarySelectorOperation> UnSelectorOperations = new()
    {
        { "!", UnarySelectorOperation.Not },
    };
}

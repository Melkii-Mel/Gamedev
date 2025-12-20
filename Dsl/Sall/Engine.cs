// using System;
//
// namespace Sall;
//
// public class Engine
// {
//     
// }
//
// public static class Evaluator
// {
//     public static Value Eval(Expr expr)
//     {
//         return expr switch
//         {
//             BinaryExpr binaryExpr => EvalBinary(binaryExpr),
//             AtomExpr atomExpr => throw new NotImplementedException(),
//             Call call => throw new NotImplementedException(),
//             UnaryExpr unaryExpr => throw new NotImplementedException(),
//             VariableRef variableRef => throw new NotImplementedException(),
//             _ => throw new ArgumentOutOfRangeException(nameof(expr), expr, null),
//         };
//     }
//
//     private static Value EvalBinary(BinaryExpr binaryExpr)
//     {
//         return binaryExpr.Operation switch
//         {
//             BinOperation.Or => Eval(binaryExpr.Left) || Eval(binaryExpr.Right),
//             BinOperation.And => expr,
//             BinOperation.Lt => expr,
//             BinOperation.Le => expr,
//             BinOperation.Gt => expr,
//             BinOperation.Ge => expr,
//             BinOperation.Eq => expr,
//             BinOperation.Ne => expr,
//             BinOperation.Add => expr,
//             BinOperation.Subtract => expr,
//             BinOperation.Multiply => expr,
//             BinOperation.Divide => expr,
//             BinOperation.Remainder => expr,
//             _ => throw new ArgumentOutOfRangeException()
//         };
//     }
// }
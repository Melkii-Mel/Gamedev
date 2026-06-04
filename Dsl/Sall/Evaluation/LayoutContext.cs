using Sall.Ast;
using Silk.NET.Maths;

namespace Sall.Evaluation;

public record LayoutContext(
    Vector2D<float> ParentSize,
    Vector2D<float> RootSize,
    float ParentFontSize,
    float RootFontSize,
    Expr SelfWidth,
    Expr SelfHeight
);

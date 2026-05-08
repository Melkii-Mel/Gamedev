namespace Sall.Api;

public class NodeConstraints
{
    public float Width, Height, MinWidth, MaxWidth, MinHeight, MaxHeight;
}

public class NodeIntrinsicSize
{
    public float IntrinsicMinWidth, IntrinsicMinHeight, IntrinsicPrefWidth, IntrinsicPrefHeight;
}

public class NodeLayoutResult
{
    public float ComputedWidth, ComputedHeight, X, Y;
}

public record NodeLayout(
    NodeConstraints Constraints,
    NodeIntrinsicSize IntrinsicSize,
    NodeLayoutResult LayoutResult
);

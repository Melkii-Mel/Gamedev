using Primitives.Shapes;
using Silk.NET.Maths;

namespace Primitives;

public record Collider2D(IShape2D Shape2D, Vector2D<float> Pivot);

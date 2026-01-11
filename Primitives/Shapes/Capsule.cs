using Silk.NET.Maths;

namespace Primitives.Shapes;

public record Capsule(float Radius, float Height) : IShape2D
{
    public Vector2D<float> Size => new Vector2D<float>(Radius * 2, Radius * 2 + Height);
}

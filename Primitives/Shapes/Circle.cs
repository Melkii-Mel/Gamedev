using Silk.NET.Maths;

namespace Primitives.Shapes;

public record Circle(float Radius) : IShape2D
{
    public Vector2D<float> Size => new(Radius);
}

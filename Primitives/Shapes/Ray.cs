using Silk.NET.Maths;

namespace Primitives.Shapes;

public record Ray(float Length) : IShape2D
{
    public Vector2D<float> Size => Vector2D<float>.Zero;
}

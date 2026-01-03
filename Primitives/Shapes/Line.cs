using Silk.NET.Maths;

namespace Primitives.Shapes;

public record Line(Vector2D<float> Normal, float Distance) : IShape2D;

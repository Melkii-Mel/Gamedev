using System;
using Silk.NET.Maths;

namespace Primitives.Shapes;

public record Polygon(Vector2D<float>[] Points) : IShape2D
{
    public bool IsConcave()
    {
        throw new NotImplementedException();
    }
}

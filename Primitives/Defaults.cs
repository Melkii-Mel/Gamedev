using System.Data;
using Silk.NET.Maths;

namespace Primitives;

public static class Defaults
{
    public const float Tolerance = 1e-6f;
    public const double DTolerance = 1e-9f;

    public static readonly Vector2D<float> CenterPivot = new(0.5f, 0.5f);
    public static readonly Vector2D<float> TopLeftPivot = Vector2D<float>.Zero;
    public static readonly Vector2D<float> DefaultPivot = CenterPivot;
}

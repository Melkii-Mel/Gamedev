using Silk.NET.Maths;

namespace Utils.Engine.MovementControls;

public class TopDownState(Vector2D<float>? position = null)
{
    public Vector2D<float> Position { get; set; } = position ?? default;
    public Vector2D<float> Velocity { get; set; }
    public Vector2D<float> CurrentDirection { get; set; }
}

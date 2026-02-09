using Silk.NET.Maths;
using Utils.Observables;

namespace Utils.Engine.MovementControls;

public record struct TopDownState
{
    public Vector2D<float> Position, Velocity;

    public TopDownState()
    {
    }

    public TopDownState(Vector2D<float> position = default, Vector2D<float> velocity = default)
    {
        Position = position;
        Velocity = velocity;
    }
};

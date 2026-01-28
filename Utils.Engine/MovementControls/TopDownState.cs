using Silk.NET.Maths;
using Utils.Observables;

namespace Utils.Engine.MovementControls;

public record struct TopDownState
{
    public Vector2D<float> Position, Velocity, CurrentDirection;

    public TopDownState()
    {
    }

    public TopDownState(Vector2D<float> position = default, Vector2D<float> velocity = default,
        Vector2D<float> currentDirection = default)
    {
        Position = position;
        Velocity = velocity;
        CurrentDirection = currentDirection;
    }
};

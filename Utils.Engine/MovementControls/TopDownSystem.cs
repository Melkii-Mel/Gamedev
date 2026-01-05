using System;
using Primitives;
using Silk.NET.Maths;
using static Gamedev.EngineInstance;

namespace Utils.Engine.MovementControls;

public class TopDownSystem
{
    public TopDownState State { get; set; }
    public event Action<Vector2D<float>>? PositionUpdated;

    public TopDownSystem(TopDownConfig config, TopDownState state, Action<float> update, Action<TopDownDirection[]>? discreteAction, Action<Vector2D<float>>? analogAction)
    {
        State = state;
        if (discreteAction == null && analogAction == null)
        {
            E.DebugOutput.Warning("Top-down movement controls initialized with neither discrete or analog input actions.");
        }
        update += delta => Update(delta, config);
        discreteAction += UpdateDirection;
        analogAction += UpdateDirection;
    }

    private void UpdateDirection(Vector2D<float> direction)
    {
        State.CurrentDirection = direction;
    }

    private void UpdateDirection(TopDownDirection[] directions)
    {
        var direction = Vector2D<float>.Zero;
        foreach (var d in directions)
        {
            direction += d switch
            {
                TopDownDirection.Up => -Vector2D<float>.UnitY,
                TopDownDirection.Down => Vector2D<float>.UnitY,
                TopDownDirection.Left => Vector2D<float>.UnitX,
                TopDownDirection.Right => -Vector2D<float>.UnitX,
                _ => throw new NotSupportedException(),
            };
        }
        State.CurrentDirection = Vector2D.Normalize(direction);
    }

    private void Update(float delta, TopDownConfig config)
    {
        if (State.CurrentDirection.LengthSquared < Defaults.Tolerance) return;
        var directionNormalized = Vector2D.Normalize(State.CurrentDirection);
        // Raise two times by half delta, raising position in-between, to increase predictability with different framerates
        RaiseVelocity();
        State.Position += State.Velocity * delta;
        RaiseVelocity();

        PositionUpdated?.Invoke(State.Position);

        return;

        void RaiseVelocity()
        {
            State.Velocity += directionNormalized * (config.Acceleration * delta / 2 * State.CurrentDirection);
            if (State.Velocity.LengthSquared > MathF.Pow(config.MaxVelocity, 2))
            {
                State.Velocity = Vector2D.Normalize(State.Velocity) * config.MaxVelocity;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Primitives;
using Silk.NET.Maths;
using static Gamedev.EngineInstance;

namespace Utils.Engine.MovementControls;

public class TopDownSystem(
    TopDownConfig config,
    TopDownState state,
    Action<Action<float>> updateBinder,
    Action<Action<IEnumerable<TopDownDirection>>>? discreteActionBinder,
    Action<Action<Vector2D<float>>>? analogActionBinder = null
)
{
    public void Init()
    {
        if (discreteActionBinder == null && analogActionBinder == null)
            E.DebugOutput.Warning(
                "Top-down movement controls initialized with neither discrete or analog input actions."
            );
        updateBinder(Update);
        discreteActionBinder?.Invoke(UpdateDirection);
        analogActionBinder?.Invoke(UpdateDirection);
    }

    public TopDownState State { get; set; } = state;
    // TODO: Raise an event when position is updated manually, or when TopDownState is replaced
    // TODO: Add velocity update event
    public event Action<Vector2D<float>>? PositionUpdated;

    private void UpdateDirection(Vector2D<float> direction)
    {
        State.CurrentDirection = direction;
    }

    private void UpdateDirection(IEnumerable<TopDownDirection> directions)
    {
        var direction = directions.Aggregate(Vector2D<float>.Zero, (current, d) => current + d switch
        {
            TopDownDirection.Up => -Vector2D<float>.UnitY,
            TopDownDirection.Down => Vector2D<float>.UnitY,
            TopDownDirection.Left => Vector2D<float>.UnitX,
            TopDownDirection.Right => -Vector2D<float>.UnitX,
            _ => throw new NotSupportedException(),
        });
        State.CurrentDirection = Vector2D.Normalize(direction);
    }

    private void Update(float delta)
    {
        if (State.CurrentDirection.LengthSquared < Defaults.Tolerance) return;
        var directionNormalized = Vector2D.Normalize(State.CurrentDirection);
        // Apply velocity in two half-steps with an intermediate position update
        // Improves consistency across different frame rates
        State.Position += State.Velocity * delta;
        RaiseVelocity();

        PositionUpdated?.Invoke(State.Position);

        return;

        void RaiseVelocity()
        {
            State.Velocity += directionNormalized * (config.Acceleration * delta / 2 * State.CurrentDirection);
            if (State.Velocity.LengthSquared > MathF.Pow(config.MaxVelocity, 2))
                State.Velocity = Vector2D.Normalize(State.Velocity) * config.MaxVelocity;
        }
    }
}

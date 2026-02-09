using System;
using Silk.NET.Maths;
using static Gamedev.EngineInstance;
using static Primitives.Defaults;

namespace Utils.Engine.MovementControls;

public class TopDownSystem(
    TopDownConfig config,
    TopDownState state,
    Action<Action<float>> updateBinder,
    TopDownDiscreteDirectionActionNames? directionActionNames,
    string? analogDirectionName = null,
    bool forceMaxAnalogVelocity = false
)
{
    private TopDownState _state = state;

    public void Init()
    {
        if (directionActionNames == null && analogDirectionName == null)
            E.DebugOutput.Warning(
                "Top-down movement controls initialized with neither discrete or analog input actions.");
        updateBinder(Update);
    }

    public TopDownState State
    {
        get => _state;
        set
        {
            _state = value;
            StateUpdated?.Invoke(_state);
        }
    }

    // TODO: Raise an event when position is updated manually, or when TopDownState is replaced
    // TODO: Add velocity update event
    public event Action<TopDownState>? StateUpdated;

    private void Update(float delta)
    {
        var direction = CurrentDirection();

        if (direction.LengthSquared > 1 || (forceMaxAnalogVelocity && direction.LengthSquared < Tolerance))
            direction = Vector2D.Normalize(direction);

        var targetVelocity = direction * config.MaxVelocity;
        var velocityDelta = targetVelocity - State.Velocity;

        var maxDelta =
            Vector2D.Dot(velocityDelta, targetVelocity) > 0
                ? config.Acceleration * delta
                : config.Deceleration * delta;

        var deltaLength = velocityDelta.Length;

        if (deltaLength > maxDelta)
            velocityDelta /= deltaLength / maxDelta;

        var velocity = State.Velocity + velocityDelta;
        var position = State.Position + velocity * delta;
        State = new TopDownState(position, velocity);
    }

    private Vector2D<float> CurrentDirection()
    {
        var direction = Vector2D<float>.Zero;
        if (directionActionNames != null)
        {
            if (IsDown(directionActionNames.Down)) direction += Vector2D<float>.UnitY;
            if (IsDown(directionActionNames.Left)) direction -= Vector2D<float>.UnitX;
            if (IsDown(directionActionNames.Up)) direction -= Vector2D<float>.UnitY;
            if (IsDown(directionActionNames.Right)) direction += Vector2D<float>.UnitX;
        }

        if (analogDirectionName != null)
        {
            direction += E.Input.GetActionValue<Vector2D<float>>(analogDirectionName);
        }
        
        return direction;

        bool IsDown(string directionActionName)
        {
            return E.Input.GetActionValue<bool>(directionActionName);
        }
    }
}

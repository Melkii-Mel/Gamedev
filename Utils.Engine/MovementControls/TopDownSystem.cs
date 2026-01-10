using System;
using Gamedev.InputSystem.ActionSystem;
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
    private Vector2D<float> _currentDiscreteDirection = Vector2D<float>.Zero;
    private TopDownState _state = state;

    public void Init()
    {
        if (directionActionNames == null && analogDirectionName == null)
            E.DebugOutput.Warning(
                "Top-down movement controls initialized with neither discrete or analog input actions.");
        if (directionActionNames != null) AddDiscreteInputListener(directionActionNames);
        if (analogDirectionName != null) AddAnalogInputListener(analogDirectionName);
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
        var direction = State.CurrentDirection;

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
        State = State with { Velocity = velocity, Position = position };
    }


    private Action<InputActionEventArgs<bool>>? _discreteInputAction;

    private void AddDiscreteInputListener(TopDownDiscreteDirectionActionNames actionNames)
    {
        _discreteInputAction = args =>
        {
            var actionName = args.Name;
            Vector2D<float>? direction = actionName switch
            {
                _ when actionName == actionNames.Up => -Vector2D<float>.UnitY,
                _ when actionName == actionNames.Down => Vector2D<float>.UnitY,
                _ when actionName == actionNames.Left => -Vector2D<float>.UnitX,
                _ when actionName == actionNames.Right => Vector2D<float>.UnitX,
                _ => null,
            };
            if (direction == null) return;
            _currentDiscreteDirection += args.Value ? direction.Value : -direction.Value;
            _state = State with { CurrentDirection = _currentDiscreteDirection };
        };
        E.Input.Dispatcher.AddListener(_discreteInputAction);
    }

    private Action<InputActionEventArgs<Vector2D<float>>>? _analogInputAction;

    private void AddAnalogInputListener(string analogActionName)
    {
        _analogInputAction = args =>
        {
            var actionName = args.Name;
            if (actionName != analogActionName) return;
            _state = State with { CurrentDirection = args.Value };
        };
        E.Input.Dispatcher.AddListener(_analogInputAction);
    }
}

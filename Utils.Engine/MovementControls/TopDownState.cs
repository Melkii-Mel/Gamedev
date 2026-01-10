using Silk.NET.Maths;
using Utils.Observables;

namespace Utils.Engine.MovementControls;

public readonly record struct TopDownState(
    Vector2D<float> Position = default,
    Vector2D<float> Velocity = default,
    Vector2D<float> CurrentDirection = default
);

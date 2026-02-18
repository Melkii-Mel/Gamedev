#nullable enable
using System;

namespace Games.Ring;

public struct Arc
{
    private float _startAngle;

    public float StartAngle
    {
        get => _startAngle;
        set
        {
            const float twoPi = MathF.PI * 2;
            value %= twoPi;
            if (value < 0) value += twoPi;
            _startAngle = value;
        }
    }

    public float Span { get; set; }

    public float EndAngle
    {
        get => _startAngle + Span;
        set => Span = value - _startAngle;
    }

    public Arc Expand(float value)
    {
        StartAngle += value / 2;
        Span += value / 2;
        return this;
    }

    public Arc Shrink(float value) => Expand(-value);
}

#nullable enable
using System;
using Gamedev.Entities;
using Silk.NET.Maths;

namespace Games.Ring;

public record Bullet(EntityComponent<INode2D> Node, float Angle, float Speed)
{
    public float Distance { get; private set; }

    public void Update(float delta)
    {
        var deltaDistance = Speed * delta;
        Node.Component.Transform.Position +=
            new Vector2D<float>(deltaDistance * MathF.Cos(Angle), deltaDistance * MathF.Sin(Angle));
        Distance += deltaDistance;
    }
}

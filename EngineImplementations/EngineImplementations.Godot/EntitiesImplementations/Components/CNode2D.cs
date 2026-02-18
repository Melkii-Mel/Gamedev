using Attributes;
using Gamedev.Entities;
using Godot;
using Silk.NET.Maths;

namespace EngineImplementations.GodotImplementation.EntitiesImplementations.Components;

[DelegateImplementation(typeof(INode), nameof(_node))]
public partial class CNode2D(Node2D node2d) : INode2D
{
    private CNode _node = new(node2d);
    
    public int ZIndex
    {
        get => node2d.ZIndex;
        set => node2d.ZIndex = value;
    }

    public ITransform2D Transform { get; } = new Transform(node2d);
}

public readonly struct Transform(Node2D node2D) : ITransform2D
{
    public Vector2D<float> Position
    {
        get => node2D.Position.ToSilk();
        set => node2D.Position = value.ToGd();
    }

    public float Rotation
    {
        get => node2D.Rotation;
        set => node2D.Rotation = value;
    }

    public Vector2D<float> Scale
    {
        get => node2D.Scale.ToSilk();
        set => node2D.Scale = value.ToGd();
    }
}

// TODO: Move somewhere else

public static class Vector2Extensions
{
    public static Vector2D<float> ToSilk(this Vector2 v)
    {
        return new Vector2D<float>(v.x, v.y);
    }

    public static Vector2 ToGd(this Vector2D<float> v)
    {
        return new Vector2(v.X, v.Y);
    }
}

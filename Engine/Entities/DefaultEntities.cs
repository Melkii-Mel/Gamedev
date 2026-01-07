using System;
using System.Collections.Generic;
using Gamedev.Localization;
using Primitives.Shapes;
using Silk.NET.Maths;

namespace Gamedev.Entities;

public interface IEntity
{
    event Action<IEntity>? ChildAdded;
    event Action<IEntity>? ChildRemoved;
    void AddChild(IEntity entity);
    void RemoveChild(IEntity entity);
    void Free();
    void FreeRn();
    IEntity RemoveChildAt(int index);
    IEnumerable<IEntity> GetChildren();
}

public static class EntityExtensions
{
    public static void AddChild<T>(this IEntity entity, EntityComponent<T> entityComponent)
    {
        entity.AddChild(entityComponent.Entity);
    }

    public static void RemoveChild<T>(this IEntity entity, EntityComponent<T> entityComponent)
    {
        entity.RemoveChild(entityComponent.Entity);
    }
}

public readonly record struct EntityComponent<T>(IEntity Entity, T Component)
{
    public void AddChild(IEntity entity)
    {
        Entity.AddChild(entity);
    }

    public void AddChild<TOther>(EntityComponent<TOther> entityComponent)
    {
        Entity.AddChild(entityComponent);
    }

    public void RemoveChild(IEntity entity)
    {
        Entity.RemoveChild(entity);
    }

    public void RemoveChild<TOther>(EntityComponent<TOther> entityComponent)
    {
        Entity.RemoveChild(entityComponent);
    }
}

public interface IButton
{
    IControl Control { get; }
    Text? Text { get; set; }
    event Action OnClick;
}

public interface IControl
{
    Rectangle<float> Bounds { get; set; }
    Rectangle<float> Anchors { get; set; }
    Vector2D<float> Pivot { get; set; }
    Rectangle<float> Margins { get; set; }
    bool Visible { get; set; }
}

public interface INode2D
{
    int ZIndex { get; set; }
    ITransform2D Transform { get; }
}

public interface ITransform2D
{
    Vector2D<float> Position { get; set; }
    float Rotation { get; set; }
    Vector2D<float> Scale { get; set; }
}

public interface ISprite2D
{
    INode2D Node { get; }

    /// <summary>
    /// The pivot of the sprite relative to its size:
    /// (0,0) = top-left, (1,1) = bottom-right.
    /// Defaults to (0.5, 0.5) to center the sprite.
    /// 
    /// Engine implementations must respect this default when providing the sprite.
    /// </summary>
    Vector2D<float> Pivot { get; set; }
}

public interface ITrigger2D
{
}

public interface INode3D
{
}

public interface INode;

using System;
using System.Collections.Generic;
using Gamedev.Localization;
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
}

public interface INode2D
{
}

public interface INode3D
{
}

public interface INode;

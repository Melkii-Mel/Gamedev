using System;
using System.Collections.Generic;
using Attributes;
using Gamedev.Localization;
using Gamedev.Resources;
using Primitives;
using Silk.NET.Maths;

namespace Gamedev.Entities;

[DelegateImplementation(typeof(IEntity), nameof(Internal), DelegationStyle.Implicit, ["Parent"])]
public partial class Entity : IEntityHolder
{
    Entity IEntityHolder.Entity => this;
    public IEntity Internal { get; }

    public Entity(IEntity entity)
    {
        Internal = entity;
    }

    public event Action<Entity>? ChildAdded;
    public event Action<Entity>? ChildRemoved;
}

public interface IEntity
{
    void AddChild(IEntity entity);
    void RemoveChild(IEntity entity);
    IEntity? Parent { get; }
    void Free();
    void FreeRn();
    IEntity RemoveChildAt(int index);
    IEnumerable<IEntity> GetChildren();
}

public interface IEntityHolder
{
    public Entity Entity { get; }
}

public static class EntityExtensions
{
    public static void AddChild(this IEntityHolder entity, IEntityHolder child)
    {
        entity.Entity.AddChild(child.Entity);
    }

    public static void RemoveChild<T>(this IEntityHolder entity, IEntityHolder child)
    {
        entity.Entity.RemoveChild(child.Entity.Internal);
    }

    public static void Reparent(this IEntityHolder entity, IEntityHolder newParent)
    {
        entity.Entity.Parent?.RemoveChild(entity.Entity);
        newParent.Entity.AddChild(entity.Entity);
    }

    public static void AddChildAddedListener(this IEntityHolder entity, Action<Entity> callback)
    {
        entity.Entity.ChildAdded += callback;
    }

    public static void RemoveChildAddedListener(this IEntityHolder entity, Action<Entity> callback)
    {
        entity.Entity.ChildRemoved += callback;
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
    event Action? OnClick;
}

public interface IPanel
{
    IControl Control { get; }
    Color BackgroundColor { get; set; }
    Color BorderColor { get; set; }
    float BorderThickness { get; set; }
    float CornerRadius { get; set; }
}

public interface ITextField
{
    IControl Control { get; }
    Text? Text { get; set; }
    float FontSize { get; set; }
    string FontFamily { get; set; }
    Color Color { get; set; }
    HAlignment HAlignment { get; set; }
    VAlignment VAlignment { get; set; }
    bool AutoWrap { get; set; }
}

public enum HAlignment
{
    Left,
    Center,
    Right,
    Stretch,
}

public enum VAlignment
{
    Top,
    Center,
    Bottom,
    Stretch,
}

public interface IImage
{
    IControl Control { get; }
    ITexture? Texture { get; set; }
}

public interface IControl
{
    Rectangle<float> Bounds { get; set; }
    Rectangle<float> Anchors { get; set; }
    Vector2D<float> Pivot { get; set; }
    Rectangle<float> Margins { get; set; }
    bool Visible { get; set; }

    // TODO: Add a property that maps a set of common events like mouse clicks, movement etc.
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
    ///     The pivot of the sprite relative to its size:
    ///     (0,0) = top-left, (1,1) = bottom-right.
    ///     Defaults to (0.5, 0.5) to center the sprite.
    ///     Engine implementations must respect this default when providing the sprite.
    /// </summary>
    Vector2D<float> Pivot { get; set; }

    ITexture? Texture { get; set; }
}

public interface ITrigger2D : ITrigger<INode2D, ITrigger2D>
{
    Collider2D? Collider { get; set; }
}

public interface ITrigger<out TNode, out TParent>
{
    TNode Node { get; }
    Flags Mask { get; set; }
    Flags Layer { get; set; }

    event Action<TParent>? OnEnter;
    event Action<TParent, float>? OnStay;
    event Action<TParent>? OnExit;
}

public interface INode3D;

public interface INode;

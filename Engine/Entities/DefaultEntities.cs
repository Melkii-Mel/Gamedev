using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Attributes;
using Gamedev.Localization;
using Gamedev.Resources;
using Primitives;
using Silk.NET.Maths;
using Utils.Collections;

namespace Gamedev.Entities;

[DelegateImplementation(typeof(IEntity), nameof(Internal))]
public partial class Entity : IEntity, IEntityHolder
{
    Entity IEntityHolder.Entity => this;
    private readonly IList<Entity> _children;
    public ReadOnlyCollection<Entity> Children { get; } 

    public Entity? Parent { get; private set; }
    private IEntity Internal { get; }

    public Entity(IEntity entity, int size = 0)
    {
        _children = new SwapAndPopList<Entity>(size);
        Internal = entity;
        Children = new ReadOnlyCollection<Entity>(_children);
    }

    public event Action<Entity>? ChildAdded;
    public event Action<Entity>? ChildRemoved;

    public void AddChild(Entity entity)
    {
        if (entity.Parent != null)
        {
            throw new InvalidOperationException("Entity already has a parent");
        }
        Internal.AddChild(entity);
        _children.Add(entity);
        entity.Parent = this;
        ChildAdded?.Invoke(entity);
    }

    public void RemoveChild(Entity entity)
    {
        Internal.RemoveChild(entity);
        _children.Remove(entity);
        entity.Parent = null;
        ChildRemoved?.Invoke(entity);
    }
}

public interface IEntity : IEntityHolder
{
    void AddChild(IEntity entity);
    void RemoveChild(IEntity entity);
    void Free();
    void FreeRn();
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

    public static void RemoveChild(this IEntityHolder entity, IEntityHolder child)
    {
        entity.Entity.RemoveChild(child.Entity);
    }

    public static ReadOnlyCollection<Entity> GetChildren(this IEntityHolder entity)
    {
        return entity.Entity.Children;
    }

    public static void DetachFromParent(this IEntityHolder entity)
    {
        entity.Entity.Parent?.RemoveChild(entity.Entity);
    }

    public static void SetParent(this IEntityHolder entity, IEntityHolder? newParent)
    {
        DetachFromParent(entity);
        newParent?.Entity.AddChild(entity.Entity);
    }

    public static bool HasParent(this IEntityHolder entity)
    {
        return GetParent(entity) != null;
    }

    public static IEntityHolder? GetParent(this IEntityHolder entity)
    {
        return entity.Entity.Parent; 
    }

    public static void AddChildAddedListener(this IEntityHolder entity, Action<Entity> callback)
    {
        entity.Entity.ChildAdded += callback;
    }

    public static void RemoveChildAddedListener(this IEntityHolder entity, Action<Entity> callback)
    {
        entity.Entity.ChildAdded -= callback;
    }

    public static void AddChildRemovedListener(this IEntityHolder entity, Action<Entity> callback)
    {
        entity.Entity.ChildRemoved += callback;
    }

    public static void RemoveChildRemovedListener(this IEntityHolder entity, Action<Entity> callback)
    {
        entity.Entity.ChildRemoved -= callback;
    }
}

public readonly record struct EntityComponent<T>(Entity Entity, T Component) : IEntityHolder;

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
    Color Modulation { get; set; }

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

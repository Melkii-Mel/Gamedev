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

public record struct EntityComponent<T>(IEntity Entity, T Component);

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

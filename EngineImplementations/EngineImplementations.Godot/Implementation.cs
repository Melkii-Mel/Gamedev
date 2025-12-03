using System;
using System.Collections.Generic;
using EngineImplementations.Godot.Components;
using EngineImplementations.Godot.EntitiesImplementations;
using Gamedev;
using Gamedev.Debugging;
using Gamedev.Entities;
using Godot;

namespace EngineImplementations.Godot;

public class Implementation : IEngine
{
    public Implementation(ref Action<double>? update, ref Action<double>? physicsUpdate, Node root)
    {
        Update = update;
        PhysicsUpdate = physicsUpdate;
        Root = new EntityComponent<INode>(new Entity(root), new CNode());
    }

    public event Action<double>? Update;
    public event Action<double>? PhysicsUpdate;

    public void Spawn(IEntity entity, IEntity parent)
    {
        var entityNode = Utils.As<Entity>(entity);
        var parentNode = Utils.As<Entity>(entity);
        parentNode.AddChild(entityNode);
    }

    public IEntities Entities { get; } = new Entities();
    public EntityComponent<INode> Root { get; }
    public IDebugOutput DebugOutput { get; } = new DebugOutput();
}

public class DebugOutput : IDebugOutput
{
    public void Out(DebugMessage message)
    {
        switch (message.Type)
        {
            case MessageType.Info:
                GD.Print(message.Message);
                break;
            case MessageType.Warning:
                GD.PushWarning(message.Message);
                break;
            case MessageType.Error:
                GD.PushError(message.Message);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class Entities : IEntities
{
    public EntityComponent<INode> Node()
    {
        var node = new Node();
        return new EntityComponent<INode>(new Entity(node), new CNode());
    }

    public IEntities.IUi Ui { get; } = new Ui();
    public IEntities.I2D E2D { get; } = new E2D();
    public IEntities.I3D E3D { get; } = new E3D();
}

public class Entity(Node node) : IEntity
{
    public event Action<IEntity>? ChildAdded;
    public event Action<IEntity>? ChildRemoved;
    private readonly Node _node = node;

    public void AddChild(IEntity entity)
    {
        _node.AddChild(ToNode(entity));
        ChildAdded?.Invoke(entity);
    }

    public void RemoveChild(IEntity entity)
    {
        _node.RemoveChild(ToNode(entity));
        ChildRemoved?.Invoke(entity);
    }

    public IEntity RemoveChildAt(int index)
    {
        var child = new Entity(_node.GetChild(index));
        RemoveChild(child);
        return child;
    }

    public IEnumerable<IEntity> GetChildren()
    {
        foreach (var child in _node.GetChildren())
        {
            if (child is Node node1)
            {
                yield return new Entity(node1);
            }
            else
            {
                GD.PushWarning("One of node's children is not a Node.");
            }
        }
    }

    public void Free()
    {
        _node.QueueFree();
    }

    public void FreeRn()
    {
        _node.Free();
    }


    private static Node ToNode(IEntity entity)
    {
        return Utils.As<Entity>(entity)._node;
    }
}
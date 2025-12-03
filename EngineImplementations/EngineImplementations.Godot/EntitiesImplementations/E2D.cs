using EngineImplementations.Godot.Components;
using Gamedev;
using Gamedev.Entities;
using Godot;

namespace EngineImplementations.Godot.EntitiesImplementations;

public class E2D : IEntities.I2D
{
    public EntityComponent<INode2D> Node()
    {
        var node2d = new Node2D();
        return new EntityComponent<INode2D>(new Entity(node2d), new CNode2D(node2d));
    }
}
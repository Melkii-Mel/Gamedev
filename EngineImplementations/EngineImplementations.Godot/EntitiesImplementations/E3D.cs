using EngineImplementations.GodotImplementation.EntitiesImplementations.Components;
using Gamedev;
using Gamedev.Entities;
using Godot;

namespace EngineImplementations.GodotImplementation.EntitiesImplementations;

public class E3D : IEntities.I3D
{
    public EntityComponent<INode3D> Node()
    {
        var node3d = new Spatial();
        return new EntityComponent<INode3D>(new GdEntity(node3d), new CNode3D(node3d));
    }
}

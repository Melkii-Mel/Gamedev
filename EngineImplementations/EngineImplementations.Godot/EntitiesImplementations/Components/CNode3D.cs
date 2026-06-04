using Attributes;
using Gamedev.Entities;
using Godot;

namespace EngineImplementations.GodotImplementation.EntitiesImplementations.Components;

[DelegateImplementation(typeof(CNode), nameof(_node))]
public partial class CNode3D(Spatial node3d) : INode3D
{
    private CNode _node = new(node3d);
}

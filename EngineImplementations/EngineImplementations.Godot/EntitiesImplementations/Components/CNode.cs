using Attributes;
using Gamedev.Entities;
using Godot;

namespace EngineImplementations.GodotImplementation.EntitiesImplementations.Components;

[DelegateImplementation(typeof(IEntity), nameof(_entity))]
public partial class CNode : INode
{
    private GdEntity _entity;

    private bool _visible = true;

    public bool Visible
    {
        get => _visible;
        set
        {
            if (_canvasItem is null) return;
            var modulation = _canvasItem.SelfModulate;
            modulation.a = value ? 1 : 0;
            _canvasItem.SelfModulate = modulation;
            _visible = value;
        }
    }

    private Primitives.Color _modulation;

    public Primitives.Color Modulation
    {
        get => _modulation;
        set
        {
            _modulation = value;
            if (_canvasItem != null) _canvasItem.SelfModulate = value.ToGd();
        }
    }

    private readonly CanvasItem? _canvasItem;

    public CNode(Node node)
    {
        _entity = new GdEntity(node);
        _canvasItem = node as CanvasItem;
        _modulation = _canvasItem?.SelfModulate.ToPrimitives() ?? new Primitives.Color(1, 1, 1, 1);
    }
}

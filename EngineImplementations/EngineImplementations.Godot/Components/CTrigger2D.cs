using System;
using System.Linq;
using Gamedev.Entities;
using Godot;
using Primitives;
using Primitives.Shapes;
using Silk.NET.Maths;

namespace EngineImplementations.GodotImplementation.Components;

public class CTrigger2D : ITrigger2D
{
    private readonly Trigger2D _trigger2d;
    private Collider2D? _collider;
    private CollisionShape2D? _collisionShape;
    private Flags _layer;
    private Flags _mask;

    public CTrigger2D(Trigger2D trigger2D)
    {
        _trigger2d = trigger2D;
        trigger2D.OnEnter += area => OnEnter?.Invoke(area);
        trigger2D.OnStay += (area, delta) => OnStay?.Invoke(area, delta);
        trigger2D.OnExit += area => OnExit?.Invoke(area);
        Node = new CNode2D(trigger2D);
        _mask = new Flags(trigger2D.CollisionMask);
        _layer = new Flags(trigger2D.CollisionLayer);
        trigger2D.CTrigger2D = this;
    }

    public Collider2D? Collider
    {
        get => _collider;
        set
        {
            _collider = value;
            _collisionShape?.QueueFree();
            if (_collider == null) return;
            _collisionShape = new CollisionShape2D
            {
                Shape = _collider.Shape2D switch
                {
                    Box box => new RectangleShape2D
                    {
                        Extents = box.Size.ToGd() / 2,
                    },
                    Capsule capsule => new CapsuleShape2D
                    {
                        Height = capsule.Height,
                        Radius = capsule.Radius,
                    },
                    Circle circle => new CircleShape2D
                    {
                        Radius = circle.Radius,
                    },
                    Line line => new LineShape2D
                    {
                        Normal = line.Normal.ToGd(),
                        D = line.Distance,
                    },
                    Ray ray => new RayShape2D
                    {
                        Length = ray.Length,
                    },
                    Polygon polygon => polygon.IsConcave() switch
                    {
                        true => new ConcavePolygonShape2D
                        {
                            Segments = [.. polygon.Points.Select(p => p.ToGd())],
                        },
                        false => new ConvexPolygonShape2D
                        {
                            Points = [.. polygon.Points.Select(p => p.ToGd())],
                        },
                    },
                    _ => throw new NotImplementedException()
                },
            };
            _collisionShape.Position = ((Vector2D<float>.One / 2 - _collider.Pivot) * _collider.Shape2D.Size).ToGd();
            _trigger2d.AddChild(_collisionShape);
        }
    }

    public INode2D Node { get; }

    public Flags Mask
    {
        get => _mask;
        set
        {
            _mask.FlagChanged -= OnMaskFlagChanged;
            _mask = value;
            _trigger2d.CollisionMask = _mask.Raw;
            _mask.FlagChanged += OnMaskFlagChanged;
            
            return;

            void OnMaskFlagChanged(FlagChangedEventArgs arg)
            {
                _trigger2d.CollisionMask = _mask.Raw;
            }
        }
    }

    public Flags Layer
    {
        get => _layer;
        set
        {
            _layer.FlagChanged -= OnLayerFlagChanged;
            _layer = value;
            _trigger2d.CollisionLayer = _layer.Raw;
            _layer.FlagChanged += OnLayerFlagChanged;
            
            return;

            void OnLayerFlagChanged(FlagChangedEventArgs arg)
            {
                _trigger2d.CollisionLayer = _layer.Raw;
            }
        }
    }


    public event Action<ITrigger2D>? OnEnter;
    public event Action<ITrigger2D, float>? OnStay;
    public event Action<ITrigger2D>? OnExit;
}

public class Trigger2D : Area2D
{
    public CTrigger2D? CTrigger2D { get; set; }


    public override void _Ready()
    {
        Connect("area_entered", this, nameof(OnAreaEntered));
        Connect("area_exited", this, nameof(OnAreaExited));
    }

    public override void _Process(float delta)
    {
        foreach (var area in GetOverlappingAreas())
            if (area is Trigger2D { CTrigger2D: not null } trigger2d)
                OnStay?.Invoke(trigger2d.CTrigger2D, delta);
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is Trigger2D { CTrigger2D: not null } trigger2D) OnEnter?.Invoke(trigger2D.CTrigger2D);
    }

    private void OnAreaExited(Area2D area)
    {
        if (area is Trigger2D { CTrigger2D: not null } trigger2D) OnExit?.Invoke(trigger2D.CTrigger2D);
    }

    public event Action<ITrigger2D>? OnEnter;
    public event Action<ITrigger2D, float>? OnStay;
    public event Action<ITrigger2D>? OnExit;
}

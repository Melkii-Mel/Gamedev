using EngineImplementations.GodotImplementation.Components;
using EngineImplementations.GodotImplementation.Resources;
using Gamedev;
using Gamedev.Entities;
using Gamedev.Resources;
using Godot;
using Primitives;
using Silk.NET.Maths;

namespace EngineImplementations.GodotImplementation.EntitiesImplementations;

public class E2D : IEntities.I2D
{
    public EntityComponent<INode2D> Node()
    {
        var node2d = new Node2D();
        return new EntityComponent<INode2D>(new Entity(node2d), new CNode2D(node2d));
    }

    public EntityComponent<ISprite2D> Sprite(ITexture? texture = null)
    {
        var sprite2d = new Sprite();
        return new EntityComponent<ISprite2D>(new Entity(sprite2d), new CSprite2D(sprite2d, texture));
    }

    public EntityComponent<ITrigger2D> Trigger(Collider2D? collider2D = null)
    {
        var trigger2D = new Trigger2D();
        var cTrigger2d = new CTrigger2D(trigger2D);
        trigger2D.CTrigger2D = cTrigger2d;
        if (collider2D != null)
        {
            cTrigger2d.Collider = collider2D;
        }
        return new EntityComponent<ITrigger2D>(new Entity(trigger2D), cTrigger2d);
    }
}

internal struct CSprite2D : ISprite2D
{
    private readonly Sprite _sprite2d;
    private ITexture? _texture;

    public CSprite2D(Sprite sprite2d, ITexture? texture)
    {
        _sprite2d = sprite2d;
        _texture = texture;
    }

    public INode2D Node => new CNode2D(_sprite2d);

    public Vector2D<float> Pivot 
    {
        get => _sprite2d.Offset.ToSilk();
        set => _sprite2d.Offset = value.ToGd(); 
    }
    public ITexture? Texture 
    { 
        get => _texture;
        set
        {
            if (value is RTexture rTexture)
            {
                _texture = rTexture;
                _sprite2d.Texture = rTexture.Inner;
            }
        }
    }
}

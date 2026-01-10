using EngineImplementations.GodotImplementation.Components;
using Gamedev;
using Gamedev.Entities;
using Gamedev.Resources;
using Godot;
using Primitives;

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
        if (collider2D != null) cTrigger2d.Collider = collider2D;
        return new EntityComponent<ITrigger2D>(new Entity(trigger2D), cTrigger2d);
    }
}

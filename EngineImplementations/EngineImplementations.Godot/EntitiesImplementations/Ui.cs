using EngineImplementations.GodotImplementation.Components;
using Gamedev;
using Gamedev.Entities;
using Godot;

namespace EngineImplementations.GodotImplementation.EntitiesImplementations;

public class Ui : IEntities.IUi
{
    public EntityComponent<IControl> Control()
    {
        var control = new Control();
        return new EntityComponent<IControl>(new Entity(control), new CControl(control));
    }

    public EntityComponent<IButton> Button()
    {
        var button = new Button();
        return new EntityComponent<IButton>(new Entity(button), new CButton(button));
    }
}

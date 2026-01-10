using EngineImplementations.GodotImplementation.Components;
using Gamedev;
using Gamedev.Entities;
using Gamedev.Resources;
using Godot;
using Color = Primitives.Color;

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

    public EntityComponent<IPanel> Panel()
    {
        var panel = new Panel();
        return new EntityComponent<IPanel>(new Entity(panel), new CPanel(panel));
    }

    public EntityComponent<ITextField> TextField()
    {
        var textField = new Label();
        return new EntityComponent<ITextField>(new Entity(textField), new CTextField(textField));
    }

    public EntityComponent<IImage> Image(ITexture? texture = null)
    {
        var textureRect = new TextureRect();
        return new EntityComponent<IImage>(new Entity(textureRect), new CImage(textureRect, texture));
    }
}

// TODO: Move out
public static class ColorExtensions
{
    public static Color ToPrimitives(this Godot.Color color)
    {
        return new Color(color.r, color.g, color.b, color.a);
    }

    public static Godot.Color ToGd(this Color color)
    {
        return new Godot.Color(color.R, color.G, color.B, color.A);
    }
}

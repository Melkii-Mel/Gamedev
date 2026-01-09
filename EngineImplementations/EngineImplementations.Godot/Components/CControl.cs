using Gamedev.Entities;
using Godot;
using Silk.NET.Maths;

namespace EngineImplementations.GodotImplementation.Components;

public readonly struct CControl(Control control) : IControl
{
    public Rectangle<float> Bounds
    {
        get => Utils.FromRect2(control.GetRect());
        set
        {
            var rect2 = Utils.ToRect2(value);
            control.RectPosition = rect2.Position;
            control.RectSize = rect2.Size;
        }
    }

    public Rectangle<float> Anchors
    {
        get => Utils.ToRectangle(control.AnchorLeft, control.AnchorTop, control.AnchorRight - control.AnchorLeft,
            control.AnchorBottom - control.AnchorTop);
        set
        {
            control.AnchorLeft = value.Origin.X;
            control.AnchorRight = value.Origin.X + value.Size.X;
            control.AnchorTop = value.Origin.Y;
            control.AnchorBottom = value.Origin.Y + value.Size.Y;
        }
    }

    public Vector2D<float> Pivot
    {
        get => Utils.FromVector2(control.RectPivotOffset);
        set => control.RectPivotOffset = Utils.ToVector2(value);
    }

    public Rectangle<float> Margins
    {
        get => Utils.ToRectangle(control.MarginLeft, control.MarginTop, control.MarginRight, control.MarginBottom);
        set
        {
            control.MarginLeft = value.Origin.X;
            control.MarginRight = value.Origin.X + value.Size.X;
            control.MarginTop = value.Origin.Y;
            control.MarginBottom = value.Origin.Y + value.Size.Y;
        }
    }

    public bool Visible
    {
        get { return control.Visible; }
        set { control.Visible = value; }
    }
}

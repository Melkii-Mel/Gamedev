using EngineImplementations.GodotImplementation.EntitiesImplementations;
using Gamedev.Entities;
using Godot;
using Color = Primitives.Color;

namespace EngineImplementations.GodotImplementation.Components;

internal class CPanel : IPanel
{
    private readonly StyleBoxFlat _styleBox;

    public CPanel(Panel panel)
    {
        Control = new CControl(panel);
        _styleBox = new StyleBoxFlat();
        panel.AddStyleboxOverride("panel", _styleBox);
    }

    public IControl Control { get; }

    public Color BackgroundColor
    {
        get => _styleBox.BgColor.ToPrimitives();
        set => _styleBox.BgColor = value.ToGd();
    }

    public Color BorderColor
    {
        get => _styleBox.BorderColor.ToPrimitives();
        set => _styleBox.BorderColor = value.ToGd();
    }

    public float BorderThickness
    {
        get => _styleBox.BorderWidthTop;
        set
        {
            var v = (int)value;
            _styleBox.BorderWidthBottom = v;
            _styleBox.BorderWidthLeft = v;
            _styleBox.BorderWidthRight = v;
            _styleBox.BorderWidthTop = v;
        }
    }

    public float CornerRadius
    {
        get => _styleBox.CornerRadiusTopRight;
        set
        {
            var v = (int)value;
            _styleBox.CornerRadiusTopRight = v;
            _styleBox.CornerRadiusTopLeft = v;
            _styleBox.CornerRadiusBottomRight = v;
            _styleBox.CornerRadiusBottomLeft = v;
        }
    }
}

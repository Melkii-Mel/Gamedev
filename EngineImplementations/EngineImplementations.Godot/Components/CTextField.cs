using EngineImplementations.GodotImplementation.Components;
using EngineImplementations.GodotImplementation.EntitiesImplementations;
using Gamedev;
using Gamedev.Entities;
using Gamedev.Localization;
using Gamedev.Resources;
using Godot;

namespace EngineImplementations.GodotImplementation.Components;

internal class CTextField : ITextField
{
    private readonly Label _textField;
    private readonly DynamicFont _font;
    private FontData _currentFontData;
    public CTextField(Label textField)
    {
        var registry = EngineInstance.E.Resources.FontRegistry;
        _currentFontData = registry.DefaultFont;
        var defaultFontPath = _currentFontData.Path;
        _textField = textField;
        _font = new()
        {
            FontData = new DynamicFontData
            {
                FontPath = defaultFontPath
            }
        };
        _textField.AddFontOverride("font", _font);
        Control = new CControl(textField);
    }

    public IControl Control { get; }

    private Text? _text;
    public Text? Text 
    { 
        get
        {
            return _text;
        }
        set
        {
            if (_text != null)
            {
                _text.OnUpdate -= Update;
            }
            _text = value;
            if (_text != null)
            {
                _text.OnUpdate += Update;
            }
            _textField.Text = _text != null ? _text : "";
            
            void Update(string newValue)
            {
                _textField.Text = newValue;
            }
        }
    }
    public float FontSize 
    { 
        get => _font.Size;
        set => _font.Size = (int)value;
    }
    public string FontFamily 
    { 
        get => _currentFontData.Name;
        set
        {
            _currentFontData = EngineInstance.E.Resources.FontRegistry.ByName(value) ?? _currentFontData;
            _font.FontData.FontPath = _currentFontData.Path;
        }
    }
    public Primitives.Color Color 
    { 
        get => _textField.GetColor("font_color").ToPrimitives(); 
        set => _textField.AddColorOverride("font_color", value.ToGd()); 
    }
    public HAlignment HAlignment
    { 
        get => _textField.Align switch
        {
            Label.AlignEnum.Left => HAlignment.Left,
            Label.AlignEnum.Center => HAlignment.Center,
            Label.AlignEnum.Right => HAlignment.Right,
            Label.AlignEnum.Fill => HAlignment.Stretch,
            _ => throw new System.NotImplementedException(),
        };
        set => _textField.Align = value switch
        {
            HAlignment.Left => Label.AlignEnum.Left,
            HAlignment.Center => Label.AlignEnum.Center,
            HAlignment.Right => Label.AlignEnum.Right,
            HAlignment.Stretch => Label.AlignEnum.Fill,
            _ => throw new System.NotImplementedException(),
        };
    }
    public VAlignment VAlignment
    {
        get => _textField.Valign switch
        {
            Label.VAlign.Top => VAlignment.Top,
            Label.VAlign.Center => VAlignment.Center,
            Label.VAlign.Bottom => VAlignment.Bottom,
            Label.VAlign.Fill => VAlignment.Stretch,
            _ => throw new System.NotImplementedException(),
        };
        set => _textField.Valign = value switch
        {
            VAlignment.Top => Label.VAlign.Top,
            VAlignment.Center => Label.VAlign.Center,
            VAlignment.Bottom => Label.VAlign.Bottom,
            VAlignment.Stretch => Label.VAlign.Fill,
            _ => throw new System.NotImplementedException(),
        };
    }
}

using System;
using Gamedev.Entities;
using Gamedev.Localization;
using Godot;

namespace EngineImplementations.Godot.Components;

public class CButton(Button button) : IButton
{
    private Text? _text;
    public IControl Control => new CControl(button);

    public Text? Text
    {
        get => _text;
        set
        {
            _text = value;
            if (_text == null)
            {
                button.Text = "";
                return;
            }

            _text.Bind(s => button.Text = s);
        }
    }

    public event Action? OnClick;
}

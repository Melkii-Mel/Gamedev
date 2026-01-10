using EngineImplementations.GodotImplementation.Resources;
using Gamedev.Entities;
using Gamedev.Resources;
using Godot;

namespace EngineImplementations.GodotImplementation.Components;

internal class CImage : IImage
{
    private readonly TextureRect _textureRect;

    private RTexture? _texture;

    public CImage(TextureRect textureRect, ITexture? texture)
    {
        Control = new CControl(textureRect);
        _textureRect = textureRect;
        Texture = texture;
    }

    public IControl Control { get; }

    public ITexture? Texture
    {
        get => _texture;
        set
        {
            if (value is RTexture rTexture)
            {
                _texture = rTexture;
                _textureRect.Texture = rTexture.Inner;
            }
            else
            {
                _texture = null;
                _textureRect.Texture = null;
            }
        }
    }
}

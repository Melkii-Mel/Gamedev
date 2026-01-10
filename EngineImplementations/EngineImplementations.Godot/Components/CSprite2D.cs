using EngineImplementations.GodotImplementation.Resources;
using Gamedev.Entities;
using Gamedev.Resources;
using Godot;
using Silk.NET.Maths;
using Engine = Gamedev.Engine;

namespace EngineImplementations.GodotImplementation.Components;

internal struct CSprite2D : ISprite2D
{
    private readonly Sprite _sprite2d;
    private ITexture? _texture;

    public CSprite2D(Sprite sprite2d, ITexture? texture)
    {
        _sprite2d = sprite2d;
        Texture = texture;
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
            if (value is not RTexture rTexture) return;
            var textureLoaderDefaults = Gamedev.EngineInstance.E.Resources.TextureLoader.Defaults;
            rTexture.Filter = textureLoaderDefaults.Filter;
            _texture = rTexture;
            _sprite2d.Texture = rTexture.Inner;
        }
    }
}

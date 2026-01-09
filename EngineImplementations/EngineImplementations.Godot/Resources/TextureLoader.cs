using EngineImplementations.GodotImplementation.Resources;
using Gamedev.Resources;
using Godot;

namespace EngineImplementations.GodotImplementation;

internal class TextureLoader : ITextureLoader
{
    public ITexture FromBytes(byte[] bytes)
    {
        var image = new Image();
        image.LoadPngFromBuffer(bytes);
        var texture = new ImageTexture();
        texture.CreateFromImage(image);
        return new RTexture(texture);
    }
}

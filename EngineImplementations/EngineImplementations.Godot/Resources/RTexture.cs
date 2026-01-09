using EngineImplementations.GodotImplementation.Components;
using Gamedev.Resources;
using Godot;
using Silk.NET.Maths;

namespace EngineImplementations.GodotImplementation.Resources;

internal class RTexture(Texture inner) : ITexture
{
    public Texture Inner { get; } = inner;

    public Vector2D<int> Size => Inner.GetSize().ToSilk().As<int>();
}
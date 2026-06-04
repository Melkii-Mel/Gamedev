using System;
using Gamedev;
using Gamedev.Entities;
using Gamedev.Entities.SceneManagement;
using Gamedev.Resources;
using static Gamedev.EngineInstance;

namespace Utils.Engine;

public static class Symbols
{
    public static readonly RootGroup Root = E.Scene;
    public static readonly SceneRoot<IControl> RootUi = Root.RootUi;
    public static readonly SceneRoot<INode2D> Root2D = Root.Root2D;
    public static readonly SceneRoot<INode3D> Root3D = Root.Root3D;
    public static readonly IEntities Entities = E.Entities;
    public static readonly IEntities.IUi Ui = Entities.Ui;
    public static readonly Resources Resources = E.Resources;
    public static readonly TextureLoader TextureLoader = Resources.TextureLoader;
    public static readonly FontRegistry FontRegistry = Resources.FontRegistry;
    public static void UpdateAdd(Action<float> h) => E.Update += h;
    public static void UpdateRemove(Action<float> h) => E.Update -= h;
}

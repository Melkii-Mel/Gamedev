using System;
using Gamedev.Debugging;
using Gamedev.Entities;
using Gamedev.Localization;
using Gamedev.Resources;
using Primitives;
using Primitives.Shapes;
using Silk.NET.Maths;

namespace Gamedev;

public static class Game
{
    public static void Start<TEngineImplementation, TGame>()
        where TEngineImplementation : IEngine, new()
        where TGame : IGame, new()
    {
        Start<TEngineImplementation, TGame>(new TEngineImplementation());
    }

    public static void Start<TEngineImplementation, TGame>(TEngineImplementation engineImplementation)
        where TEngineImplementation : IEngine
        where TGame : IGame, new()
    {
        EngineInstance.E = new Engine(engineImplementation);
        LocalizationCore.InitFromFile("localization.csv");
        Diagnostics.MessageTriggered += message => EngineInstance.E.E.DebugOutput.Out(message);
        new TGame().Start();
    }
}

public static class EngineInstance
{
    public static Engine E { get; internal set; } = null!;
}

/// <summary>
///     An interface acting as a bridge between the language-agnostic framework and specific engine implementations
/// </summary>
public interface IEngine
{
    IEntities Entities { get; }
    EntityComponent<INode> Root { get; }
    IDebugOutput DebugOutput { get; }
    Resources.Resources Resources { get; }

    /// <summary>
    ///     An event that should be invoked on each Update of the root entity, which is the entity that is not supposed to be
    ///     removed during the runtime
    /// </summary>
    public event Action<double>? Update;

    /// <summary>
    ///     An event that should be invoked on each Physics Update of the root entity, which is the entity that is not supposed
    ///     to be removed during the runtime
    /// </summary>
    public event Action<double>? PhysicsUpdate;

    // TODO: Consider removing
    public void Spawn(IEntity entity, IEntity parent);
}

/// <summary>
///     An interface for initializing common Entities in different engines.
/// </summary>
public interface IEntities
{
    IUi Ui { get; }
    I2D E2D { get; }
    I3D E3D { get; }

    EntityComponent<INode> Node();

    public interface IUi
    {
        EntityComponent<IControl> Control();
        EntityComponent<IButton> Button();
    }

    public interface I2D
    {
        EntityComponent<INode2D> Node();
        EntityComponent<ISprite2D> Sprite(ITexture? tilesTexture = null);
        EntityComponent<ITrigger2D> Trigger(Collider2D? collider2D = null);
    }

    public interface I3D
    {
        EntityComponent<INode3D> Node();
    }
}

public static class I2DExtensions
{
    public static EntityComponent<ITrigger2D> Trigger(this IEntities.I2D i2d, IShape2D shape2D,
        Vector2D<float>? pivot = null)
    {
        return i2d.Trigger(new Collider2D(shape2D, pivot ?? Defaults.DefaultPivot));
    }
}

public class Engine
{
    public Engine(IEngine engine)
    {
        E = engine;
        Scene = InitGlobalRoot();
        E.Update += Update;
    }

    public IEngine E { get; }
    public RootGroup Scene { get; }
    public IEntities Entities => E.Entities;
    public Resources.Resources Resources => E.Resources;

    public DebugOutput DebugOutput => new(E.DebugOutput);
    public EssentialSettings EssentialSettings { get; } = new();
    public event Action<double>? Update;

    private RootGroup InitGlobalRoot()
    {
        var root2D = E.Entities.E2D.Node();
        var root3D = E.Entities.E3D.Node();
        var rootControl = E.Entities.Ui.Control();
        E.Root.Entity.AddChild(root2D.Entity);
        E.Root.Entity.AddChild(root3D.Entity);
        E.Root.Entity.AddChild(rootControl.Entity);
        return new RootGroup(root2D, rootControl, root3D);
    }
}

public class EssentialSettings
{
    public bool Debug { get; set; } = true;
}

public interface IGame
{
    void Start();
}

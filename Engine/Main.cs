using System;
using Gamedev.Debugging;
using Gamedev.Entities;

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
        Localization.LocalizationCore.InitFromFile("localization.csv");
        Diagnostics.MessageTriggered += message => EngineInstance.E.E.DebugOutput.Out(message);
        new TGame().Start();
    }
}

public static class EngineInstance
{
    public static Engine E { get; internal set; } = null!;
}

/// <summary>
/// An interface acting as a bridge between the language-agnostic framework and specific engine implementations
/// </summary>
public interface IEngine
{
    /// <summary>
    /// An event that should be invoked on each Update of the root entity, which is the entity that is not supposed to be removed during the runtime
    /// </summary>
    public event Action<double>? Update;

    /// <summary>
    /// An event that should be invoked on each Physics Update of the root entity, which is the entity that is not supposed to be removed during the runtime
    /// </summary>
    public event Action<double>? PhysicsUpdate;

    // TODO: Consider removing
    public void Spawn(IEntity entity, IEntity parent);
    public IEntities Entities { get; }
    public EntityComponent<INode> Root { get; }
    public IDebugOutput DebugOutput { get; }
}

/// <summary>
/// An interface for initializing common Entities in different engines.
/// </summary>
public interface IEntities
{
    public interface IUi
    {
        public EntityComponent<IControl> Control();
        public EntityComponent<IButton> Button();
    }

    public interface I2D
    {
        public EntityComponent<INode2D> Node();
    }

    public interface I3D
    {
        public EntityComponent<INode3D> Node();
    }

    public EntityComponent<INode> Node();
    public IUi Ui { get; }
    public I2D E2D { get; }
    public I3D E3D { get; }
}

public class Engine
{
    public IEngine E { get; }
    public event Action<double>? Update;
    public RootGroup Scene { get; }
    public IEntities Entities => E.Entities;
    
    public IDebugOutput DebugOutput => E.DebugOutput;
    public EssentialSettings EssentialSettings { get; } = new();

    public Engine(IEngine engine)
    {
        E = engine;
        Scene = InitGlobalRoot();
        E.Update += Update;
    }

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
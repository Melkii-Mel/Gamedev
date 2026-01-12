using System;
using System.Collections.Generic;
using EngineImplementations.GodotImplementation.Components;
using EngineImplementations.GodotImplementation.EntitiesImplementations;
using Gamedev;
using Gamedev.Debugging;
using Gamedev.Entities;
using Gamedev.InputSystem.Api;
using Gamedev.InputSystem.Api.Devices;
using Gamedev.Resources;
using Godot;
using Silk.NET.Maths;
using TextureLoader = EngineImplementations.GodotImplementation.Resources.TextureLoader;

namespace EngineImplementations.GodotImplementation;

public class Implementation : IEngine
{
    public Implementation(ref Action<double>? update, ref Action<double>? physicsUpdate, Node root)
    {
        update += delta => Update?.Invoke(delta);
        PhysicsUpdate += delta => PhysicsUpdate?.Invoke(delta);
        Root = new EntityComponent<INode>(new Entity(root), new CNode());

        var inputCenter = new InputCenter();
        root.AddChild(inputCenter);
        Input = new Input(inputCenter);
        Display = new Display(root);
        Application = new Application(root);
    }

    public IDisplay Display { get; }
    public IApplication Application { get; }
    public event Action<double>? Update;
    public event Action<double>? PhysicsUpdate;

    public void Spawn(IEntity entity, IEntity parent)
    {
        var entityNode = Utils.As<Entity>(entity);
        var parentNode = Utils.As<Entity>(entity);
        parentNode.AddChild(entityNode);
    }

    public IEntities Entities { get; } = new Entities();
    public EntityComponent<INode> Root { get; }
    public IDebugOutput DebugOutput { get; } = new DebugOutput();

    public IResources Resources { get; } = new GdResources();

    public IInput Input { get; }
}

public class Application(Node node) : IApplication
{
    public void Quit()
    {
        node.GetTree().Quit(0);
    }
}

public class Display(Node node) : IDisplay
{
    public Vector2D<float> ScreenSize => OS.GetScreenSize().ToSilk();
    public Vector2D<float> ViewportSize => node.GetViewport().GetVisibleRect().Size.ToSilk();

    public DisplayMode DisplayMode
    {
        get =>
            OS.WindowFullscreen ? DisplayMode.Fullscreen :
            OS.WindowBorderless ? DisplayMode.Borderless : DisplayMode.Windowed;
        set
        {
            switch (value)
            {
                case DisplayMode.Windowed:
                    OS.WindowFullscreen = false;
                    OS.WindowBorderless = false;
                    break;
                case DisplayMode.Fullscreen:
                    OS.WindowFullscreen = true;
                    OS.WindowBorderless = false;
                    break;
                case DisplayMode.Borderless:
                    OS.WindowFullscreen = false;
                    OS.WindowBorderless = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    public Rectangle<float> WindowRect
    {
        get => new(OS.WindowPosition.ToSilk(), OS.WindowSize.ToSilk());
        set
        {
            OS.WindowPosition = value.Origin.ToGd();
            OS.WindowSize = value.Size.ToGd();
        }
    }
}

public class GdResources : IResources
{
    public ITextureLoader TextureLoader { get; } = new TextureLoader();
}

public class InputCenter : Node
{
    public event IKeyboard.KeyboardAction? KeyboardAction;
    public event IMouse.MouseAction? MouseAction;
    public event IController.ControllerAction? ControllerAction;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey { Echo: false } eKey)
            KeyboardAction?.Invoke(((KeyList)eKey.Scancode).ToString(), eKey.Pressed);
        // TODO: Finish after other input devices' API is complete
    }
}

public class Input : IInput
{
    public Input(InputCenter inputCenter)
    {
        Keyboard = new Keyboard(inputCenter);
        Mouse = new Mouse(inputCenter);
        Controller = new Controller(inputCenter);
    }

    public IKeyboard Keyboard { get; }

    public IMouse Mouse { get; }

    public IController Controller { get; }
}

internal class Controller : IController
{
    public Controller(InputCenter inputCenter)
    {
        inputCenter.ControllerAction += () => Action?.Invoke();
    }

    public event IController.ControllerAction? Action;
}

internal class Mouse : IMouse
{
    public Mouse(InputCenter inputCenter)
    {
        inputCenter.ControllerAction += () => Action?.Invoke();
    }

    public event IMouse.MouseAction? Action;
}

public class Keyboard : IKeyboard
{
    public Keyboard(InputCenter inputCenter)
    {
        inputCenter.KeyboardAction += (k, d) => Action?.Invoke(k, d);
    }

    public event IKeyboard.KeyboardAction? Action;

    public bool IsKeyDown(string key)
    {
        // Quick-patch solution
        return Godot.Input.IsPhysicalKeyPressed((int)Enum.Parse(typeof(KeyList), key.Capitalize()));
    }
}

public class DebugOutput : IDebugOutput
{
    public void Out(DebugMessage message)
    {
        switch (message.Type)
        {
            case MessageType.Info:
                GD.Print(message.Message);
                break;
            case MessageType.Warning:
                GD.PushWarning(message.Message);
                break;
            case MessageType.Error:
                GD.PushError(message.Message);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class Entities : IEntities
{
    public EntityComponent<INode> Node()
    {
        var node = new Node();
        return new EntityComponent<INode>(new Entity(node), new CNode());
    }

    public IEntities.IUi Ui { get; } = new Ui();
    public IEntities.I2D E2D { get; } = new E2D();
    public IEntities.I3D E3D { get; } = new E3D();
}

public class Entity(Node node) : IEntity
{
    private readonly Node _node = node;
    public event Action<IEntity>? ChildAdded;
    public event Action<IEntity>? ChildRemoved;

    public void AddChild(IEntity entity)
    {
        _node.AddChild(ToNode(entity));
        ChildAdded?.Invoke(entity);
    }

    public void RemoveChild(IEntity entity)
    {
        _node.RemoveChild(ToNode(entity));
        ChildRemoved?.Invoke(entity);
    }

    public IEntity RemoveChildAt(int index)
    {
        var child = new Entity(_node.GetChild(index));
        RemoveChild(child);
        return child;
    }

    public IEnumerable<IEntity> GetChildren()
    {
        foreach (var child in _node.GetChildren())
            if (child is Node node1)
                yield return new Entity(node1);
            else
                GD.PushWarning("One of node's children is not a Node.");
    }

    public void Free()
    {
        _node.QueueFree();
    }

    public void FreeRn()
    {
        _node.Free();
    }


    private static Node ToNode(IEntity entity)
    {
        return Utils.As<Entity>(entity)._node;
    }

    IEntity IEntityHolder.Entity => this;
}

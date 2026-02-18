using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamedev.Entities.SceneManagement;

/// <summary>
///     Abstraction over IEntity that provides a functionality of hierarchical scene management via PushScene and PopScene
///     methods.
/// </summary>
public partial class SceneRoot
{
    private readonly Stack<Func<IEntity>> _sceneStack = new();
    private IEntity? _lastScene;

    /// <summary>
    ///     Creates a new SceneRoot from an IEntity.
    /// </summary>
    /// <param name="entity">
    ///     IEntity to create SceneRoot from. Must be empty when creating a new SceneRoot, otherwise an
    ///     exception will be thrown.
    /// </param>
    /// <exception cref="ArgumentException">`entity` is not empty when creating a new SceneRoot.</exception>
    /// <exception cref="NotSupportedException">Thrown when trying to add or remove entities from a SceneRoot.</exception>
    public SceneRoot(Entity entity)
    {
        Entity = entity;
        if (entity.GetChildren().Count() != 0)
            throw new ArgumentException("SceneRoot's internal IEntity must be empty when creating a new SceneRoot");

        Entity.ChildAdded += _ =>
        {
            if (Entity.GetChildren().Count() > 1 ||
                (Entity.GetChildren().Count() == 1 && entity.GetChildren().First().Equals(_lastScene)))
                throw new NotSupportedException(
                    "can't freely add entities to a SceneRoot. SceneRoot's Entities management should be handled via the PushScene and PopScene methods");
        };
        Entity.ChildRemoved += _ =>
        {
            if (Entity.GetChildren().Count() != 0 || _lastScene != null)
                throw new NotSupportedException(
                    "can't freely remove entities from a SceneRoot. SceneRoot's Entities management should be handled via the PushScene and PopScene methods");
        };
    }

    public Entity Entity { get; }

    /// <summary>
    ///     Pushes a new scene to the stack and sets it as the current scene.
    /// </summary>
    /// <param name="source">
    ///     Function that returns a new scene. Must be able to return the scene multiple times to be able to
    ///     pop it.
    /// </param>
    public void PushScene(Func<IEntity> source)
    {
        _sceneStack.Push(source);
        SetScene(source());
    }

    // TODO: Consider caching the SceneRoot instead in order to preserve its state
    /// <summary>
    ///     Pushes a new SceneRoot to the stack and sets it as the current scene, allowing for hierarchical creation of Scenes.
    /// </summary>
    /// <param name="source">
    ///     Function that returns a new SceneRoot. Must be able to return the SceneRoot multiple times to be
    ///     able to pop it.
    /// </param>
    public void PushScene(Func<SceneRoot> source)
    {
        PushScene(() => source().Entity);
    }

    /// <summary>
    ///     Pops the current scene from the stack and sets the previous scene as the current scene.
    /// </summary>
    /// <exception cref="InvalidOperationException">The <see cref="SceneRoot" /> is empty</exception>
    public void PopScene()
    {
        DropScene();
        _sceneStack.Pop();
        if (_sceneStack.Count != 0)
            SetScene(_sceneStack.Peek()());
    }

    public void Popup(Func<IEntity> scene)
    {
        throw new NotImplementedException();
    }

    public void Popup(Func<SceneRoot> sceneRoot)
    {
        Popup(() => sceneRoot().Entity);
    }

    private void SetScene(IEntity scene)
    {
        _lastScene = scene;
        Entity.AddChild(scene);
    }

    private void DropScene()
    {
        _lastScene = null;
        Entity.RemoveChild(Entity.Children[0]);
    }

    public void ChangeScene(Func<IEntity> scene)
    {
        if (_sceneStack.Count > 0) PopScene();
        PushScene(scene);
    }
}

public class SceneRoot<T> : SceneRoot
{
    public SceneRoot(EntityComponent<T> entity) : base(entity.Entity)
    {
        Component = entity.Component;
    }

    public T Component { get; }
}

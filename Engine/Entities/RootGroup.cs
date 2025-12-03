using Gamedev.Entities.SceneManagement;

namespace Gamedev.Entities;

// TODO: Consider removing
public record RootGroup
{
    public RootGroup(EntityComponent<INode2D> root2D, EntityComponent<IControl> rootUi, EntityComponent<INode3D> root3D)
    {
        Root2D = new SceneRoot<INode2D>(root2D);
        RootUi = new SceneRoot<IControl>(rootUi);
        Root3D = new SceneRoot<INode3D>(root3D);
    }

    public SceneRoot<INode2D> Root2D { get; }
    public SceneRoot<IControl> RootUi { get; }
    public SceneRoot<INode3D> Root3D { get; }
}
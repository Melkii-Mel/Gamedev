namespace Sall.Properties;

public enum PropertyBehavior
{
    // Child receives computed value unless overridden.
    Inherited,

    // Does not inherit logically, but affects rendered subtree.
    Accumulated,

    // Only affects this node.
    SelfOnly,

    // Affects layout relationships.
    Layout,

    // Alters subtree traversal/participation.
    Subtree,

    // Context lookup / theme propagation.
    Context,
}

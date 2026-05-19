using System;
using System.Collections.Generic;
using System.Linq;
using Sall.Evaluation;
using Sall.Lowering;

namespace Sall.Api;

public record struct Dependency(Node Node, StringId OfProperty, StringId OnProperty);

public class DirtyNodesRegister : List<Node>;

public record Node(
    Type Type,
    StringId[] Markers,
    // TODO (later): Consider changing the data structure for children storage
    List<Node> Children,
    Node? Parent,
    List<Dependency> DirectDeps,
    List<Dependency> Dependents,
    NodeLayout NodeLayout
)
{
    public event Action<Node>? ChildAddedRecursive;
    public event Action<Node>? ChildRemoved;
    public ValueSet<StringId> Dirt = [];

    public void AddChild(Node node)
    {
        Children.Add(node);
        InvokeChildAdded(node);
    }

    public bool RemoveChild(Node node)
    {
        var removed = Children.Remove(node);
        if (removed) InvokeChildRemoved(node);
        return removed;
    }

    public void RemoveChild(int index)
    {
        var removed = Children[index];
        Children.RemoveAt(index);
        InvokeChildRemoved(removed);
    }
    
    public void AddDependency(Node node, StringId ofProp, StringId onProp)
    {
        DirectDeps.Add(new Dependency(node, ofProp, onProp));
        node.Dependents.Add(new Dependency(this, onProp, ofProp));
        // TODO: Invalidate or comment to call at the right moment
    }

    private void InvokeChildAdded(Node node)
    {
        ChildAddedRecursive?.Invoke(node);
        Parent?.InvokeChildAdded(node);
    }

    private void InvokeChildRemoved(Node node)
    {
        ChildRemoved?.Invoke(node);
        Parent?.InvokeChildRemoved(node);
    }

    /// <summary>
    /// Calculates layout context immediately.
    /// Does not invalidate itself.
    /// Make sure to call it only when all the dependencies are resolved.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public LayoutContext CalculateContext()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Marks node as dirty and invalidates its dependents.
    /// Thus, all the nodes that are directly or indirectly depend on this node are also marked as Dirty
    /// </summary>
    public void Invalidate(StringId prop, DirtyNodesRegister dnr)
    {
        Dirt.Add(prop);
        dnr.Add(this);
        foreach (var dependent in Dependents.Where(d => d.OnProperty == prop))
            dependent.Node.Invalidate(dependent.OfProperty, dnr);
    }

    /// <summary>
    /// Updates all the dependencies necessary
    /// </summary>
    public void UpdateAllConnectionsIfDirty()
    {
    }

    public void CalculateDependencies()
    {
    }

    // TODO: Consider deleting
    public bool UpdateDirty() => Dirty = DirectDeps.Any(d => d.Dirty);

    public bool HasCircularDependencies()
    {
        throw new NotImplementedException();
    }

    // TODO: Prevent endless loop due to circular dependencies
    public List<Node> GetAllDeps()
    {
        var result = new List<Node>();
        foreach (var directDep in DirectDeps)
        {
            result.Add(directDep);
            foreach (var dep in directDep.GetAllDeps())
            {
                if (ReferenceEquals(dep, this))
                {
                    // TODO (later): Exception message
                    throw new Exception();
                }

                result.Add(dep);
            }
        }

        return result;
    }

    public void Resolve()
    {
        ResolveDeps();
        ResolveSelf();
    }

    private void ResolveSelf()
    {
    }

    private void ResolveDeps()
    {
    }
}

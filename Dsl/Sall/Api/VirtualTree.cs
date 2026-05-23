using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Sall.Evaluation;
using Sall.Lowering;
using Sall.Properties;
using Utils.Extensions;

namespace Sall.Api;

public class TreeContext
{
    internal Dictionary<Node, List<Dependency>> DependenciesMap { get; } = [];
}

public enum DependencyDirection
{
    Parent,
    Children,
}

public record struct Dependency(DependencyDirection Direction, StringId OfProperty, StringId OnProperty);

public class DirtyNodesRegister : List<Node>;

public class Node
{
    // !!! Consider alternative
    public Type Type { get; }
    public IReadOnlyList<StringId> Markers => _markers;
    public IReadOnlyList<Node> Children => _children;
    public Node? Parent { get; private set; }
    public TreeContext Context { get; private set; }

    public Properties Properties
    {
        get => _properties;
        private set => _properties = value;
    }

    private List<StringId> _markers { get; } = [];
    private List<Node> _children { get; } = [];
    private Properties _properties;

    public event Action<Node>? ChildAddedRecursive;
    public event Action<Node>? ChildRemoved;

    public void Update(float delta)
    {
        
    }

    public void Recompute(StringId property)
    {
        var selfValue = GetClassPropertyValue(property);
        switch (PropertyBehaviorMap.GetBehavior(property))
        {
            case PropertyBehavior.Inherited:
                break;
            case PropertyBehavior.Accumulated:
                Properties[property] = Parent?.Properties[property] * selfValue;
                break;
            case PropertyBehavior.SelfOnly:
                break;
            case PropertyBehavior.Layout:
                break;
            case PropertyBehavior.Subtree:
                break;
            case PropertyBehavior.Context:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void AddChild(Node node)
    {
        _children.Add(node);
        HandleChildAdded(node);
        InvokeChildAdded(node);
    }

    public bool RemoveChild(Node node)
    {
        var removed = _children.Remove(node);
        HandleChildRemoved(node);
        if (removed) InvokeChildRemoved(node);
        return removed;
    }

    public void RemoveChild(int index)
    {
        var removed = Children[index];
        _children.RemoveAt(index);
        HandleChildRemoved(removed);
        InvokeChildRemoved(removed);
    }

    private void HandleChildAdded(Node node)
    {
    }

    private void HandleChildRemoved(Node node)
    {
        foreach (var childrenDependency in ChildrenDependencies())
        {
            childrenDependency
        }
    }

    private IEnumerable<Dependency> ParentDependencies()
    {
        return Context.DependenciesMap[this].Where(d => d.Direction == DependencyDirection.Parent);
    }

    private IEnumerable<Dependency> ChildrenDependencies()
    {
        return Dependencies().Where(d => d.Direction == DependencyDirection.Children);
    }

    private List<Dependency> Dependencies()
    {
        return Context.DependenciesMap.GetOrInit(this, static () => []);
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

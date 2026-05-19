using System;
using System.Collections.Generic;
using System.Linq;
using Sall.Ast;
using Sall.Evaluation;
using Sall.Lowering;
using Utils.Collections;

namespace Sall.Api;

internal record EngineUpdateCache(
    HashSet<StringId> Disabled,
    HashSet<StringId> JustEnabledOrAdded,
    HashSet<Node> Track,
    HashSet<Node> Untrack);

public partial class Engine : IEngine
{
    private List<Stylespace> _stylespaces = [];
    private Dictionary<StringId, Stylespace> _stylespaceIdMap = [];
    private readonly Dictionary<StringId, Stylespace> _disabled = [];
    private readonly WeakSet<Node> _registered = [];
    private bool _reloadQueued;
    private List<WeakReference<Node>> _dirtyNodes = [];
    private MarkerIndex _markerIndex = new([]);

    public void QueueReload() => _reloadQueued = true;

    public void AddStylesheet(Stylesheet stylesheet)
    {
        _stylespaces = new[] { _stylespaces, stylesheet.ToApiStylespaces() }.Merge().ToList();
        _stylespaceIdMap = _stylespaces.ToDictionary(ss => ss.Name);
        QueueReload();
    }

    public void ToggleStylespaceState(StringId id, bool? state = null)
    {
        if (state.HasValue) SetStylespaceStateAndReload(id, state.Value);
        SetStylespaceStateAndReload(id, _disabled.ContainsKey(id));
        QueueReload();
    }

    private void SetStylespaceStateAndReload(StringId id, bool state)
    {
        if (state) _disabled[id] = _stylespaceIdMap[id];
        else _disabled.Remove(id);
        QueueReload();
    }

    public void RegisterNode(Node root)
    {
        _registered.Add(root);
        root.ChildAddedRecursive += SetDirtyRecursive;
        // TODO (later): Maybe should also listen to child removal, not sure yet
        SetDirtyRecursive(root);
    }

    private void SetDirtyRecursive(Node node)
    {
        _dirtyNodes.Add(new WeakReference<Node>(node));
        foreach (var nodeChild in node.Children) SetDirtyRecursive(nodeChild);
    }

    public void UnregisterNode(Node root)
    {
        root.ChildAddedRecursive -= SetDirtyRecursive;
        StopListeningSomething() ?? StopListeningSomething();
    }

    public bool IsRegistered(Node root)
    {
        return _registered.Contains(root);
    }

    public void Update(float delta)
    {
        if (_reloadQueued)
        {
            _reloadQueued = false;
            Reload();
            return;
        }
    }

    private void Reload()
    {
        var activeStylespaces = ActiveStylespaces();
        _markerIndex = new MarkerIndex(activeStylespaces.SelectMany(s =>
            new ClassNormalizer(s, s.Scope).NormalizeClassesFlat()));
        foreach (var registeredNode in _registered) ReloadNode(registeredNode);
    }

    private Stylespace[] ActiveStylespaces()
    {
        return _stylespaces.Where(ss => !_disabled.ContainsKey(ss.Name)).ToArray();
    }

    private void ReloadNode(Node node)
    {
        var classesToApply = _markerIndex.GetClassesFor(node.Markers);
        foreach (var child in node.Children) ReloadNode(child);
    }
}

// TODO: Error handling (severity levels, debug printing instead of throwing)
// TODO: Weight
public partial class Engine
{
    public Engine()
    {
        var globalScope = Scope.FromStylespace(stylespace);
        var classNormalizer = new ClassNormalizer(stylespace, globalScope);
        _normalizedClasses = classNormalizer.NormalizeClasses();
    }

    private void Apply(Node node)
    {
        if (_normalizedClasses.TryGetValue(ValueSet<string>.From(node.Markers), out var list))
        {
            foreach (var normalizedClass in list)
            {
                foreach (var normalizedClassProperty in normalizedClass.Properties)
                {
                }
            }
        }
    }
}

internal interface IEnginePrelude
{
    IEngine Engine { get; }
    void LoadStylesheet(string filename);
    void RegisterNode(Node root);
    void Update(float delta);
}

internal interface IEngine
{
    void AddStylesheet(Stylesheet stylesheet);
    void ToggleStylespaceState(StringId id, bool? state = null);
    void RegisterNode(Node root);
    void UnregisterNode(Node root);
    bool IsRegistered(Node root);
    void Update(float delta);
    void QueueReload();
}

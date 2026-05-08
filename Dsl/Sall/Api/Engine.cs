using System.Collections.Generic;
using Sall.Ast;
using Sall.Evaluation;
using Sall.Lowering;

namespace Sall.Api;

public partial class Engine : IEngine
{
    public void ToggleStylespaceState(StringId id, bool? state = null)
    {
        
        throw new System.NotImplementedException();
    }

    public void RegisterNode(Node root)
    {
        throw new System.NotImplementedException();
    }

    public void UnregisterNode(Node root)
    {
        throw new System.NotImplementedException();
    }

    public bool IsRegistered(Node root)
    {
        throw new System.NotImplementedException();
    }
}

// TODO: Error handling (severity levels, debug printing instead of throwing)
// TODO: Weight
public partial class Engine
{
    private NormalizedClasses _normalizedClasses;

    public Engine(Node root, Stylespace stylespace)
    {
        var globalScope = Scope.FromStylespace(stylespace);
        var classNormalizer = new ClassNormalizer(stylespace, globalScope);
        _normalizedClasses = classNormalizer.NormalizeClasses();


        return;
    }

    public void AddStylesheet(Stylesheet stylesheet)
    {
    }

    private void Apply(Node node)
    {
        if (_normalizedClasses.TryGetValue(ValueSet<string>.From(node.Classes), out var list))
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
    void LoadStylesheet(string filename);
    void UnloadStylesheet(string filename);
    void RegisterNode(Node root);
    void UnregisterNode(Node root);
}

internal interface IEngine
{
    void AddStylesheet(Stylesheet stylesheet);
    void ToggleStylespaceState(StringId id, bool? state = null);
    void RegisterNode(Node root);
    void UnregisterNode(Node root);
    bool IsRegistered(Node root);
}

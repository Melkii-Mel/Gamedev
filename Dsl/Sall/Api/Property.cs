using System.Collections.Generic;
using Sall.Ast;
using Sall.Evaluation;
using Sall.Lowering;
using Utils.Extensions;

namespace Sall.Api;

public class Properties
{
    private Dictionary<StringId, List<NormalizedProperty>> _propertyMap = [];
    private HashSet<StringId> _dirtyProperties = [];

    // 
    private Dictionary<StringId, List<Transition>> _transitions = [];
    // TODO: Animations property
    // TODO: Transitions property

    public Properties()
    {
        
    }

    public void Add(StringId name, NormalizedProperty property)
    {
        _propertyMap.GetOrInit(name).Add(property);
        _dirtyProperties.Add(name);
    }

    public void Remove(StringId name, NormalizedProperty property)
    {
        _propertyMap.GetOrInit(name).Remove(property);
        _dirtyProperties.Add(name);
    }

    public void Update()
    {
        foreach (var dirtyProperty in _dirtyProperties)
        {
            if (TryGetTransition(dirtyProperty, out var propertyTransition))
            {
                HandleTransition(propertyTransition);
                continue;
            }

            UpdateProperty(dirtyProperty);
        }

        // TODO: Handle animations
        // TODO: Handle transitions
        
        return;

        bool TryGetTransition(StringId propertyId, out PropertyTransition propertyTransition)
        {
            
        }

        void HandleTransition(PropertyTransition propertyTransition)
        {
            
        }
    }
}

public struct CascadingProperties
{
}

public struct Layout
{
    public float Width, Height, X, Y;
    public bool WidthDirty, HeightDirty, XDirty, YDirty;
}

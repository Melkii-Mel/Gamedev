using System.Collections.Generic;
using Sall.Evaluation;
using Sall.Lowering;
using Utils.Extensions;

namespace Sall.Api;

public class Properties
{
    public Dictionary<StringId, List<NormalizedProperty>> _propertyMap = [];
    public HashSet<StringId> _dirtyProperties = [];
    // TODO: Animations property
    // TODO: Transitions property

    public Properties()
    {
        
    }

    public void Add(StringId name, NormalizedProperty property)
    {
        _propertyMap.GetOrInit(name, () => []).Add(property);
        _dirtyProperties.Add(name);
    }

    public void Update()
    {
        foreach (var dirtyProperty in _dirtyProperties)
        {
            if (HandleTransitionIfExists(dirtyProperty)) continue;
            
        }

        // TODO: Handle animations
        // TODO: Handle transitions
        
        return;

        bool HandleTransitionIfExists(StringId propertyId)
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

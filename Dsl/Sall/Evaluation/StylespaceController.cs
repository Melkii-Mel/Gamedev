using System.Collections.Generic;
using Sall.Api;
using Sall.Ast;

namespace Sall.Evaluation;

public class StylespaceController
{
    private Dictionary<StringId, Stylespace> _stylespaces = [];

    public StylespaceController(IEnumerable<Stylesheet>? initialStylesheets)
    {
        
    }

    public void AddStylesheet(Stylesheet stylesheet)
    {
        foreach (var stylespace in stylesheet.AstStylespaces)
        {
            var id = StringId.FromString(stylespace.Ident);
            if (_stylespaces.TryGetValue(id, out var registeredStylespace))
            {
                registeredStylespace.Merge(stylespace);
            }
            else
            {
                _stylespaces[id] = stylespace;
            }
        }
    }

    public void RemoveStylesheet(Stylesheet stylesheet)
    {
        
    }

    public void UpdateStylesheet()
    {
        
    }

    public void ToggleStylespace()
    {
        
    }

    public void SetStylespaceState()
    {
        
    }
}

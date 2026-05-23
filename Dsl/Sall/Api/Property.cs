using System.Collections.Generic;
using Sall.Ast;
using Sall.Evaluation;

namespace Sall.Api;

public struct Properties
{
    public Layout Layout;
    public Dictionary<StringId, Expr> InheritedOverrides;
    public Dictionary<>
}

public struct CascadingProperties
{
    
}

public struct Layout
{
    public float Width, Height, X, Y;
    public bool WidthDirty, HeightDirty, XDirty, YDirty;
}

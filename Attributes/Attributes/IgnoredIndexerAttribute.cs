using System;

namespace Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class IgnoredIndexerAttribute : Attribute
{
    public Type[] ParamTypes { get; }

    public IgnoredIndexerAttribute(Type[] paramTypes)
    {
        ParamTypes = paramTypes;
    }
}
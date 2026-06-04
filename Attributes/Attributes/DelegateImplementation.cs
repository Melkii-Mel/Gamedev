using System;

namespace Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class DelegateImplementationAttribute : Attribute
{
    public Type InterfaceType { get; }
    public string TargetProperty { get; }
    public DelegationStyle DelegationStyle { get; }
    public string[] IgnoredMembers { get; }

    // See the attribute guidelines at 
    // http://go.microsoft.com/fwlink/?LinkId=85236
    public DelegateImplementationAttribute(
        Type interfaceType, 
        string targetProperty,
        DelegationStyle delegationStyle = DelegationStyle.Implicit,
        string[]? ignoredMembers = null
    )
    {
        InterfaceType = interfaceType;
        TargetProperty = targetProperty;
        DelegationStyle = delegationStyle;
        IgnoredMembers = ignoredMembers ?? [];
    }
}

public enum DelegationStyle
{
    Implicit,
    Explicit,
}

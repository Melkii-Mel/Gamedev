using System;

namespace Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class DelegateImplementationAttribute : Attribute
{
    public Type InterfaceType { get; }
    public string TargetProperty { get; }

    public DelegationStyle DelegationStyle { get; }

    // See the attribute guidelines at 
    // http://go.microsoft.com/fwlink/?LinkId=85236
    public DelegateImplementationAttribute(Type interfaceType, string targetProperty, DelegationStyle delegationStyle = DelegationStyle.Implicit)
    {
        InterfaceType = interfaceType;
        TargetProperty = targetProperty;
        DelegationStyle = delegationStyle;
    }
}

public enum DelegationStyle
{
    Implicit,
    Explicit,
}

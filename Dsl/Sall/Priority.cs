using System;

namespace Sall;

public struct Priority : IComparable<Priority>
{
    public int Specificity;
    public int OrderingIndex;

    public int CompareTo(Priority other)
    {
        var specificityComparison = Specificity.CompareTo(other.Specificity);
        return specificityComparison != 0 ? specificityComparison : OrderingIndex.CompareTo(other.OrderingIndex);
    }
}
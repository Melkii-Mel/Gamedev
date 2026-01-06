using System.Collections;
using System.Collections.Generic;

namespace Utils.DataStructures;

public class Tree<T> : IEnumerable<T>
{
    private List<Tree<T>> Children { get; } = [];
    public T? Value { get; set; }

    public void Add(Tree<T> node)
    {
        Children.Add(node);
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (Value != null)
        {
            yield return Value;
        }
        foreach (var child in Children)
        {
            foreach (var value in child)
            {
                yield return value;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

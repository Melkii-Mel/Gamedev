using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Collections;

public class Tree<T> : IEnumerable<T>
{
    private List<Tree<T>> Children { get; } = [];
    public T? Value { get; set; }

    public IEnumerator<T> GetEnumerator()
    {
        if (Value != null) yield return Value;
        foreach (var value in Children.SelectMany(child => child))
            yield return value;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(Tree<T> node)
    {
        Children.Add(node);
    }
}

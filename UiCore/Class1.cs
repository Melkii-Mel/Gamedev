using System;
using System.Collections.Generic;
using Sall;

namespace UiCore;

public record Node(
    Type Type,
    string[] Classes,
    Node[] Children
);



// TODO: Error handling (severity levels, debug printing instead of throwing)
// TODO: Weight
public class Engine(Node root, Stylespace stylespace)
{
    private Dictionary<Node, Dictionary<string, Node>> _index = [];

    public void AddStylesheet(Stylesheet stylesheet)
    {
    }

    private void Apply(Node node)
    {
        node
    }
}

using Godot;
using System.Collections.Generic;

namespace ColdNight.src;

public static class NodeExtension
{
    public static bool IsActive (this Node node) => GodotObject.IsInstanceValid(node) && node.IsInsideTree();

    public static List<Node> GetChildrenWithRid(this Node root)
    {
        var result = new List<Node>();
        var stack  = new Stack<Node>();
        
        stack.Push(root);

        while (stack.Count > 0)
        {
            var n = stack.Pop();

            if (n is CollisionObject3D)
                result.Add(n);

            foreach (var c in n.GetChildren())
                stack.Push(c);
        }

        return result;
    }

    public static List<Rid> GetCollisionRids(this Node root)
    {
        var result = new List<Rid>();
        var stack  = new Stack<Node>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var n = stack.Pop();

            if (n is CollisionObject3D co)
                result.Add(co.GetRid());

            foreach (var c in n.GetChildren())
                stack.Push(c);
        }

        return result;
    }

    public static List<(CollisionObject3D Node, Rid Rid)> GetCollisionObjectsWithRid(this Node root)
    {
        var result = new List<(CollisionObject3D, Rid)>();
        var stack  = new Stack<Node>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var n = stack.Pop();

            if (n is CollisionObject3D co)
                result.Add((co, co.GetRid()));

            foreach (var c in n.GetChildren())
                stack.Push(c);
        }

        return result;
    }

    public static IEnumerable<Node> GetAllDescendants(this Node root)
    {
        foreach ( Node child in root.GetChildren())
        {
            yield return child;
            foreach (var sub in GetAllDescendants(child))
                yield return sub;
        }
    }

    public static IEnumerable<T> GetAllDescendants<T>(this Node root, bool descendIntoMatches = true, bool includeSelf = false) where T : Node
    {
        if (includeSelf && root is T self)
            yield return self;

        foreach ( Node child in root.GetChildren())
        {
            if (child is T t)
            {
                yield return t;
                if (!descendIntoMatches) continue;
            }

            foreach (var sub in GetAllDescendants<T>(child, descendIntoMatches))
                yield return sub;
        }
    }
}
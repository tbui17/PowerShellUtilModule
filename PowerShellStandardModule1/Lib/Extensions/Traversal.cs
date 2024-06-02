using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PowerShellStandardModule1.Models;

namespace PowerShellStandardModule1.Lib.Extensions;

public static class Traversal
{
    public static IEnumerable<T> Bfs<T>(T root, Func<T, IEnumerable<T>> getChildren)
    {
        var queue = new Queue<T>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var dir = queue.Dequeue();
            yield return dir;
            foreach (var subDir in getChildren(dir))
            {
                queue.Enqueue(subDir);
            }
        }
    }

    public static IEnumerable<TreeNode<T>> BfsDetailed<T>(T root, Func<T, IEnumerable<T>> getChildren) =>
        BfsDetailed(root, getChildren.WithTreeNodeAdapter());

    public static IEnumerable<TreeNode<T>> BfsDetailed<T>(T root, Func<TreeNode<T>, IEnumerable<T>> getChildren) =>
        BfsDetailed(root, getChildren.WithTreeNodeAdapter());

    private static IEnumerable<TreeNode<T>> BfsDetailed<T>(
        T root,
        Func<TreeNode<T>, IEnumerable<TreeNode<T>>> adaptedGetter
    )
    {
        var item = new TreeNode<T> { Value = root };
        var queue = new Queue<TreeNode<T>>();
        queue.Enqueue(item);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();

            yield return node;
            

            var children = adaptedGetter(node).ToList();

            node.Children = children;
            queue.EnqueueRange(children);
        }
    }

    public static IEnumerable<T> Dfs<T>(T root, Func<T, IEnumerable<T>> getChildren)
    {
        var stack = Stack.From([root]);

        while (stack.NotEmpty())
        {
            var current = stack.Pop();
            yield return current;

            stack.PushRange(getChildren(current));
        }
    }

    public static IEnumerable<T> DfsDetailed<T>(T root, Func<T, IEnumerable<T>> getChildren)
    {
        var stack = Stack.From([new TreeNode<T> { Value = root }]);

        var adaptedGetter = getChildren.WithTreeNodeAdapter();

        while (stack.NotEmpty())
        {
            var current = stack.Pop();
            yield return current.Value;

            var children = adaptedGetter(current).ToImmutableList();

            current.Children = children;
            stack.PushRange(children);
        }
    }

    public static Func<TreeNode<T>, IEnumerable<TreeNode<T>>> WithTreeNodeAdapter<T>(
        this Func<TreeNode<T>, IEnumerable<T>> getChildrenFromTreeNode
    ) =>
        node => TreeNodeAdapter(node, getChildrenFromTreeNode);

    public static Func<TreeNode<T>, IEnumerable<TreeNode<T>>> WithTreeNodeAdapter<T>(
        this Func<T, IEnumerable<T>> getChildrenFromValue
    ) =>
        node => TreeNodeAdapter(node, n => getChildrenFromValue(n.Value));

    private static IEnumerable<TreeNode<T>> TreeNodeAdapter<T>(
        TreeNode<T> node,
        Func<TreeNode<T>, IEnumerable<T>> getChildren
    )
    {
        var parentHeightPlusOne = node.Height + 1;

        return getChildren(node)
           .Select(
                (x, i) => new TreeNode<T>
                {
                    Value = x,
                    Height = parentHeightPlusOne,
                    Parent = node,
                    Index = i
                }
            );
    }
}
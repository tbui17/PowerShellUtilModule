using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PowerShellStandardModule1.Structs;

namespace PowerShellStandardModule1;

public static class Extensions
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

    public static IEnumerable<TreeNode<T>> BfsDetailed<T>(T root, Func<T, IEnumerable<T>> getChildren)
    {
        var item = new TreeNode<T> { Value = root };
        var queue = new Queue<TreeNode<T>>();
        queue.Enqueue(item);
        var adaptedGetter = getChildren.WithTreeNodeAdapter();

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();

            yield return node;

            var children = adaptedGetter(node).ToImmutableList();

            node.Children = children;
            queue.EnqueueRange(children);
        }
    }

    public static Func<TreeNode<T>, IEnumerable<TreeNode<T>>> WithTreeNodeAdapter<T>(
        this Func<T, IEnumerable<T>> getChildren
    )
    {
        return Adapter;

        IEnumerable<TreeNode<T>> Adapter(TreeNode<T> node)
        {
            var parentHeightPlusOne = node.Height + 1;

            return getChildren(node.Value)
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

    public static IEnumerable<T> Bfs<T>(this Func<T, IEnumerable<T>> childGetter, T root) => Bfs(root, childGetter);

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


    public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            queue.Enqueue(item);
        }
    }

    public static void PushRange<T>(this Stack<T> stack, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            stack.Push(item);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }

    public static void ForEach<T, TReturn>(this IEnumerable<T> items, Func<T, TReturn> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }

    public static Stack<T> ToStack<T>(this IEnumerable<T> items) => new(items);

    public static bool NotEmpty<T>(this Stack<T> stack) => stack.Count != 0;

    public static TReturn Pipe<T, TReturn>(this T src, Func<T, TReturn> fn) => fn(src);

    public static T Tap<T>(this T src, Action<T> fn)
    {
        fn(src);
        return src;
    }

    public static T Tap<T, TReturn>(this T src, Func<T, TReturn> fn)
    {
        fn(src);
        return src;
    }

    public static (IList<T> True, IList<T> False) PartitionBy<T>(this IEnumerable<T> source, Predicate<T> predicate)
    {
        var left = new List<T>();
        var right = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item))
            {
                left.Add(item);
            }
            else
            {
                right.Add(item);
            }
        }

        return (left, right);
    }

    public static Func<T, TReturn> Compose<T, TIntermediate, TReturn>(
        this Func<T, TIntermediate> fn1,
        Func<TIntermediate, TReturn> fn2
    ) =>
        x => fn2(fn1(x));
    
    
    public static string StringJoin<T>(this IEnumerable<T> items, string separator) => string.Join(separator, items);
}

public static class Stack
{
    public static Stack<T> From<T>(IEnumerable<T> items) => new(items);
}
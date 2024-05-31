using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PowerShellStandardModule1.Structs;

namespace PowerShellStandardModule1.Lib;

public abstract class Traverser<T>
{
    protected abstract void Enqueue(T value);

    protected abstract bool IsEmpty { get; }
    protected abstract T Dequeue();
}

public abstract class BasicTraverser<T> : Traverser<T>
{
    protected abstract IEnumerable<T> GetChildren(T value);

    public IEnumerable<T> Invoke(T root)
    {
        Enqueue(root);
        while (!IsEmpty)
        {
            var value = Dequeue();
            yield return value;

            Process(value);
        }
    }

    private void Process(T node)
    {
        foreach (var child in GetChildren(node))
        {
            Enqueue(child);
        }
    }
}

public abstract class TreeTraverser<T> : Traverser<TreeNode<T>>
{
    public IEnumerable<TreeNode<T>> Invoke(T node)
    {
        var root = new TreeNode<T> { Value = node };
        Enqueue(root);
        while (!IsEmpty)
        {
            var value = Dequeue();
            yield return value;

            Process(value);
        }
    }


    private void Process(TreeNode<T> node)
    {
        var parentHeightPlusOne = node.Height + 1;

        var children = GetChildren(node)
           .Select(
                (x, i) => new TreeNode<T>
                {
                    Value = x,
                    Height = parentHeightPlusOne,
                    Parent = node,
                    Index = i
                }
            )
           .ToImmutableList();

        node.Children = children;
        foreach (var child in children)
        {
            Enqueue(child);
        }
    }

    protected abstract IEnumerable<T> GetChildren(TreeNode<T> value);
}

public abstract class BfsTraverser<T> : BasicTraverser<T>
{
    private readonly Queue<T> _queue = new();

    protected override void Enqueue(T value)
    {
        _queue.Enqueue(value);
    }

    protected override bool IsEmpty => _queue.Count == 0;

    protected override T Dequeue()
    {
        return _queue.Dequeue();
    }
}

public abstract class BfsDetailedTraverser<T> : TreeTraverser<T>
{
    private readonly Queue<TreeNode<T>> _queue = new();

    protected override void Enqueue(TreeNode<T> value)
    {
        _queue.Enqueue(value);
    }

    protected override bool IsEmpty => _queue.Count == 0;

    protected override TreeNode<T> Dequeue()
    {
        return _queue.Dequeue();
    }
}

public abstract class DfsTraverser<T> : BasicTraverser<T>
{
    private readonly Stack<T> _stack = new();

    protected override void Enqueue(T value)
    {
        _stack.Push(value);
    }

    protected override bool IsEmpty => _stack.Count == 0;

    protected override T Dequeue()
    {
        return _stack.Pop();
    }
}

public abstract class DfsDetailedTraverser<T> : TreeTraverser<T>
{
    private readonly Stack<TreeNode<T>> _stack = new();

    protected override void Enqueue(TreeNode<T> value)
    {
        _stack.Push(value);
    }

    protected override bool IsEmpty => _stack.Count == 0;

    protected override TreeNode<T> Dequeue()
    {
        return _stack.Pop();
    }
}
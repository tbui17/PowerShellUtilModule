using System;
using System.Collections.Generic;
using System.Linq;
using PowerShellStandardModule1.Models;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class BfsExecutor<T>
{
    public Func<TreeNode<T>, bool> Filter { get; set; } = _ => true;
    public Func<TreeNode<T>, bool> ShouldBreak { get; set; } = _ => true;
    public Func<T, IEnumerable<T>> ChildProvider { get; set; } = _ => [];
    public int Count { get; private set; }
    private Queue<TreeNode<T>> Queue { get; } = new();

    public IEnumerable<TreeNode<T>> Invoke(T root)
    {
        Count = 0;
        Queue.Clear();
        Queue.Enqueue(new TreeNode<T> { Value = root });

        while (Queue.Count > 0)
        {
            var next = Queue.Dequeue();
            yield return next;


            var newChildHeight = next.Height + 1;


            var count = Count;
            var nodes = ChildProvider(next.Value)
               .Select(
                    (x, i) => new TreeNode<T>
                    {
                        Value = x,
                        Height = newChildHeight,
                        Index = i,
                        Parent = next,
                        Count = count + i + 1
                    }
                );

            foreach (var node in nodes)
            {
                Count++;
                if (ShouldBreak(node)) yield break;
                if (!Filter(node)) continue;

                node.Parent?.Children.Add(node);
                Queue.Enqueue(node);
            }
        }
    }
}
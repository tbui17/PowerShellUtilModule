using System;
using System.Collections.Generic;
using System.Linq;
using PowerShellStandardModule1.Lib.Extensions;
using PowerShellStandardModule1.Models;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class BfsExecutor<T>
{
    public Func<TreeNode<T>, bool> Where { get; set; } = _ => true;
    public Func<TreeNode<T>, bool> ShouldBreak { get; set; } = _ => true;
    public Func<T, IEnumerable<T>> ChildProvider { get; set; } = _ => [];


    public IEnumerable<TreeNode<T>> Invoke(T root)
    {
        var queue = Queue.StartWith(new TreeNode<T> { Value = root });
        var count = 0;

        while (queue.NotEmpty())
        {
            var next = queue.Dequeue();
            yield return next;
            
            var newChildHeight = next.Height + 1;


            var nodes = ChildProvider(next.Value)
               .Select(
                    (x, i) =>
                    {
                        count++;
                        return new TreeNode<T>
                        {
                            Value = x,
                            Height = newChildHeight,
                            Index = i,
                            Parent = next,
                            Count = count
                        };
                    }
                );

            foreach (var node in nodes)
            {
                if (ShouldBreak(node)) yield break;
                if (!Where(node)) continue;

                node.Parent?.Children.Add(node);
                queue.Enqueue(node);
            }
        }
    }
}
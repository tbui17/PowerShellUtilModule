using System;
using System.Collections.Generic;

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
    
    public static IEnumerable<T> Bfs<T>(this Func<T,IEnumerable<T>> childGetter, T root) => Bfs(root, childGetter);
}
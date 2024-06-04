using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PowerShellStandardModule1.Lib.Extensions;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class WithinHandler(bool within, Func<FileSystemInfo, bool> filter, CancellationToken cancellationToken)
{
    public void ProcessResult(IList<FileSystemInfoTreeNode> result)
    {
        
        if (!within) return;

        var branches = GetBranchesSatisfyingFilter(result);
        foreach (var node in result)
        {
            node.Children = node
               .Children
               .Where(branches.Contains)
               .ToList();
        }
    }

    private HashSet<FileSystemInfoTreeNode> GetBranchesSatisfyingFilter(FileSystemInfoTreeNodeEnumerable nodes)
    {
        var visited = new HashSet<FileSystemInfoTreeNode>();

        nodes
           .Where(x => filter(x.Value))
           .TapEach(_ => cancellationToken.ThrowIfCancellationRequested())
           .ForEach(MarkAncestors);

        return visited;

        void MarkAncestors(FileSystemInfoTreeNode? node)
        {
            while (node is not null && !visited.Contains(node))
            {
                cancellationToken.ThrowIfCancellationRequested();
                visited.Add(node);
                node = node.Parent;
            }
        }
    }

    public Func<FileSystemInfo, bool> GetBfsFilter()
    {
        return within
            ? filter
            : _ => true;
    }
}
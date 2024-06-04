using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PowerShellStandardModule1.Lib.Extensions;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class WithinHandler(PrintTreeService printTreeService)
{
    public void ProcessResult(IList<FileSystemInfoTreeNode> result)
    {
        if (!printTreeService.Within) return;

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
           .Where(x => printTreeService.Filter(x.Value))
           .TapEach(_ => printTreeService.Token.ThrowIfCancellationRequested())
           .ForEach(MarkAncestors);

        return visited;

        void MarkAncestors(FileSystemInfoTreeNode? node)
        {
            while (node is not null && !visited.Contains(node))
            {
                printTreeService.Token.ThrowIfCancellationRequested();
                visited.Add(node);
                node = node.Parent;
            }
        }
    }

    public Func<FileSystemInfo, bool> GetBfsFilter()
    {
        return printTreeService.Within
            ? printTreeService.Filter
            : _ => true;
    }
}
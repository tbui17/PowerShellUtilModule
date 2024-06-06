using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PowerShellStandardModule1.Lib.Extensions;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class RemoveBranchesNotSatisfyingFilterImpl(Func<FileSystemInfo, bool> filter, CancellationToken cancellationToken)
{
    public void Invoke(IList<FileSystemInfoTreeNode> result)
    {

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


}
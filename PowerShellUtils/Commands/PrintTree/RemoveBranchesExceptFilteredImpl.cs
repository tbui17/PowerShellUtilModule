using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PowerShellStandardModule1.Lib.Extensions;


namespace PowerShellStandardModule1.Commands.PrintTree;

public class RemoveBranchesExceptFilteredImpl(
    Func<FileSystemInfo, bool> filter,
    CancellationToken cancellationToken)
{
    public void Invoke(IList<FileSystemInfoTreeNode> result)
    {
        var branches = GetBranchesSatisfyingFilter(result);
        foreach (var node in result)
        {
            node.Children = node
               .Children
               .Where(branches.Contains)
               .TapEach(_ => cancellationToken.ThrowIfCancellationRequested())
               .ToList();
        }
    }

    private HashSet<FileSystemInfoTreeNode> GetBranchesSatisfyingFilter(FileSystemInfoTreeNodeEnumerable nodes)
    {
        var visited = new HashSet<FileSystemInfoTreeNode>();

        nodes
           .Where(x => filter(x.Value))
           .TapEach(_ => cancellationToken.ThrowIfCancellationRequested())
           .ForEach(MarkAncestorsAndNode);

        return visited;

        void MarkAncestorsAndNode(FileSystemInfoTreeNode? node)
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

public class PreserveTerminalNodeChildrenImpl(
    Func<FileSystemInfo, bool> filter,
    CancellationToken cancellationToken,
    int parallelThreshold = int.MaxValue)
{
    private bool ShouldInvokeParallel(int count) => count >= parallelThreshold;

    public void Invoke(IList<FileSystemInfoTreeNode> result)
    {
        var (dependencyNodes, terminalNodes) = GetDependencyAndTerminalNodes(result);


        if (ShouldInvokeParallel(result.Count))
        {
            dependencyNodes
               .AsParallel()
               .ForAll(KeepDependencyAndTerminalNodes);
        }
        else
        {
            dependencyNodes.ForEach(KeepDependencyAndTerminalNodes);
        }


        return;

        void KeepDependencyAndTerminalNodes(FileSystemInfoTreeNode node)
        {
            node.Children = node
               .Children.Where(IsInEitherSet)
               .TapEach(_ => cancellationToken.ThrowIfCancellationRequested())
               .ToList();
        }

        bool IsInEitherSet(FileSystemInfoTreeNode node) =>
            dependencyNodes.Contains(node) || terminalNodes.Contains(node);
    }

    private (HashSet<FileSystemInfoTreeNode> dependencyNodes, HashSet<FileSystemInfoTreeNode> terminalNodes)
        GetDependencyAndTerminalNodes(IList<FileSystemInfoTreeNode> nodes)
    {
        var dependencyNodes = new HashSet<FileSystemInfoTreeNode>();
        var terminalNodes = new HashSet<FileSystemInfoTreeNode>();

        if (ShouldInvokeParallel(nodes.Count))
        {
            // synchronize before foreach block
            foreach (var node in nodes
                        .AsParallel()
                        .Where(x => filter(x.Value)))
            {
                DoMarkingOperations(node);
            }
        }
        else
        {
            nodes
               .Where(x => filter(x.Value))
               .ForEach(DoMarkingOperations);
        }

        return (dependencyNodes, terminalNodes);

        void MarkAncestors(FileSystemInfoTreeNode node)
        {
            var parent = node.Parent;
            while (parent is not null && !dependencyNodes.Contains(parent))
            {
                dependencyNodes.Add(parent);
                parent = parent.Parent;
            }
        }

        void DoMarkingOperations(FileSystemInfoTreeNode node)
        {
            cancellationToken.ThrowIfCancellationRequested();
            MarkAncestors(node);
            terminalNodes.Add(node);
        }
    }
}
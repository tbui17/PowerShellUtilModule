using System;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class WidthFilterCreator(int nodeWidth, int rootNodeWidth, int width)
{

    public Func<FileSystemInfoTreeNode, bool> CreateWidthIsWithinLimitsFilter() =>
        width < 0
            ? StandardNodeLimiter
            : MixedNodeLimiter;

    private bool StandardNodeLimiter(FileSystemInfoTreeNode node) => node.Index < nodeWidth;
    private bool RootNodeLimiter(FileSystemInfoTreeNode node) => node.Index < rootNodeWidth;

    private bool MixedNodeLimiter(FileSystemInfoTreeNode node) =>
        node.Height <= 1
            ? RootNodeLimiter(node)
            : StandardNodeLimiter(node);

    
}
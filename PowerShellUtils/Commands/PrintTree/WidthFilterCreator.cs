using System;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class WidthFilterCreator(int nodeWidth, int rootNodeWidth)
{
    public int RootNodeWidthLimit { get; init; } = 0;
    public int NodeHeightLimit { get; init; } = 1;

    public Func<FileSystemInfoTreeNode, bool> CreateWidthIsWithinLimitsFilter() =>
        rootNodeWidth < RootNodeWidthLimit
            ? StandardNodeLimiter
            : MixedNodeLimiter;

    private bool StandardNodeLimiter(FileSystemInfoTreeNode node) => node.Index < nodeWidth;
    private bool RootNodeLimiter(FileSystemInfoTreeNode node) => node.Index < rootNodeWidth;

    private bool MixedNodeLimiter(FileSystemInfoTreeNode node) =>
        node.Height <= NodeHeightLimit
            ? RootNodeLimiter(node)
            : StandardNodeLimiter(node);
}
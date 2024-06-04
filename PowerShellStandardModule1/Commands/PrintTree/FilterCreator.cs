using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PowerShellStandardModule1.Lib.Extensions;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class FilterCreator(PrintTreeService printTreeService)
{
    private PrintTreeService PrintTreeService { get; init; } = printTreeService;

    private Func<FileSystemInfoTreeNode, bool> CreateWidthIsWithinLimitsFunction() =>
        PrintTreeService.RootNodeWidth < 0
            ? StandardNodeLimiter
            : MixedNodeLimiter;

    private bool StandardNodeLimiter(FileSystemInfoTreeNode node) => node.Index < PrintTreeService.NodeWidth;
    private bool RootNodeLimiter(FileSystemInfoTreeNode node) => node.Index < PrintTreeService.RootNodeWidth;

    private bool MixedNodeLimiter(FileSystemInfoTreeNode node) =>
        node.Height <= 1
            ? RootNodeLimiter(node)
            : StandardNodeLimiter(node);

    public Func<FileSystemInfoTreeNode, bool> CreateShouldContinueFilter()
    {
        Func<FileSystemInfo, bool> fileFilter = PrintTreeService.File
            ? _ => true
            : x => x is not FileInfo;
        List<Func<FileSystemInfo, bool>> filters = [fileFilter, PrintTreeService.WithinHandler.GetBfsFilter()];

        Func<FileSystemInfo, bool> allFilter = filters.AggregateAll();
        Func<FileSystemInfoTreeNode, bool> adaptedFilter = allFilter.Compose((FileSystemInfoTreeNode x) => x.Value);
        
        
        Func<FileSystemInfoTreeNode, bool> widthIsWithinLimits = CreateWidthIsWithinLimitsFunction();

        Func<FileSystemInfoTreeNode, bool>[] nodeFilters = [adaptedFilter, widthIsWithinLimits];

        var aggregateFilter = nodeFilters.AggregateAll();
        return aggregateFilter;
    }

    public DirectoryTreeNodeEnumerableProcessor CreateOrderer()
    {
        var orderer = PrintTreeService.NodeOrderers.GetValueOrDefault(PrintTreeService.OrderBy, PrintTreeService.DefaultNodeOrderer);

        return PrintTreeService.Descending
            ? orderer.AndThen(x => x.Reverse())
            : orderer;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PowerShellStandardModule1.Lib;
using PowerShellStandardModule1.Lib.Extensions;

namespace PowerShellStandardModule1.Commands.PrintTree;

public partial class PrintTreeService
{
    public required DirectoryInfo StartingDirectory { get; init; }
    public StringValueSelector StringValueSelector { get; init; } = DefaultStringValueSelector;
    public int Height { get; init; } = 3;
    public int Width { get; init; } = int.MaxValue;
    public int NodeWidth { get; init; } = int.MaxValue;
    public int Limit { get; init; } = int.MaxValue;
    public CancellationToken Token { get; init; } = CancellationToken.None;
    public int RootNodeWidth { get; init; } = -1;

    public string OrderBy { get; init; } = "Name";

    public bool Descending { get; init; }

    public Func<FileSystemInfo, bool> Filter { get; init; } = _ => true;

    public bool Within { get; init; }

    public bool File { get; set; }


    private Func<FileSystemInfo, IEnumerable<FileSystemInfo>> ChildProvider { get; init; } = FsUtil.GetChildren;

    private PrintNodeImpl? PrintNodeImpl { get; set; }
    private BfsImplFs? BfsImpl { get; set; }

    private WithinHandler? WithinHandler { get; set; }
    private WidthFilterCreator? FilterCreator { get; set; }

    private FileFilterCreator? FileFilterCreator { get; set; }

    public void Init()
    {
        WithinHandler = new WithinHandler(
            within: Within, filter: Filter,
            cancellationToken: Token
        );

        FilterCreator = new WidthFilterCreator(
            nodeWidth: NodeWidth, rootNodeWidth: RootNodeWidth,
            width: RootNodeWidth
        );

        FileFilterCreator = new FileFilterCreator(shouldFilterFiles: File);

        PrintNodeImpl = new PrintNodeImpl(
            orderer: CreateOrderer(),
            width: Width,
            stringValueSelector: StringValueSelector,
            token: Token
        );
        
        BfsImpl = new BfsImplFs(
            shouldContinueFilter: CreateShouldContinueFilter(
                withinHandler: WithinHandler, widthFilterCreator: FilterCreator, fileFilterCreator: FileFilterCreator
            ),
            childProvider: ChildProvider,
            startingDirectory: StartingDirectory, cancellationToken: Token, height: Height,
            limit: Limit
        );
    }

    private static Func<FileSystemInfoTreeNode, bool> CreateShouldContinueFilter(
        WithinHandler withinHandler,
        WidthFilterCreator widthFilterCreator,
        FileFilterCreator fileFilterCreator
    )
    {
        Func<FileSystemInfoTreeNode, bool>[] nodeFilters =
            [CreateAdaptedFilter(), widthFilterCreator.CreateWidthIsWithinLimitsFilter()];

        Func<FileSystemInfoTreeNode, bool> aggregateFilter = nodeFilters.AggregateAll();

        return aggregateFilter;

        Func<FileSystemInfoTreeNode, bool> CreateAdaptedFilter()
        {
            List<Func<FileSystemInfo, bool>> filters = [fileFilterCreator.CreateFilter(), withinHandler.GetBfsFilter()];

            Func<FileSystemInfo, bool> allFilter = filters.AggregateAll();

            Func<FileSystemInfoTreeNode, bool> adaptedFilter = allFilter.Compose((FileSystemInfoTreeNode x) => x.Value);
            return adaptedFilter;
        }
    }

    private FileSystemInfoTreeNodeEnumerableProcessor CreateOrderer()
    {
        FileSystemInfoTreeNodeEnumerableProcessor orderer = NodeOrderers.GetValueOrDefault(OrderBy, DefaultNodeOrderer);

        return Descending
            ? orderer.AndThen(x => x.Reverse())
            : orderer;
    }

    public IReadOnlyList<FileSystemInfoTreeNode> CreateTreeNodes()
    {
        // build up tree meeting most requirements

        if (!StartingDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {StartingDirectory}");
        }

        if (BfsImpl is null || WithinHandler is null)
        {
            throw new InvalidOperationException("Service not initialized");
        }


        return BfsImpl
           .Invoke()
           .ToList()
           .Tap(WithinHandler.ProcessResult);
    }


    public FileSystemInfoPrintNodeEnumerable CreatePrintNodes()
    {
        if (PrintNodeImpl is null)
        {
            throw new InvalidOperationException("Service not initialized");
        }

        return CreateTreeNodes()
           .Take(1)
           .SelectMany(PrintNodeImpl.CreatePrintNodes);
    }


    public int PredictMaxPrintNodeWidth()
    {
        var parameters = new[] { RootNodeWidth, NodeWidth, Width };

        var num = RootNodeWidth <= -1
            ? Width
            : parameters.Max();

        return Math.Max(1, num);
    }

    public int PredictMaxItemCount()
    {
        var parameters = new[]
        {
            Height,
            NodeWidth,
            RootNodeWidth,
            Width,
            Limit
        };
        return Math.Max(0, parameters.Min());
    }


    public string Invoke() =>
        CreatePrintNodes()
           .ToTreeString();
}

public partial class PrintTreeService
{
    public static string DefaultStringValueSelector(FileSystemInfoTreeNode node) => node.Value.Name;

    private static readonly Dictionary<string, FileSystemInfoTreeNodeEnumerableProcessor> NodeOrderers =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Name"] = DefaultNodeOrderer,
            ["CreationTime"] = x => x.OrderBy(n => n.Value.CreationTime),
            ["LastAccessTime"] = x => x.OrderBy(n => n.Value.LastAccessTime),
            ["LastWriteTime"] = x => x.OrderBy(n => n.Value.LastWriteTime),
            ["Extension"] = x => x.OrderBy(n => n.Value.Extension),
            ["Attributes"] = x => x.OrderBy(n => n.Value.Attributes),
            ["Exists"] = x => x.OrderBy(n => n.Value.Exists)
        };

    public static DirectoryTreeNodeEnumerable DefaultNodeOrderer(DirectoryTreeNodeEnumerable node) => node;
}
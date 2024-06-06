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
    public DirectoryInfo StartingDirectory { get; }
    public StringValueSelector StringValueSelector { get; }
    public int Height { get; }
    public int Width { get; }
    public int NodeWidth { get; }
    public int Limit { get; }
    public CancellationToken Token { get; }
    public int RootNodeWidth { get; }
    public string OrderBy { get; }
    public bool Descending { get; }
    public Func<FileSystemInfo, bool> Filter { get; }
    public bool Within { get; }
    public bool File { get; }
    public int ParallelThreshold { get; init; } = Environment.ProcessorCount * 100;
}

public partial class PrintTreeService
{
    private Func<FileSystemInfo, IEnumerable<FileSystemInfo>> ChildProvider =>
        File
            ? FsUtil.GetChildren
            : DirectoryUtil.GetChildren;


    private BfsExecutor<FileSystemInfo> BfsImpl { get; set; }

    private PrintNodeImpl PrintNodeImpl { get; set; }

    private RemoveBranchesNotSatisfyingFilterImpl RemoveBranchesNotSatisfyingFilterImpl { get; set; }

    private ClearChildrenExceedingWidthImpl ClearChildrenExceedingWidthImpl { get; set; }

    public PrintTreeService(
        DirectoryInfo startingDirectory,
        StringValueSelector? stringValueSelector = null,
        int height = 3,
        int width = int.MaxValue,
        int nodeWidth = int.MaxValue,
        int limit = int.MaxValue,
        int rootNodeWidth = -1,
        string orderBy = "Name",
        bool descending = false,
        Func<FileSystemInfo, bool>? filter = null,
        bool within = false,
        bool file = false,
        CancellationToken token = default
    )
    {
        StartingDirectory = startingDirectory;
        StringValueSelector = stringValueSelector ?? DefaultStringValueSelector;
        Height = height;
        Width = width;
        NodeWidth = nodeWidth;
        Limit = limit;
        Token = token;
        RootNodeWidth = rootNodeWidth < 0
            ? nodeWidth
            : rootNodeWidth;
        OrderBy = orderBy;
        Descending = descending;
        Filter = filter ?? (_ => true);
        Within = within;
        File = file;
        RemoveBranchesNotSatisfyingFilterImpl = new RemoveBranchesNotSatisfyingFilterImpl(
            filter: Filter,
            cancellationToken: Token
        );

        PrintNodeImpl = new PrintNodeImpl(
            orderer: CreateOrderer(),
            width: Width,
            stringValueSelector: StringValueSelector,
            token: Token
        );

        ClearChildrenExceedingWidthImpl = new ClearChildrenExceedingWidthImpl(
            nodeWidth: NodeWidth,
            rootNodeWidth: RootNodeWidth
        );

        BfsImpl = CreateBfsImpl();
        return;

        FileSystemInfoTreeNodeEnumerableProcessor CreateOrderer()
        {
            FileSystemInfoTreeNodeEnumerableProcessor orderer = NodeOrderers.GetValueOrDefault(
                OrderBy, DefaultNodeOrderer
            );

            return Descending
                ? orderer.AndThen(x => x.Reverse())
                : orderer;
        }

        BfsExecutor<FileSystemInfo> CreateBfsImpl()
        {
            return new BfsExecutor<FileSystemInfo>
            {
                ChildProvider = ChildProvider,
                ShouldBreak = node =>
                {
                    Token.ThrowIfCancellationRequested();
                    return node.Height > Height || node.Count > Limit;
                },
                Where = Within
                    ? _ => true
                    : node => Filter(node.Value)
            };
        }
    }


    public IReadOnlyList<FileSystemInfoTreeNode> CreateTreeNodes()
    {
        if (!StartingDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {StartingDirectory}");
        }

        if (RootNodeWidth == 0 || Limit <= 0 || Height <= 0)
        {
            return [];
        }

        var result = BfsImpl
           .Invoke(StartingDirectory)
           .ToList();

        if (Within)
        {
            RemoveBranchesNotSatisfyingFilterImpl.Invoke(result);
        }

        ClearChildrenExceedingWidthImpl.Invoke(result, ParallelThreshold);

        return result;
    }


    public FileSystemInfoPrintNodeEnumerable CreatePrintNodes() =>
        CreateTreeNodes()
           .Take(1)
           .SelectMany(PrintNodeImpl.CreatePrintNodes);


    public int PredictMaxPrintNodeWidth()
    {
        var parameters = new[] { RootNodeWidth, NodeWidth, Width };

        var num = RootNodeWidth <= -1
            ? Math.Max(NodeWidth, Width)
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
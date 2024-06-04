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
    public required DirectoryInfo StartingDirectory { get; set; }
    public StringValueSelector StringValueSelector { get; set; } = DefaultStringValueSelector;
    public int Height { get; set; } = 3;
    public int Width { get; set; } = int.MaxValue;
    public int NodeWidth { get; set; } = int.MaxValue;
    public int Limit { get; set; } = int.MaxValue;
    public CancellationToken Token { get; set; } = CancellationToken.None;
    public int RootNodeWidth { get; set; } = -1;

    public string OrderBy { get; set; } = "Name";

    public bool Descending { get; set; }

    public Func<FileSystemInfo, bool> Filter { get; set; } = _ => true;

    public bool Within { get; set; }

    public bool File { get; set; }

    public WithinHandler WithinHandler { get; init; }
    public FilterCreator FilterCreator { get; init; }

    public Func<FileSystemInfo, IEnumerable<FileSystemInfo>> ChildProvider { get; init; } = FsUtil.GetChildren;
    
    public PrintTreeImpl TreeImpl { get; }
    public BfsImpl Impl { get; }

    public PrintTreeService()
    {
        WithinHandler = new WithinHandler(this);
        FilterCreator = new FilterCreator(this);
        TreeImpl = new PrintTreeImpl(this);
        Impl = new BfsImpl(this);
    }

    public IReadOnlyList<FileSystemInfoTreeNode> CreateTreeNodes()
    {
        // build up tree meeting most requirements

        if (!StartingDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {StartingDirectory}");
        }


        return Impl
           .Bfs()
           .ToList()
           .Tap(WithinHandler.ProcessResult);
    }


    public FileSystemInfoPrintNodeEnumerable CreatePrintNodes() =>
        CreateTreeNodes()
           .Take(1)
           .SelectMany(TreeImpl.CreatePrintNodes);


    public int PredictMaxPrintNodeWidth()
    {
        var parameters = new[] { RootNodeWidth, NodeWidth, Width };

        return RootNodeWidth <= -1
            ? Width
            : parameters.Max();
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

    public static readonly Dictionary<string, DirectoryTreeNodeEnumerableProcessor> NodeOrderers =
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
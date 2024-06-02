using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using PowerShellStandardModule1.Delegates;
using PowerShellStandardModule1.Lib;
using PowerShellStandardModule1.Lib.Extensions;
using PowerShellStandardModule1.Models;

namespace PowerShellStandardModule1.Commands.PrintTree;

using NodeOrderer = Func<IEnumerable<TreeNode<DirectoryInfo>>, IEnumerable<TreeNode<DirectoryInfo>>>;

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

    public bool Descending { get; set; } = false;

    public Func<DirectoryInfo, bool> Filter { get; set; } = _ => true;

    private static readonly Func<DirectoryInfo, IEnumerable<DirectoryInfo>> BaseGetter =
        ChildGetterFactory.CreateDirectoryChildGetter();

    private NodeOrderer CreateOrderer()
    {
        if (!NodeOrderers.TryGetValue(OrderBy, out var orderer))
        {
            return DefaultNodeOrderer;
        }

        return Descending
            ? orderer.Compose(x => x.Reverse())
            : orderer;
    }


    private Func<AbstractNode<T>, IEnumerable<T>> CreateGetter<T>(Func<T, IEnumerable<T>> baseGetter)
    {
        return RootNodeWidth < 0
            ? NodeGetter
            : MixedGetter;

        IEnumerable<T> NodeGetter(AbstractNode<T> node) =>
            baseGetter(node.Value)
               .Take(NodeWidth);

        IEnumerable<T> RootGetter(AbstractNode<T> node) =>
            baseGetter(node.Value)
               .Take(RootNodeWidth);

        IEnumerable<T> MixedGetter(AbstractNode<T> node) =>
            node.Height == 0
                ? RootGetter(node)
                : NodeGetter(node);
    }

    public IImmutableList<TreeNode<DirectoryInfo>> CreateTreeNodes()
    {
        if (!StartingDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {StartingDirectory}");
        }

        return Traversal
           .BfsDetailed(StartingDirectory, CreateGetter(BaseGetter))
           .TakeWhile(_ => !Token.IsCancellationRequested)
           .TakeWhile(x => x.Height < Height)
           .Where(x => Filter(x.Value))
           .Take(Limit)
           .ToImmutableList(); // must materialize to populate children
    }


    public IEnumerable<PrintNode<DirectoryInfo>> CreatePrintNodes() =>
        CreateTreeNodes()
           .Take(1)
           .SelectMany(CreatePrintNodes);

    public IEnumerable<PrintNode<DirectoryInfo>> CreatePrintNodes(TreeNode<DirectoryInfo> treeNode)
    {
        var root = CreateRootNode(treeNode);

        return root
           .ToPreOrderPrintNodes()
           .TakeWhile(_ => !Token.IsCancellationRequested)
            // flattened list represents lines of output, truncate excess lines
           .Take(Width);
    }

    private PrintNode<DirectoryInfo> CreateRootNode(TreeNode<DirectoryInfo> treeNode)
    {
        var orderer = CreateOrderer();
        var root = treeNode.ToPrintNode() with
        {
            StringValueSelector = StringValueSelector.Invoke,
            ChildProvider = x => x.Value.Children.Pipe(orderer)
        };
        return root;
    }


    public string Invoke() =>
        CreatePrintNodes()
           .ToTreeString();
}

public partial class PrintTreeService
{
    public static string DefaultStringValueSelector(TreeNode<DirectoryInfo> node) => node.Value.Name;

    public static Dictionary<string, NodeOrderer> NodeOrderers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Name"] = DefaultNodeOrderer,
        ["CreationTime"] = x => x.OrderBy(n => n.Value.CreationTime),
        ["LastAccessTime"] = x => x.OrderBy(n => n.Value.LastAccessTime),
        ["LastWriteTime"] = x => x.OrderBy(n => n.Value.LastWriteTime),
        ["Extension"] = x => x.OrderBy(n => n.Value.Extension),
        ["Attributes"] = x => x.OrderBy(n => n.Value.Attributes),
        ["Exists"] = x => x.OrderBy(n => n.Value.Exists),
        ["Root"] = x => x.OrderBy(n => n.Value.Root)
    };

    public static IEnumerable<TreeNode<DirectoryInfo>> DefaultNodeOrderer(IEnumerable<TreeNode<DirectoryInfo>> node) =>
        node;
}
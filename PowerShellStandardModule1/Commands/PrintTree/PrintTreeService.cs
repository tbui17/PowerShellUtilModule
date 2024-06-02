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

    public bool Descending { get; set; }

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


    private Func<AbstractNode<T>, IEnumerable<T>> AddNodeWidthConstraint<T>(Func<T, IEnumerable<T>> baseGetter)
    {
        // if is root, take * RootNodeWidth else take * NodeWidth

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

    private Func<AbstractNode<DirectoryInfo>, IEnumerable<DirectoryInfo>> CreateGetter()
    {
        var filteredGetter = BaseGetter.Compose(x => x.Where(Filter));

        // width limit should only count for nodes that pass the filter, so it must come after filter operation
        var filteredNodeWidthConstrainedGetter = AddNodeWidthConstraint(filteredGetter);

        return filteredNodeWidthConstrainedGetter;
    }

    public IImmutableList<TreeNode<DirectoryInfo>> CreateTreeNodes()
    {
        // build up tree meeting most requirements

        if (!StartingDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {StartingDirectory}");
        }


        return Traversal
           .BfsDetailed(StartingDirectory, CreateGetter())
           .TakeWhile(_ => !Token.IsCancellationRequested)
           .Take(Limit)
           .TakeWhile(x => x.Height < Height)
           .ToImmutableList(); // must materialize to populate children
    }


    public IEnumerable<PrintNode<DirectoryInfo>> CreatePrintNodes() =>
        CreateTreeNodes()
           .Take(1)
           .SelectMany(CreatePrintNodes);

    public IEnumerable<PrintNode<DirectoryInfo>> CreatePrintNodes(TreeNode<DirectoryInfo> treeNode)
    {
        // tag existing tree using pre order traversal to produce padding/branch data for printing tree

        // if user stops during bfs, do not begin traversal
        if (Token.IsCancellationRequested) return [];
        var root = CreateRootNode(treeNode);

        return root
           .ToPreOrderPrintNodes()
           .TakeWhile(_ => !Token.IsCancellationRequested)
           .Take(
                Width
            ); // flattened sequence represents lines of output, trim excess lines. the output should be trimmed down based on preorder rather than breadth-first ordering
    }

    private PrintNode<DirectoryInfo> CreateRootNode(TreeNode<DirectoryInfo> treeNode)
    {
        // inject projection and children ordering logic

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
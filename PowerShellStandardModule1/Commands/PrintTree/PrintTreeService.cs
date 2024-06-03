﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using PowerShellStandardModule1.Lib;
using PowerShellStandardModule1.Lib.Extensions;
using PowerShellStandardModule1.Models;

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

    public Func<DirectoryInfo, bool> Filter { get; set; } = _ => true;

    public bool Within;

    private static readonly Func<DirectoryInfo, IEnumerable<DirectoryInfo>> BaseGetter =
        ChildGetterFactory.CreateDirectoryChildGetter();

    private DirectoryTreeNodeEnumerableProcessor CreateOrderer()
    {
        if (!NodeOrderers.TryGetValue(OrderBy, out var orderer))
        {
            return DefaultNodeOrderer;
        }

        return Descending
            ? orderer.AndThen(x => x.Reverse())
            : orderer;
    }


    private Func<AbstractNode<T>, IEnumerable<T>> AddNodeWidthConstraint<T>(Func<T, IEnumerable<T>> baseGetter)
    {
        // if is root, take * RootNodeWidth else take * NodeWidth unless NodeWidth is negative then all are treated as regular nodes

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
        var filteredGetter = Within ? BaseGetter : BaseGetter.AndThen(x => x.Where(Filter));

        // width limit should only count for nodes that pass the filter, so it must come after filter operation
        var filteredNodeWidthConstrainedGetter = AddNodeWidthConstraint(filteredGetter);

        return filteredNodeWidthConstrainedGetter;
    }

    public IImmutableList<DirectoryTreeNode> CreateTreeNodes()
    {
        // build up tree meeting most requirements

        if (!StartingDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {StartingDirectory}");
        }


        var result = Traversal
           .BfsDetailed(StartingDirectory, CreateGetter())
           .TakeWhile(_ => !Token.IsCancellationRequested)
           .Take(Limit)
           .TakeWhile(x => x.Height < Height)
           .ToImmutableList(); // must materialize to populate children

        return result;
    }


    public IEnumerable<DirectoryPrintNode> CreatePrintNodes() =>
        CreateTreeNodes()
           .Take(1)
           .SelectMany(CreatePrintNodes);

    public IEnumerable<DirectoryPrintNode> CreatePrintNodes(DirectoryTreeNode treeNode)
    {
        // tag existing tree using pre order traversal to produce padding/branch data for printing tree and produce pre order sequence

        // if user stops during bfs, do not begin traversal
        if (Token.IsCancellationRequested) return [];
        var root = CreateRootNode(treeNode);

        return root
           .ToPreOrderPrintNodes()
           .TakeWhile(_ => !Token.IsCancellationRequested)
            // flattened sequence represents lines of output, trim excess lines. the output should be trimmed down based on preorder rather than breadth-first ordering
           .Take(Width); 
    }

    public static HashSet<DirectoryTreeNode> GetBranchesSatisfyingFilter(
        IImmutableList<DirectoryTreeNode> nodes,
        Func<DirectoryTreeNode, bool> predicate
    )
    {
        var visited = new HashSet<DirectoryTreeNode>();


        foreach (var node in nodes)
        {
            if (!predicate(node)) continue;
            MarkAncestors(node);
        }

        return visited;

        void MarkAncestors(DirectoryTreeNode? node)
        {
            while (node is not null && !visited.Add(node)) node = node.Parent;
        }
    }

    private DirectoryPrintNode CreateRootNode(DirectoryTreeNode treeNode)
    {
        // inject projection and children ordering logic


        var orderer = CreateOrderer();
        var root = treeNode.ToPrintNode();
        root.StringValueSelector = StringValueSelector;


        root.ChildProvider = x =>
        {
            var children = x.Value.Children;
            return orderer(children);
        };

        return root;
    }


    public string Invoke() =>
        CreatePrintNodes()
           .ToTreeString();
}

public partial class PrintTreeService
{
    public static string DefaultStringValueSelector(DirectoryTreeNode node) => node.Value.Name;

    public static readonly Dictionary<string, DirectoryTreeNodeEnumerableProcessor> NodeOrderers =
        new(StringComparer.OrdinalIgnoreCase)
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

    public static IEnumerable<DirectoryTreeNode> DefaultNodeOrderer(IEnumerable<DirectoryTreeNode> node) => node;
}

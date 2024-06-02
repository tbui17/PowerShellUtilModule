using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using PowerShellStandardModule1.Delegates;
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

    private static readonly Func<DirectoryInfo, IEnumerable<DirectoryInfo>> BaseGetter =
        ChildGetterFactory.CreateDirectoryChildGetter();


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
        var root = treeNode.ToPrintNode() with
        {
            StringValueSelector = x => StringValueSelector(x),
            ChildProvider = PrintNode.DefaultChildProvider
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
}
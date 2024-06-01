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
    public static Dictionary<string, StringValueSelector> DirectoryPropertyStringSelectors = new()
    {
        ["Name"] = DefaultStringValueSelector,
        ["FullName"] = x => x.Value.FullName,
        ["Extension"] = x => x.Value.Extension,
        ["CreationTime"] = x => x.Value.CreationTime.ToString(CultureInfo.InvariantCulture),
        ["LastAccessTime"] = x => x.Value.LastAccessTime.ToString(CultureInfo.InvariantCulture),
        ["LastWriteTime"] = x => x.Value.LastWriteTime.ToString(CultureInfo.InvariantCulture),
        ["Attributes"] = x => x.Value.Attributes.ToString(),
        ["Exists"] = x => x.Value.Exists.ToString(),
        ["Root"] = x => x.Value.Root.ToString(),
        ["Parent"] = x => x.Value.Parent?.FullName ?? "",
        ["ParentName"] = x => x.Value.Parent?.Name ?? "",
        ["ParentFullName"] = x => x.Value.Parent?.FullName ?? "",
        ["ParentExtension"] = x => x.Value.Parent?.Extension ?? "",
        ["ParentCreationTime"] = x => x.Value.Parent?.CreationTime.ToString(CultureInfo.InvariantCulture) ?? "",
        ["ParentLastAccessTime"] = x => x.Value.Parent?.LastAccessTime.ToString(CultureInfo.InvariantCulture) ?? "",
        ["ParentLastWriteTime"] = x => x.Value.Parent?.LastWriteTime.ToString(CultureInfo.InvariantCulture) ?? "",
        ["ParentAttributes"] = x => x.Value.Parent?.Attributes.ToString() ?? "",
        ["ParentExists"] = x => x.Value.Parent?.Exists.ToString() ?? "",
        ["ParentRoot"] = x => x.Value.Parent?.Root.ToString() ?? "",
    };
    public static string DefaultStringValueSelector(TreeNode<DirectoryInfo> node) => node.Value.Name;
}
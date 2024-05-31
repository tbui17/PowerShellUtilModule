using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using PowerShellStandardModule1.Lib;
using PowerShellStandardModule1.Structs;
using static PowerShellStandardModule1.Lib.Extensions;


namespace PowerShellStandardModule1.Commands;

public class PrintTreeRunner
{
    public required DirectoryInfo TargetDirectory;
    public int Height = 3;
    public int Width = int.MaxValue;
    public int NodeWidth = int.MaxValue;
    public int Take = int.MaxValue;
    public CancellationToken Token = CancellationToken.None;
    public int RootNodeWidth = -1;

    private static readonly Func<DirectoryInfo, IEnumerable<DirectoryInfo>> BaseGetter =
        ChildGetterFactory.CreateDirectoryChildGetter();

    private Func<TreeNode<DirectoryInfo>, IEnumerable<DirectoryInfo>> CreateGetter()
    {
        return RootNodeWidth < 0
            ? ConstrainedGetter
            : ConstrainedGetterWithRootConstraint;

        IEnumerable<DirectoryInfo> ConstrainedGetter(TreeNode<DirectoryInfo> node) =>
            BaseGetter(node.Value).Take(NodeWidth);

        IEnumerable<DirectoryInfo> ConstrainedGetterWithRootConstraint(TreeNode<DirectoryInfo> node) =>
            node.Height == 0
                ? BaseGetter(node.Value).Take(RootNodeWidth)
                : ConstrainedGetter(node);
    }

    public IImmutableList<TreeNode<DirectoryInfo>> CreateTreeNodes()
    {
        if (!TargetDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {TargetDirectory}");
        }

        return BfsDetailed(TargetDirectory, CreateGetter())
           .TakeWhile(_ => !Token.IsCancellationRequested)
           .TakeWhile(x => x.Height < Height)
           .Take(Take)
           .ToImmutableList(); // must materialize to populate children
    }


    public IEnumerable<PrintNode<DirectoryInfo>> CreatePrintNodes() =>
        CreateTreeNodes()
           .Take(1)
           .SelectMany(CreatePrintNodes);

    public IEnumerable<PrintNode<DirectoryInfo>> CreatePrintNodes(TreeNode<DirectoryInfo> treeNode) =>
        treeNode
           .ToPreOrderPrintNodes()
           .TakeWhile(_ => !Token.IsCancellationRequested)
            // flattened list represents lines of output, truncate excess lines
           .Take(Width)
            // modify formatting of all involved nodes
           .Select(DefaultPrintNodeSelector);

    public string Invoke() => CreatePrintNodes().ToTreeString();


    private static PrintNode<DirectoryInfo> DefaultPrintNodeSelector(PrintNode<DirectoryInfo> outerNode) =>
        outerNode with { StringValueSelector = node => node.Value.Name };
}
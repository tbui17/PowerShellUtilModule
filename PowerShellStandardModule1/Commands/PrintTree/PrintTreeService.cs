using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PowerShellStandardModule1.Lib;
using PowerShellStandardModule1.Lib.Extensions;
using PowerShellStandardModule1.Models;

namespace PowerShellStandardModule1.Commands.PrintTree;

delegate FileSystemInfoTreeNodeEnumerable NodeWidthFilter(FileSystemInfoTreeNodeEnumerable node);

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

    public bool Within;

    public bool File = false;

    private DirectoryTreeNodeEnumerableProcessor CreateOrderer()
    {
        var orderer = NodeOrderers.GetValueOrDefault(OrderBy, DefaultNodeOrderer);

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


        return Bfs2()
           .ToList()
           .Tap(ProcessResult);
    }

    private Func<FileSystemInfoTreeNode, bool> CreateWidthIsWithinLimitsFunction() =>
        RootNodeWidth < 0
            ? StandardNodeLimiter
            : MixedNodeLimiter;

    private bool StandardNodeLimiter(FileSystemInfoTreeNode node) => node.Index < NodeWidth;
    private bool RootNodeLimiter(FileSystemInfoTreeNode node) => node.Index < RootNodeWidth;

    private bool MixedNodeLimiter(FileSystemInfoTreeNode node) =>
        node.Height <= 1
            ? RootNodeLimiter(node)
            : StandardNodeLimiter(node);


    private FileSystemInfoTreeNodeEnumerable Bfs2()
    {
        var widthIsWithinLimits = CreateWidthIsWithinLimitsFunction();
        var filter = widthIsWithinLimits.CopySignature(x => Filter(x.Value));
        var shouldContinue = new[] { widthIsWithinLimits, filter }
           .AggregateAll();
        var bfs = new BfsExecutor<FileSystemInfo>
        {
            Filter = shouldContinue,
            ItemProvider = fsItem => fsItem is DirectoryInfo dir
                ? FsUtil.GetDirectoryFileSystemInfoChildren(dir)
                : [],
            ShouldBreak = node =>
            {
                Token.ThrowIfCancellationRequested();
                return node.Height <= Height && node.Count <= Limit;
            }
        };
        return bfs.Invoke(StartingDirectory);
    }


    void ProcessResult(IList<FileSystemInfoTreeNode> result)
    {
        if (!Within) return;

        var branches = GetBranchesSatisfyingFilter(result);
        foreach (var node in result)
        {
            node.Children = node
               .Children
               .Where(branches.Contains)
               .ToList();
        }
    }


    public FileSystemInfoPrintNodeEnumerable CreatePrintNodes() =>
        CreateTreeNodes()
           .Take(1)
           .SelectMany(CreatePrintNodes);

    public FileSystemInfoPrintNodeEnumerable CreatePrintNodes(FileSystemInfoTreeNode treeNode)
    {
        // tag existing tree using pre order traversal to produce padding/branch data for printing tree and produce pre order sequence

        // if user stops during bfs, do not begin traversal
        Token.ThrowIfCancellationRequested();
        var root = CreateRootNode(treeNode);

        return root
           .ToPreOrderPrintNodes()
           .TapEach(_ => Token.ThrowIfCancellationRequested())
            // flattened sequence represents lines of output, trim excess lines. the output should be trimmed down based on preorder rather than breadth-first ordering
           .Take(Width);
    }

    private HashSet<FileSystemInfoTreeNode> GetBranchesSatisfyingFilter(FileSystemInfoTreeNodeEnumerable nodes)
    {
        var visited = new HashSet<FileSystemInfoTreeNode>();

        nodes
           .Where(x => Filter(x.Value))
           .TapEach(_ => Token.ThrowIfCancellationRequested())
           .ForEach(MarkAncestors);

        return visited;

        void MarkAncestors(FileSystemInfoTreeNode? node)
        {
            while (node is not null && !visited.Contains(node))
            {
                Token.ThrowIfCancellationRequested();
                visited.Add(node);
                node = node.Parent;
            }
        }
    }


    private FileSystemInfoPrintNode CreateRootNode(FileSystemInfoTreeNode treeNode)
    {
        // inject projection and children ordering logic


        var orderer = CreateOrderer();
        var root = treeNode.ToPrintNode();
        root.StringValueSelector = StringValueSelector;
        root.ChildProvider = x => x.Value.Children.Thru(orderer);


        return root;
    }

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

public class BfsExecutor<T>
{
    public Func<TreeNode<T>, bool> Filter { get; set; } = _ => true;
    public Func<TreeNode<T>, bool> ShouldBreak { get; set; } = _ => true;
    public Func<T, IEnumerable<T>> ItemProvider { get; set; } = _ => [];
    public int Count { get; private set; }
    private Queue<TreeNode<T>> Queue { get; } = new();

    public IEnumerable<TreeNode<T>> Invoke(T root)
    {
        Queue.Clear();
        Queue.Enqueue(new TreeNode<T> { Value = root });

        while (Queue.Count > 0)
        {
            var next = Queue.Dequeue();
            yield return next;


            var newChildHeight = next.Height + 1;


            var count = Count;
            var nodes = ItemProvider(next.Value)
               .Select(
                    (x, i) => new TreeNode<T>
                    {
                        Value = x,
                        Height = newChildHeight,
                        Index = i,
                        Parent = next,
                        Count = count + i + 1
                    }
                );

            foreach (var node in nodes)
            {
                Count++;
                if (ShouldBreak(node)) yield break;
                if (!Filter(node)) continue;

                node.Parent?.Children.Add(node);
                Queue.Enqueue(node);
            }
        }
    }
}
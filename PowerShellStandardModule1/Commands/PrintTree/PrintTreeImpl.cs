using System.Linq;
using PowerShellStandardModule1.Lib.Extensions;
using PowerShellStandardModule1.Models;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class PrintTreeImpl(PrintTreeService printTreeService)
{
    public FileSystemInfoPrintNodeEnumerable CreatePrintNodes(FileSystemInfoTreeNode treeNode)
    {
        // tag existing tree using pre order traversal to produce padding/branch data for printing tree and produce pre order sequence

        // if user stops during bfs, do not begin traversal
        printTreeService.Token.ThrowIfCancellationRequested();
        var root = CreateRootNode(treeNode);

        return root
           .ToPreOrderPrintNodes()
           .TapEach(_ => printTreeService.Token.ThrowIfCancellationRequested())
            // flattened sequence represents lines of output, trim excess lines. the output should be trimmed down based on preorder rather than breadth-first ordering
           .Take(printTreeService.Width);
    }

    private FileSystemInfoPrintNode CreateRootNode(FileSystemInfoTreeNode treeNode)
    {
        // inject projection and children ordering logic


        var orderer = printTreeService.FilterCreator.CreateOrderer();
        var root = treeNode.ToPrintNode();
        root.StringValueSelector = printTreeService.StringValueSelector;
        root.ChildProvider = x => x.Value.Children.Thru(orderer);


        return root;
    }
}
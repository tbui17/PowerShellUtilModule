using System.Linq;
using System.Threading;
using PowerShellStandardModule1.Lib.Extensions;
using PowerShellStandardModule1.Models;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class PrintNodeImpl(
    FileSystemInfoTreeNodeEnumerableProcessor orderer,
    int width,
    StringValueSelector stringValueSelector,
    CancellationToken token)
{
    
    

    public FileSystemInfoPrintNodeEnumerable CreatePrintNodes(FileSystemInfoTreeNode treeNode)
    {
        // tag existing tree using pre order traversal to produce padding/branch data for printing tree and produce pre order sequence

        // if user stops during bfs, do not begin traversal
        token.ThrowIfCancellationRequested();
        var root = CreateRootNode(treeNode);

        return root
           .ToPreOrderPrintNodes()
           .TapEach(_ => token.ThrowIfCancellationRequested())
            // flattened sequence represents lines of output, trim excess lines. the output should be trimmed down based on preorder rather than breadth-first ordering
           .Take(width);
    }

    private FileSystemInfoPrintNode CreateRootNode(FileSystemInfoTreeNode treeNode)
    {
        // inject projection and children ordering logic
        var root = treeNode.ToPrintNode();
        root.StringValueSelector = stringValueSelector;
        root.ChildProvider = x => x.Value.Children.Thru(orderer);


        return root;
    }
}
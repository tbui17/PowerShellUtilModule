using System.Collections.Generic;
using System.Linq;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class ClearChildrenExceedingWidthImpl(
    int nodeWidth,
    int rootNodeWidth,
    int parallelThreshold)
{
    private void ClearChildren(FileSystemInfoTreeNode node)
    {
        if (node.Height == 0)
        {
            node.Children = node
               .Children.Take(rootNodeWidth)
               .ToList();
            return;
        }

        node.Children = node
           .Children.Take(nodeWidth)
           .ToList();
    }

    public void Invoke(ICollection<FileSystemInfoTreeNode> result)
    {
        if (result.Count >= parallelThreshold)
        {
            result
               .AsParallel()
               .ForAll(ClearChildren);
        }
        else
        {
            foreach (var node in result) ClearChildren(node);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PowerShellStandardModule1.Structs;
using static PowerShellStandardModule1.Extensions;

namespace PowerShellStandardModule1;

public static class PrintTreeExtensions
{
    public static string ToTreeString<T>(this TreeNode<T> node) =>
        node
           .ToPreOrderPrintNodes()
           .ToTreeString();

    public static string ToTreeString<T>(this IEnumerable<PrintNode<T>> nodes) =>
        nodes
           .Select(x => x.Line)
           .Aggregate(new StringBuilder(), (sb, x) => sb.AppendLine(x))
           .ToString();

    public static IEnumerable<PrintNode<T>> ToPreOrderPrintNodes<T>(this TreeNode<T> node)
    {
        var root = node.ToPrintNode() with { IsRoot = true };

        // if children are already in order, flip children to process top items first since stack is LIFO
        return Dfs(root, x => x.ReversedChildren);
    }
}
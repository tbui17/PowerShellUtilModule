using System.Collections.Generic;
using System.Linq;
using System.Text;
using PowerShellStandardModule1.Lib.Extensions;
using PowerShellStandardModule1.Models;
using static PowerShellStandardModule1.Lib.Extensions.Extensions;

namespace PowerShellStandardModule1.Commands.PrintTree;

public static class PrintTreeExtensions
{

    public static string ToTreeString<T>(this IEnumerable<PrintNode<T>> nodes) =>
        nodes
           .Select(x => x.Line)
           .ToStringBuilder()
           .ToString();

    public static IEnumerable<PrintNode<T>> ToPreOrderPrintNodes<T>(this TreeNode<T> root) =>
        root.ToPrintNode().ToPreOrderPrintNodes();

    public static IEnumerable<PrintNode<T>> ToPreOrderPrintNodes<T>(this PrintNode<T> root) =>
        Traversal.Dfs(root, x => x.Children);
}
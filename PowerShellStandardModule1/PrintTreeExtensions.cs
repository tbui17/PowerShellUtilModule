using System.Collections.Generic;
using System.Linq;
using System.Text;
using PowerShellStandardModule1.Structs;

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
        var stack = Stack.From([node.ToPrintNode() with { IsRoot = true }]);
        while (stack.NotEmpty())
        {
            var current = stack.Pop();
            yield return current;

            // if children are already in order, flip to process top items first since stack is LIFO

            ReversedChildren(current).ForEach(stack.Push);
        }
    }

    public static IEnumerable<PrintNode<T>> ReversedChildren<T>(PrintNode<T> node) =>
        node
            .Value
            .Children
            .Reverse()
            .Select((x, i) => x.ToPrintNode() with { Index = i, Indent = node.NextIndent });
}
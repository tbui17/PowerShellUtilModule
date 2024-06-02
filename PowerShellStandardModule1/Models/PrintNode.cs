using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PowerShellStandardModule1.Lib.Extensions;

namespace PowerShellStandardModule1.Models;

public enum Indents
{
    None,
    Middle,
    Last,
    Padding,
    PaddedBranch,
}

public static class IndentExtensions
{
    public static string Value(this Indents indent) =>
        indent switch
        {
            Indents.None => "",
            Indents.Middle => "├── ",
            Indents.Last => "└── ",
            Indents.Padding => "    ",
            Indents.PaddedBranch => "│   ",
            _ => throw new ArgumentOutOfRangeException(nameof(indent), indent, null)
        };

    public static Indents Parse(string value) =>
        value switch
        {
            "" => Indents.None,
            "├── " => Indents.Middle,
            "└── " => Indents.Last,
            "    " => Indents.Padding,
            "│   " => Indents.PaddedBranch,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };

    public static string ToValueString(this IEnumerable<Indents> indents) =>
        indents
           .Select(x => x.Value())
           .StringJoin("");
}

public record PrintNode<T>
{
    public required TreeNode<T> Value;
    public int Index;

    public PrintNode<T>? Parent;
    public bool IsRoot => Parent == null;
    private IImmutableList<Indents> Indent { get; set; } = [];


    public Func<TreeNode<T>, string> StringValueSelector = PrintNode.DefaultStringValueSelector;


    private static Func<PrintNode<T>, TreeNode<T>, int, PrintNode<T>> ChildConstructor => DefaultChildConstructor;


    public Func<PrintNode<T>, IEnumerable<TreeNode<T>>> ChildProvider = PrintNode.DefaultChildProvider;

    public string StringValue => StringValueSelector(Value);

    // if iterating over children in reverse order, last child is now index 0
    private bool IsLast => Index == 0;

    private Indents Prefix =>
        IsLast
            ? Indents.Last
            : Indents.Middle;

    public string Line =>
        IsRoot
            ? StringValue
            : CompiledIndent.ToValueString() + StringValue;

    public IImmutableList<Indents> CompiledIndent =>
        IsRoot
            ? []
            : Indent.Add(Prefix);


    private Indents PaddingBranch =>
        IsRoot
            ? Indents.None
            : IsLast
                ? Indents.Padding
                : Indents.PaddedBranch;


    // if it's the last child of its parent, then do not have line in padding
    private IImmutableList<Indents> NextIndent => Indent.Add(PaddingBranch);

    public IEnumerable<PrintNode<T>> Children =>
        Value
           .Children
           .Reverse()
           .Select(CreateChild);

    private PrintNode<T> CreateChild(TreeNode<T> node, int index)
    {
        // override private properties
        var child = ChildConstructor(this, node, index);
        child.Indent = NextIndent;
        return child;
    }

    private static PrintNode<T> DefaultChildConstructor(PrintNode<T> parent, TreeNode<T> node, int index) =>
        parent with
        {
            Value = node,
            Index = index,
            Parent = parent
        };
}

public static class PrintNode
{
    public static PrintNode<T> ToPrintNode<T>(this TreeNode<T> node) => From(node);

    public static PrintNode<T> From<T>(TreeNode<T> node) => new() { Value = node };

    public static string DefaultStringValueSelector<T>(AbstractNode<T> node) =>
        node.Value?.ToString() ?? Indents.None.Value();

    public static IEnumerable<TreeNode<T>> DefaultChildProvider<T>(PrintNode<T> node) => node.Value.Children;
}
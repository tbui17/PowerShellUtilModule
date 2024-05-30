using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PowerShellStandardModule1.Structs;

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

    public static string ToValueString(this IImmutableList<Indents> indents) =>
        indents
           .Select(x => x.Value())
           .StringJoin("");
}

public record PrintNode<T>
{
    public required TreeNode<T> Value;
    public int Index;
    public bool IsRoot;
    private IImmutableList<Indents> _indent = [];
    public Func<TreeNode<T>, string> StringValueSelector = DefaultStringValueSelector;

    public string StringValue => StringValueSelector(Value);

    private bool IsFirst => Index == 0;

    private Indents Prefix =>
        IsFirst
            ? Indents.Last
            : Indents.Middle;

    public string Line =>
        IsRoot
            ? StringValue
            : CompiledIndent.ToValueString() + StringValue;

    public IImmutableList<Indents> CompiledIndent =>
        IsRoot
            ? []
            : _indent.Add(Prefix);


    private Indents PaddingBranch =>
        IsRoot
            ? Indents.None
            : IsFirst
                ? Indents.Padding
                : Indents.PaddedBranch;

    private IImmutableList<Indents> NextIndent => _indent.Add(PaddingBranch);

    public IEnumerable<PrintNode<T>> ReversedChildren =>
        Value
           .Children
           .Reverse()
           .Select((x, i) => x.ToPrintNode() with { Index = i, _indent = NextIndent });

    private static string DefaultStringValueSelector(AbstractNode<T> node) =>
        node.Value?.ToString() ?? Indents.None.Value();
}

public static class PrintNode
{
    public static PrintNode<T> ToPrintNode<T>(this TreeNode<T> node) => From(node);

    public static PrintNode<T> From<T>(TreeNode<T> node) => new() { Value = node };
}
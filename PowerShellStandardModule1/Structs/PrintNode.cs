using System;
using System.Collections.Immutable;

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
    public static string Value(this Indents indent) => indent switch
    {
        Indents.None => "",
        Indents.Middle => "├── ",
        Indents.Last => "└── ",
        Indents.Padding => "    ",
        Indents.PaddedBranch => "│   ",
        _ => throw new ArgumentOutOfRangeException(nameof(indent), indent, null)
    };
    
    public static Indents Parse(string value) => value switch
    {
        "" => Indents.None,
        "├── " => Indents.Middle,
        "└── " => Indents.Last,
        "    " => Indents.Padding,
        "│   " => Indents.PaddedBranch,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };
}

public class Indent
{
    public required Indents Value { get; init; }
    
    public static implicit operator Indent(string value) => IndentExtensions.Parse(value);
    public static implicit operator Indent(Indents value) => new(){Value=value}; 
    
    public static ImmutableList<Indents> operator +(Indent indent, Indents value) => ImmutableList.Create(indent.Value, value);
}

public record PrintNode<T>
{
    public required TreeNode<T> Value;
    public int Index;
    public bool IsRoot;
    public string Indent = Indents.None.Value();
    public Func<TreeNode<T>, string> StringValueSelector = DefaultStringValueSelector;
    
    public string StringValue => StringValueSelector(Value);

    private bool IsFirst => Index == 0;

    private string Prefix =>
        IsFirst
            ? Indents.Last.Value()
            : Indents.Middle.Value();

    public string Line =>
        IsRoot
            ? StringValue
            : Indent + Prefix + StringValue;


    private string PaddingBranch =>
        IsRoot
            ? ""
            : IsFirst
                ? Indents.Padding.Value()
                : Indents.PaddedBranch.Value();

    public string NextIndent => Indent + PaddingBranch;
    
    private static string DefaultStringValueSelector(AbstractNode<T> node) => node.Value?.ToString() ?? "";
    
}

public static class PrintNode
{
    public static PrintNode<T> ToPrintNode<T>(this TreeNode<T> node) => From(node);

    public static PrintNode<T> From<T>(TreeNode<T> node) => new() { Value = node };
}
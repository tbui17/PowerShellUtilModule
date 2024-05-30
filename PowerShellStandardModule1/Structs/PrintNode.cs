using System;

namespace PowerShellStandardModule1.Structs;

public record PrintNode<T>
{
    public required TreeNode<T> Value;
    public int Index;
    public bool IsRoot;
    public string Indent = "";
    public Func<TreeNode<T>, string> StringValueSelector = DefaultStringValueSelector;
    
    public string StringValue => StringValueSelector(Value);

    public bool IsFirst => Index == 0;

    public string Branch =>
        IsFirst
            ? "└── "
            : "├── ";

    public string Line =>
        IsRoot
            ? StringValue
            : Indent + Branch + StringValue;


    public string PaddingBranch =>
        IsRoot
            ? ""
            : IsFirst
                ? "    "
                : "│   ";

    public string NextIndent => Indent + PaddingBranch;
    
    private static string DefaultStringValueSelector(AbstractNode<T> node) => node.Value?.ToString() ?? "";
    
}

public static class PrintNode
{
    public static PrintNode<T> ToPrintNode<T>(this TreeNode<T> node) => From(node);

    public static PrintNode<T> From<T>(TreeNode<T> node) => new() { Value = node };
}
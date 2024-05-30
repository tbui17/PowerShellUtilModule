using System.Collections.Generic;

namespace PowerShellStandardModule1.Structs;

public record TreeNode<T> : AbstractNode<T>
{
    public TreeNode<T>? Parent;
    public IEnumerable<TreeNode<T>> Children = [];
};
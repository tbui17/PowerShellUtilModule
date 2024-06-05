using System.Collections.Generic;


// ReSharper disable CollectionNeverQueried.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace PowerShellStandardModule1.Models;

public class TreeNode<T> : AbstractNode<T>
{
    public TreeNode<T>? Parent { get; set; }
    public IList<TreeNode<T>> Children { get; set; } = [];

    public int Count { get; set; }

    public TreeNode<T> Clone()
    {
        return (TreeNode<T>)MemberwiseClone();
    }

    
};

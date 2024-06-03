﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

// ReSharper disable CollectionNeverQueried.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace PowerShellStandardModule1.Models;

public class TreeNode<T> : AbstractNode<T>
{
    public TreeNode<T>? Parent { get; set; }
    public IList<TreeNode<T>> Children { get; set; } = [];

    public TreeNode<T> Clone()
    {
        return (TreeNode<T>)MemberwiseClone();
    }
};

public static class TreeNode
{
    public static TreeNode<TValue> ToSerializable<T, TValue>(this TreeNode<T> node, Func<T, TValue> selector) =>
        new()
        {
            Value = selector(node.Value),
            Height = node.Height,
            Index = node.Index,
            Children = node
               .Children.Select(x => x.ToSerializable(selector))
               .ToList()
        };

    public static TreeNode<T> ToSerializable<T>(this TreeNode<T> node)
    {
        var clone = node.Clone();
        clone.Parent = null;
        clone.Children = node
           .Children.Select(x => x.ToSerializable())
           .ToList();
        return clone;
    }


    public static TreeNode<T> RestoreParents<T>(this TreeNode<T> node)
    {
        foreach (var child in node.Children)
        {
            child.Parent = node;
            child.RestoreParents();
        }

        return node;
    }

    public static TreeNode<T>? JsonParse<T>(string json) =>
        JsonConvert
           .DeserializeObject<TreeNode<T>>(json)
          ?.RestoreParents();
}
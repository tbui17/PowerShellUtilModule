// ReSharper disable RedundantUsingDirective.Global
global using NUnit.Framework;
global using static TestProject1.Container;
global using DirectoryTreeNode = PowerShellStandardModule1.Models.TreeNode<System.IO.DirectoryInfo>;
global using DirectoryPrintNode = PowerShellStandardModule1.Models.PrintNode<System.IO.DirectoryInfo>;
global using StringValueSelector = System.Func<PowerShellStandardModule1.Models.TreeNode<System.IO.DirectoryInfo>,string>;
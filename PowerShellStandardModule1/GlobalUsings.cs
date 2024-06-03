global using DirectoryTreeNode = PowerShellStandardModule1.Models.TreeNode<System.IO.DirectoryInfo>;
global using DirectoryPrintNode = PowerShellStandardModule1.Models.PrintNode<System.IO.DirectoryInfo>;
global using StringValueSelector = System.Func<PowerShellStandardModule1.Models.TreeNode<System.IO.DirectoryInfo>,string>;
global using DirectoryTreeNodeEnumerable = System.Collections.Generic.IEnumerable<PowerShellStandardModule1.Models.TreeNode<System.IO.DirectoryInfo>>;
global using DirectoryPrintNodeEnumerable = System.Collections.Generic.IEnumerable<PowerShellStandardModule1.Models.PrintNode<System.IO.DirectoryInfo>>;
global using DirectoryTreeNodeEnumerableProcessor = System.Func<System.Collections.Generic.IEnumerable<PowerShellStandardModule1.Models.TreeNode<System.IO.DirectoryInfo>>, System.Collections.Generic.IEnumerable<PowerShellStandardModule1.Models.TreeNode<System.IO.DirectoryInfo>>>;
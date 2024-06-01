using System.IO;
using PowerShellStandardModule1.Models;

namespace PowerShellStandardModule1.Delegates;

public delegate string StringValueSelector(TreeNode<DirectoryInfo> node);

public static class Examp
{

    public static StringValueSelector GetFullName = n => n.Value.FullName;
}
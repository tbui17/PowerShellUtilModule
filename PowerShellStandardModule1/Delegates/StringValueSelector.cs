using System.IO;
using System.Management.Automation;
using PowerShellStandardModule1.Lib;
using PowerShellStandardModule1.Models;

namespace PowerShellStandardModule1.Delegates;

public delegate string StringValueSelector(TreeNode<DirectoryInfo> node);

public static class StringValueSelectorFactory
{
    public static StringValueSelector ToStringValueSelector(this ScriptBlock stringSelector)
    {
        return StringSelector;

        string StringSelector(TreeNode<DirectoryInfo> directory)
        {
            var res = stringSelector.Invoke(directory.Value);
            return PsUtil.SerializePsResult(res);
        }
    }
}
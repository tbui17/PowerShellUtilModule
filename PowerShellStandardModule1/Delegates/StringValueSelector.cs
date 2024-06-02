using System.IO;
using System.Management.Automation;
using PowerShellStandardModule1.Lib;
using PowerShellStandardModule1.Models;

namespace PowerShellStandardModule1.Delegates;

public delegate string StringValueSelector(DirectoryTreeNode node);

public static class StringValueSelectorFactory
{
    public static StringValueSelector ToStringValueSelector(this ScriptBlock stringSelector)
    {
        return StringSelector;

        string StringSelector(DirectoryTreeNode node) =>
            stringSelector
               .InvokeWithValue(node.Value)
               .SerializePsResult();
    }
}
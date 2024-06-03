using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace PowerShellStandardModule1.Lib;

public static class PsUtil
{
    public static string SerializePsResult(this IReadOnlyCollection<PSObject> psResult) => psResult.FirstOrDefault()?.ToString() ?? "";


    public static Collection<PSObject> InvokeWithValue(this ScriptBlock block, object? value)
    {
        var variable = new PSVariable("_", value);
        return block.InvokeWithContext(new(), [variable]);
    }

    public static PSObject GetFirst(this Collection<PSObject> src) => src.First();

    public static T GetFirst<T>(this Collection<PSObject> src) =>
        GetFirst(src)
           .BaseObject is T t
            ? t
            : throw new PSInvalidOperationException(
                $"The scriptblock did not return the expected type. Expected: {typeof(T).Name} Received : ${src}"
            );
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace PowerShellStandardModule1.Lib;

public static class PsUtil
{
    public static string SerializePsResult(this IReadOnlyCollection<PSObject> psResult) =>
        ValidatePsResult(psResult)
           .ToString();

    public static PSObject ValidatePsResult(this IReadOnlyCollection<PSObject> psResult)
    {
        if (psResult.Count is 0)
        {
            var ex = new PSInvalidOperationException(
                $"The scriptblock did not return a result. It should return at least return a string. {psResult}"
            );
            throw ex;
        }

        var first = psResult.First();
        if (first is null)
        {
            var ex = new PSInvalidOperationException($"The scriptblock returned a null value. {psResult}");
            throw ex;
        }

        return first;
    }


    public static Collection<PSObject> InvokeWithValue(this ScriptBlock block, object? value)
    {
        var variable = new PSVariable("_", value);
        return block.InvokeWithContext(new(), [variable]);
    }

    public static PSObject ValidateGetFirst(this Collection<PSObject> src) => ValidatePsResult(src);

    public static T ValidateGetFirst<T>(this Collection<PSObject> src) =>
        ValidateGetFirst(src)
           .BaseObject is T t
            ? t
            : throw new PSInvalidOperationException(
                $"The scriptblock did not return the expected type. Expected: {typeof(T).Name} Received : ${src}"
            );
}
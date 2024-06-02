using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PowerShellStandardModule1.Lib;

public static class PsUtil
{
    public static string SerializePsResult(IReadOnlyCollection<PSObject> psResult)
    {
        if (psResult.Count is 0)
        {
            var ex = new PSInvalidOperationException(
                "The scriptblock did not return a result. It should return at least return a string."
            );
            throw ex;
        }

        var first = psResult.First();
        if (first is null)
        {
            var ex = new PSInvalidOperationException(
                "The scriptblock was malformed and did not instantiate. An example of a proper script block is '{param($directory) $directory.Name}'."
            );
            throw ex;
        }

        return first.ToString();
    }
}
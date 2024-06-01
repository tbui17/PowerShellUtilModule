using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Threading;
using PowerShellStandardModule1.Lib;
using PowerShellStandardModule1.Lib.Extensions;

namespace PowerShellStandardModule1.Commands.Bfs;

using DirectoryChildGetter = Func<DirectoryInfo, IEnumerable<DirectoryInfo>>;

public class BfsController(
    string pattern,
    string startingDirectory,
    bool ignoreCase = true,
    DirectoryChildGetter? directoryChildGetter = null,
    int itemsToReturn = 1,
    int limit = int.MaxValue
)
{
    public readonly int ItemsToReturn = Math.Max(0, itemsToReturn);

    public readonly int Limit = Math.Max(0, limit);

    private readonly DirectoryChildGetter _childGetter = directoryChildGetter ??
                                                         ChildGetterFactory.CreateDirectoryChildGetter(
                                                             ChildGetterFactory.DefaultEnumerationOptions
                                                         );

    private readonly DirectoryInfo _startingDirectory = new(startingDirectory);


    public IEnumerable<DirectoryInfo> Invoke(CancellationToken? token = null)
    {
        var cancelToken = token ?? CancellationToken.None;
        Validate();
        
        return Traversal.Bfs(_startingDirectory,_childGetter)
            .Take(Limit)
            .TakeWhile(_ => !cancelToken.IsCancellationRequested)
            .Where(x => IsMatch(x.Name))
            .Take(ItemsToReturn);
    }

    private void Validate()
    {
        if (!_startingDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {_startingDirectory}");
        }
    }

    private bool IsMatch(string name) => FileSystemName.MatchesSimpleExpression(pattern, name, ignoreCase);
}
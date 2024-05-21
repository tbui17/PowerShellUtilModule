using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Threading;

namespace PowerShellStandardModule1;

using DirectoryChildGetter = Func<DirectoryInfo, IEnumerable<DirectoryInfo>>;

public class BfsRunner(
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


    public IEnumerable<DirectoryInfo> Run(CancellationToken? token = null)
    {
        Validate();
        return _childGetter
            .Bfs(_startingDirectory)
            .Take(Limit)
            .TakeWhile(_ => token switch
            {
                null => true,
                _ => !token.Value.IsCancellationRequested
            })
            .Where(x => IsMatch(x.Name))
            .Take(ItemsToReturn);
    }

    public void Validate()
    {
        if (!_startingDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {_startingDirectory}");
        }
    }

    public bool IsMatch(string name) => FileSystemName.MatchesSimpleExpression(pattern, name, ignoreCase);
}
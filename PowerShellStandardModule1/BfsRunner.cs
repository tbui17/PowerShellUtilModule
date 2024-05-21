using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;

namespace PowerShellStandardModule1;

using DirectoryChildGetter = Func<DirectoryInfo, IEnumerable<DirectoryInfo>>;

public class BfsRunner(
    string pattern,
    string startingDirectory,
    bool ignoreCase = true,
    DirectoryChildGetter? directoryChildGetter = null,
    int take = 1
)
{
    public static readonly EnumerationOptions DefaultEnumerationOptions = new()
    {
        IgnoreInaccessible = true,
        RecurseSubdirectories = false,
        MatchCasing = MatchCasing.CaseSensitive,
        MatchType = MatchType.Simple,
        ReturnSpecialDirectories = false,
        MaxRecursionDepth = 1,
        AttributesToSkip = FileAttributes.Hidden
    };

    public static DirectoryChildGetter CreateDirectoryChildGetter(EnumerationOptions enumerationOptions) =>
        (directory) => directory
            .EnumerateDirectories(
                "*",
                enumerationOptions
            );

    private readonly DirectoryChildGetter _childGetter = directoryChildGetter ??
                                                        CreateDirectoryChildGetter(DefaultEnumerationOptions);
    
    private readonly DirectoryInfo _startingDirectory = new(startingDirectory);
    
    
    


    public IEnumerable<DirectoryInfo> Run()
    {
        Validate();
        return BfsSequence().Take(take);
    }

    public void Validate()
    {
        if (!_startingDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {_startingDirectory}");
        }
    }

    public IEnumerable<DirectoryInfo> BfsSequence() => _childGetter
        .Bfs(_startingDirectory)
        .Where(x => FileSystemName.MatchesSimpleExpression(pattern, x.Name, ignoreCase));
}
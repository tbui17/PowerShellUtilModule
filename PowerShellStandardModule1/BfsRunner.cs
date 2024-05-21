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

    public readonly string Pattern = pattern;
    public readonly bool IgnoreCase = ignoreCase;

    public readonly DirectoryChildGetter ChildGetter = directoryChildGetter ??
                                                       CreateDirectoryChildGetter(DefaultEnumerationOptions);


    public readonly DirectoryInfo StartingDirectory = new(startingDirectory);
    
    
    


    public IEnumerable<DirectoryInfo> Run()
    {
        Validate();
        return BfsSequence().Take(take);
    }

    public void Validate()
    {
        if (!StartingDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {StartingDirectory}");
        }
    }

    public IEnumerable<DirectoryInfo> BfsSequence() => ChildGetter
        .Bfs(StartingDirectory)
        .Where(x => FileSystemName.MatchesSimpleExpression(Pattern, x.Name, IgnoreCase));
}
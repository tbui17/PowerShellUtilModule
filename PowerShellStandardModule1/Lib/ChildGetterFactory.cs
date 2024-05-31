using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PowerShellStandardModule1.Lib;

public static class ChildGetterFactory
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

    public static Func<DirectoryInfo, IEnumerable<DirectoryInfo>> CreateDirectoryChildGetter(
        EnumerationOptions enumerationOptions,
        string pattern = "*"
    ) => (directory) => 
        GetDirectoryChildren(() => directory.EnumerateDirectories(pattern, enumerationOptions));
    
    public static Func<DirectoryInfo, IEnumerable<DirectoryInfo>> CreateDirectoryChildGetter(
        string pattern = "*"
    ) => CreateDirectoryChildGetter(DefaultEnumerationOptions, pattern);

    // ReSharper disable once ReturnTypeCanBeEnumerable.Local
    /// <summary>
    /// Gets the children of a parent directory, stopping when a directory related error is thrown to allow for partial results and continuation of directory BFS.
    /// In doing so, any directories at the current depth that have not been iterated over before the exception will not be processed.
    /// </summary>
    public static List<DirectoryInfo> GetDirectoryChildren(
        Func<IEnumerable<DirectoryInfo>> directoryGetter
    )
    {
        List<DirectoryInfo> results = [];

        try
        {
            var source = directoryGetter();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var item in source)
            {
                results.Add(item);
            }
        }
        catch (Exception e) when (e is UnauthorizedAccessException or DirectoryNotFoundException)
        {
            Debug.WriteLine(e);
        }

        return results;
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PowerShellStandardModule1.Lib;

public static class DirectoryUtil
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
    ) =>
        (directory) =>
            GetChildren(() => directory.EnumerateDirectories(pattern, enumerationOptions));

    public static Func<DirectoryInfo, IEnumerable<DirectoryInfo>> CreateDirectoryChildGetter(string pattern = "*") =>
        CreateDirectoryChildGetter(DefaultEnumerationOptions, pattern);

    public static List<DirectoryInfo> GetChildren(Func<IEnumerable<DirectoryInfo>> directoryGetter)
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

    public static List<DirectoryInfo> GetChildren(DirectoryInfo directory)
    {
        List<DirectoryInfo> results = [];


        try
        {
            var source = directory.EnumerateDirectories("*", DefaultEnumerationOptions);

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

    public static IEnumerable<FileSystemInfo> GetChildren(FileSystemInfo fsItem) =>
        fsItem switch
        {
            DirectoryInfo dir => GetChildren(dir),
            _ => []
        };
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PowerShellStandardModule1.Lib;

public static class FsUtil
{
    public static IEnumerable<FileSystemInfo> GetChildren(
        DirectoryInfo directory,
        string searchPattern = "*",
        EnumerationOptions? options = null
    )
    {
        options ??= DirectoryUtil.DefaultEnumerationOptions;
        List<FileSystemInfo> results = [];


        try
        {
            var source = directory.EnumerateFileSystemInfos(searchPattern, options);

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

    public static IEnumerable<FileSystemInfo> GetChildren(this FileSystemInfo fsItem)
    {
        return fsItem switch
        {
            DirectoryInfo dir => GetChildren(dir),
            _ => []
        };
    }
    
    
}
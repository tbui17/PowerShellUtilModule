using System;
using System.IO;

namespace PowerShellStandardModule1.Commands.PrintTree;

internal class FileFilterCreator(bool shouldFilterFiles)
{
    public Func<FileSystemInfo, bool> CreateFilter() =>
        shouldFilterFiles
            ? _ => true
            : Filter;

    private static bool Filter(FileSystemInfo info) => info is not FileInfo;
}
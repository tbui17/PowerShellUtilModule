using System;
using System.IO;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class FileFilterCreator(bool shouldFilterFiles)
{
    public Func<FileSystemInfo, bool> CreateFilter() =>
        shouldFilterFiles
            ? _ => true
            : IsNotFileInfo;

    private static bool IsNotFileInfo(FileSystemInfo info) => info is not FileInfo;
}
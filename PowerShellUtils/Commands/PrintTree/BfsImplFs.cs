using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class BfsImplFs(
    Func<FileSystemInfoTreeNode, bool> shouldContinueFilter,
    Func<FileSystemInfo, IEnumerable<FileSystemInfo>> childProvider,
    FileSystemInfo startingDirectory,
    int height,
    int limit,
    CancellationToken cancellationToken)
{
    public FileSystemInfoTreeNodeEnumerable Invoke()
    {
        if (height <= 0 || limit <= 0) return [];
        
        return new BfsExecutor<FileSystemInfo>
        {
            Where = shouldContinueFilter,
            ChildProvider = childProvider,
            ShouldBreak = ShouldBreak
        }.Invoke(startingDirectory);
    }

    private bool ShouldBreak(FileSystemInfoTreeNode node)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return node.Height > height || node.Count > limit;
    }
}
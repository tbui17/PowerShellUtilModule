using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Threading;
using PowerShellStandardModule1.Commands.PrintTree;
using PowerShellStandardModule1.Lib;

namespace PowerShellStandardModule1.Commands.Bfs;

using DirectoryChildGetter = Func<FileSystemInfo, IEnumerable<FileSystemInfo>>;

public class BfsController(
    DirectoryInfo startingDirectory,
    string pattern = "*",
    bool ignoreCase = true,
    int limit = int.MaxValue,
    int take = int.MaxValue,
    int height = int.MaxValue,
    bool file = false)
{
    public readonly int Limit = Math.Max(0, limit);

    public readonly int Height = Math.Max(0, height);

    private readonly DirectoryChildGetter _childGetter =
        file
            ? FsUtil.GetChildren
            : DirectoryUtil.GetChildren;


    public FileSystemInfoTreeNodeEnumerable Invoke(CancellationToken? token = null)
    {
        var cancelToken = token ?? CancellationToken.None;
        Validate();

        var impl = new BfsExecutor<FileSystemInfo>
        {
            ChildProvider = _childGetter,
            ShouldBreak = ShouldBreak
        };

        var res = impl.Invoke(startingDirectory);

        if (pattern != "*")
        {
            res = res.Where(x => IsMatch(x.Value.Name));
        }

        return res.Take(take);


        bool ShouldBreak(FileSystemInfoTreeNode x)
        {
            cancelToken.ThrowIfCancellationRequested();
            return x.Height > Height || x.Count > Limit;
        }
    }


    private void Validate()
    {
        if (!startingDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {startingDirectory}");
        }
    }

    public bool IsMatch(string name) => FileSystemName.MatchesSimpleExpression(pattern, name, ignoreCase);
}
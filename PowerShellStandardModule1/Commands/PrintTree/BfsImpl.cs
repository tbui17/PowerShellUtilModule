using System.IO;

namespace PowerShellStandardModule1.Commands.PrintTree;

public class BfsImpl(PrintTreeService printTreeService)
{
    private PrintTreeService TreeService { get; } = printTreeService;

    public FileSystemInfoTreeNodeEnumerable Bfs() =>
        new BfsExecutor<FileSystemInfo>
        {
            Filter = TreeService.FilterCreator.CreateShouldContinueFilter(),
            ChildProvider = TreeService.ChildProvider,
            ShouldBreak = ShouldBreak
        }.Invoke(TreeService.StartingDirectory);

    private bool ShouldBreak(FileSystemInfoTreeNode node)
    {
        TreeService.Token.ThrowIfCancellationRequested();
        return node.Height <= TreeService.Height && node.Count <= TreeService.Limit;
    }
}
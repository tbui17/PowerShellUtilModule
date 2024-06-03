namespace TestProject1;

public class Utils
{
    public static DirectoryInfo GetSolutionDirectory(string? currentPath = null)
    {
        currentPath ??= Directory.GetCurrentDirectory();

        var directory = new DirectoryInfo(currentPath);

        while (directory is not null && HasNoSolution())
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new InvalidOperationException("Solution directory not found.");
        }

        return directory;

        bool HasNoSolution()
        {
            var files = directory.GetFiles("*.sln");
            return files.Length == 0;
        }
    }
}
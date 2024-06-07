namespace TestProject1;

public class Utils
{

    public const string TestDataPath = "TestProject1/Resources/TestData";
    
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
    
    public static void CreateTestDataFolder()
    {
        var basePath = GetSolutionDirectory().FullName + "/" + TestDataPath;
        CreateTestFolders(basePath);
        Console.WriteLine($"Test folders created under {basePath}");
    }

    static void CreateTestFolders(string basePath)
    {
        // Create a deep folder structure with more folders at each level
        Directory.CreateDirectory(Path.Combine(basePath, "level1/level2/level3/level4/level5/level6/level7"));
        Directory.CreateDirectory(Path.Combine(basePath, "level1/level2/level3/level4/level5/level6b"));
        Directory.CreateDirectory(Path.Combine(basePath, "level1/level2/level3/level4b/level5b"));
        Directory.CreateDirectory(Path.Combine(basePath, "level1/level2/level3b/level4c"));
        Directory.CreateDirectory(Path.Combine(basePath, "level1/level2b/level3c"));
        Directory.CreateDirectory(Path.Combine(basePath, "level1b/level2c/level3d"));
        Directory.CreateDirectory(Path.Combine(basePath, "level1c/level2d"));
        Directory.CreateDirectory(Path.Combine(basePath, "level1d"));

        // Create some dummy files
        for (int i = 1; i <= 3; i++)
        {
            File.WriteAllText(Path.Combine(basePath, $"level1/file{i}.txt"), $"Dummy content for file {i}");
        }

        for (int i = 1; i <= 2; i++)
        {
            File.WriteAllText(Path.Combine(basePath, $"level1/level2/file{i}.txt"), $"Dummy content for file {i}");
        }

        File.WriteAllText(Path.Combine(basePath, "level1/level2/level3/level4/file1.txt"), "Dummy content for file 1");
        File.WriteAllText(Path.Combine(basePath, "level1b/level2c/file1.txt"), "Dummy content for file 1");
        File.WriteAllText(Path.Combine(basePath, "level1c/file1.txt"), "Dummy content for file 1");
    }

    public static DirectoryInfo GetTestDataDirectory()
    {
        var basePath = Utils.GetSolutionDirectory()
           .FullName;

        var testDataPath = basePath + "/TestProject1/Resources/TestData";


        var dir = new DirectoryInfo(testDataPath);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {dir.FullName}");
        }

        return dir;
    }
}
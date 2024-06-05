using System.Diagnostics;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;
using PowerShellStandardModule1.Commands.Bfs;
using PowerShellStandardModule1.Lib;

namespace TestProject1;

public class Tests
{
    private BfsController _controller = null!;
    private DirectoryInfo _currentDirectory = null!;
    

    public const string Pattern = "*s";


    [OneTimeSetUp]
    public void SetupBeforeAll()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        _currentDirectory = Utils.GetSolutionDirectory();
        
    }

    [OneTimeTearDown]
    public void TearDownAfterAll()
    {
        Trace.Flush();
    }

    [SetUp]
    public void Setup()
    {
        _controller = new BfsController(pattern: Pattern, startingDirectory: _currentDirectory);
    }

    [Test]
    public void TestRun()
    {
        var res = _controller
           .Invoke()
           .ToList();
        res
           .Should()
           .NotBeEmpty();
    }


    [Test]
    public void TestGetDirectoryChildren()
    {
        Func<IEnumerable<DirectoryInfo>> shouldCatch = () => throw new DirectoryNotFoundException("Should catch");
        Func<IEnumerable<DirectoryInfo>> shouldNotCatch = () => throw new ApplicationException("Should not catch");

        var shouldCatchCase = void () => DirectoryUtil.GetChildren(shouldCatch);
        var shouldNotCatchCase = void () => DirectoryUtil.GetChildren(shouldNotCatch);

        using var scope = new AssertionScope();

        shouldCatchCase
           .Should()
           .NotThrow();
        shouldNotCatchCase
           .Should()
           .Throw<ApplicationException>();
    }

    [Test]
    public void TestBasic()
    {
        var instance = new BfsController(
            pattern: "*net*", startingDirectory: Utils.GetSolutionDirectory(), ignoreCase: true,
            file: false
        );

        instance
           .IsMatch("*net*")
           .Should()
           .BeTrue();
        
        var res = instance.Invoke();


        res
           .Count()
           .Should()
           .BeGreaterThan(5);


    }
}
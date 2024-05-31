using System.Diagnostics;
using FluentAssertions;
using Newtonsoft.Json;
using PowerShellStandardModule1.Commands;
using PowerShellStandardModule1.Lib;

namespace TestProject1;

using PowerShellStandardModule1;

public class Tests
{
    private BfsRunner _runner = null!;
    private string _currentDirectory = null!;
    private string _initialDirectory = null!;

    public const string Pattern = "*s";


    [OneTimeSetUp]
    public void SetupBeforeAll()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        _initialDirectory = Directory.GetCurrentDirectory();
        _currentDirectory = _initialDirectory
            .Thru(Directory.GetParent)!
            .Thru(x => x.Parent?.Parent?.FullName)!;
    }

    [OneTimeTearDown]
    public void TearDownAfterAll()
    {
        Trace.Flush();
    }

    [SetUp]
    public void Setup()
    {
        _runner = new BfsRunner(Pattern, _currentDirectory);
    }

    [Test]
    public void TestRun()
    {
        var res = _runner.Run().ToList();
        res.Should().NotBeEmpty();
    }


    [Test]
    public void TestGetDirectoryChildren()
    {
        Func<IEnumerable<DirectoryInfo>> shouldCatch = () => throw new DirectoryNotFoundException("Should catch");
        Func<IEnumerable<DirectoryInfo>> shouldNotCatch = () => throw new ApplicationException("Should not catch");

        var shouldCatchCase = void () => ChildGetterFactory.GetDirectoryChildren(shouldCatch);
        var shouldNotCatchCase = void () => ChildGetterFactory.GetDirectoryChildren(shouldNotCatch);

        shouldCatchCase.Should().NotThrow();
        shouldNotCatchCase.Should().Throw<ApplicationException>();
    }

    [Test]
    public void TestCancel()
    {
        var src = new CancellationTokenSource();

        var dirA = new DirectoryInfo("a");
        var dirB = new DirectoryInfo("b");
        int i = 0;

        var time = 2000;
        var timeMinus500 = time - 500;
        var timePlus500 = time + 500;

        Func<DirectoryInfo, IEnumerable<DirectoryInfo>> childGetter = (_) =>
        {
            DirectoryInfo[] res = i <= 5
                ? [dirA]
                : [dirB];

            i++;
            return res;
        };

        var runner = new BfsRunner("a", _currentDirectory, directoryChildGetter: childGetter, itemsToReturn: 100);

       

        var stopwatch = Stopwatch.StartNew();
        var msgObj = new
        {
            name = nameof(TestCancel),
            message = "Started test",
            time,
            timeMinus500,
            timePlus500,
            stopwatch.ElapsedMilliseconds,
        };
        msgObj.Serialize(Formatting.Indented).Log();
        src.CancelAfter(time);

        // ReSharper disable once UnusedVariable
        var res = runner.Run(src.Token).ToList();
        stopwatch.Stop();
        Console.WriteLine($"Finished processing in {stopwatch.ElapsedMilliseconds} ms.");

        res.Should().NotBeEmpty();
        
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(timeMinus500).And.BeLessThan(timePlus500);
    }
}
using System.Collections;
using FluentAssertions;
using FluentAssertions.Execution;
using PowerShellStandardModule1.Commands.PrintTree;
using PowerShellStandardModule1.Lib.Extensions;
using PowerShellStandardModule1.Models;
using static TestProject1.Utils;

namespace TestProject1.PrintTree;

[
    TestFixture(false),
    TestFixture(true)
]
[TestFixture(
     6, 10, 50,
     20, 100
 ),
 TestFixture(
     10, 10, 5,
     30, 100
 ),
 TestFixture(
     1, 1, 1,
     1, 1
 ),
 TestFixture(
     int.MaxValue, int.MaxValue, int.MaxValue,
     int.MaxValue, int.MaxValue
 ),
 TestFixture(
     15288, 31876, 5,
     62336, 5
 ),
 TestFixture(
     29421, 31582, 3,
     31478, 9
 ),
 TestFixture(
     95047, 7093, 19719,
     68563, 77
 ),
 TestFixture(
     64772, 7, 3,
     41, 3
 ),
 TestFixture(
     6, 3, 9,
     69783, 7
 )
]
[TestFixtureSource(typeof(FixtureData), nameof(FixtureData.FixtureParams))]
public class PrintTreeNodeIntegration(
    int height = 6,
    int nodeWidth = 10,
    int width = 50,
    int take = 20,
    int rootNodeWidth = 200)
{
    private PrintTreeService Instance { get; set; } = null!;
    private List<TreeNode<FileSystemInfo>> TreeNodes { get; set; } = null!;
    private List<PrintNode<FileSystemInfo>> PrintNodes { get; set; } = null!;

    private static int AdjustedHeight(int height) => height + 1;


    public bool File { get; set; }

    public CancellationToken Token { get; } = default;

    public bool Descending { get; set; }
    public bool Within { get; set; }


    public PrintTreeNodeIntegration(bool file) : this()
    {
        File = file;
    }


    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Instance = new PrintTreeService
        {
            StartingDirectory = GetSolutionDirectory(),
            Height = height,
            NodeWidth = nodeWidth,
            Width = width,
            Limit = take,
            RootNodeWidth = rootNodeWidth,
            File = File,
            Token = Token,
            Descending = Descending,
            Within = Within,
            StringValueSelector = x => x.Value.Name
        };

        TreeNodes = Instance
           .CreateTreeNodes()
           .ToList();


        PrintNodes = Instance
           .CreatePrintNodes()
           .ToList();


        using var scope = new AssertionScope();

        TreeNodes
           .Should()
           .NotBeEmpty();


        PrintNodes
           .Should()
           .NotBeEmpty();
    }

    [Test]
    public void TestTreeNodesCreation()
    {
        TreeNodes
           .Should()
           .HaveCountGreaterThan(0);
    }

    [Test]
    public void TestTreeNodesCount()
    {
        TreeNodes
           .Should()
           .HaveCountLessThanOrEqualTo(take);
    }

    [Test]
    public void TestTreeNodesHeight()
    {
        TreeNodes
           .Should()
           .AllSatisfy(
                x => x
                   .Height.Should()
                   .BeLessThanOrEqualTo(height)
            );
    }

    [Test]
    public void TestTreeNodesChildrenCount()
    {
        var res = TreeNodes
           .Where(x => x.Height != 0)
           .ToList();

        // passes by vacuous truth
        if (res.IsEmpty())
        {
            return;
        }

        var root = TreeNodes[0];
       
        
        


        res
           .Should()
           .AllSatisfy(
                x => x
                   .Children.Count
                   .Should()
                   .BeLessThanOrEqualTo(Instance.PredictMaxPrintNodeWidth())
            );
    }

    [Test]
    public void TestPrintNodesRootCount()
    {
        PrintNodes
           .Count(x => x.IsRoot)
           .Should()
           .Be(1);
    }

    [Test]
    public void TestPrintNodesWidth()
    {
        PrintNodes
           .Count.Should()
           .BeLessThanOrEqualTo(width);
    }

    [Test]
    public void TestPrintNodesIndexUniqueness()
    {
        PrintNodes
           .GroupBy(x => x.Value.Parent)
           .Should()
           .AllSatisfy(
                group => group
                   .Sliding()
                   .All(pair => pair.Item1.Index != pair.Item2.Index)
                   .Should()
                   .BeTrue()
            );
    }

    [Test]
    public void TestPrintNodesIndentation()
    {
        PrintNodes
           .Should()
           .AllSatisfy(
                x =>
                {
                    if (x.Value.Height == 0)
                    {
                        x
                           .CompiledIndent.Count.Should()
                           .Be(0, "The root is not indented.");
                    }
                    else
                    {
                        x
                           .CompiledIndent.Count.Should()
                           .Be(AdjustedHeight(x.Value.Height));
                    }
                }
            );
    }

    [Test]
    public void TestPrintNodesToTreeString()
    {
        var str = PrintNodes
           .ToTreeString()
           .Trim();

        var lineCount = str
           .Split("\n")
           .Length;
        
        str.Log();


        lineCount
           .Should()
           .BeLessThanOrEqualTo(Instance.PredictMaxPrintNodeWidth());
    }


    [Test]
    public void TestFile() { }
}

public class FixtureData
{
    public static IEnumerable FixtureParams => CreateData();

    private static IEnumerable CreateData()
    {
        var random = new Random();

        var data = 5
           .Times()
           .Select(CreateTestFixtureData);

        return data;

        TestFixtureData CreateTestFixtureData(int _) =>
            new(
                N(), N(), N(),
                N(), N()
            );

        int N() => Next(High());

        int Next(int randomNumberGeneratorSeed) =>
            randomNumberGeneratorSeed % 2 == 0
                ? High()
                : Low();

        int High() => random.Next(0, 100000);

        int Low() => random.Next(1, 10);
    }
}
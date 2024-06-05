using System.Collections;
using AutoFixture;
using FluentAssertions;
using PowerShellStandardModule1.Commands.PrintTree;
using PowerShellStandardModule1.Lib.Extensions;
using PowerShellStandardModule1.Models;
using static TestProject1.Utils;

namespace TestProject1.PrintTree;

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
     0, 0, 0,
     0, 0
 ),
 TestFixture(
     int.MaxValue, int.MaxValue, int.MaxValue,
     int.MaxValue, int.MaxValue
 ),
 TestFixture(
     int.MinValue, int.MinValue, int.MinValue,
     int.MinValue, int.MinValue
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
 ),
 TestFixture(
     10940, 14592, 0,
     -6, 96171
 ),
 TestFixture(
     21570, 52612, -3,
     82888, -9
 )
]
[TestFixtureSource(typeof(FixtureData), nameof(FixtureData.FixtureParams))]
public class PrintTreeNodeIntegration(
    int height = 1000,
    int nodeWidth = 1000,
    int width = 1000,
    int take = 1000,
    int rootNodeWidth = 1000)
{
    protected PrintTreeService Instance { get; set; } = null!;
    protected List<TreeNode<FileSystemInfo>> TreeNodes { get; set; } = null!;
    protected List<PrintNode<FileSystemInfo>> PrintNodes { get; set; } = null!;

    private static int AdjustedHeight(int height) => height + 1;


    protected virtual void InitInstance()
    {
       
        
        Instance = new PrintTreeService(
            startingDirectory: GetSolutionDirectory(),
            height: height,
            nodeWidth: nodeWidth,
            width: width,
            limit: take,
            rootNodeWidth: rootNodeWidth
        );
    }


    [SetUp]
    public void OneTimeSetup()
    {
        InitInstance();

        TreeNodes = Instance
           .CreateTreeNodes()
           .ToList();


        PrintNodes = Instance
           .CreatePrintNodes()
           .ToList();
    }


    [Test]
    public void TestTreeNodesCount()
    {
        TreeNodes
           .Should()
           .HaveCountLessThanOrEqualTo(take.MinZero());
    }

    [Test]
    public void TestTreeNodesHeight()
    {
        if (TreeNodes.IsEmpty())
        {
            return;
        }

        TreeNodes
           .Should()
           .AllSatisfy(
                node => node
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
        if (PrintNodes.IsEmpty())
        {
            return;
        }

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
           .BeLessThanOrEqualTo(width.MinZero());
    }

    [Test]
    public void TestPrintNodesIndexUniqueness()
    {
        if (PrintNodes.IsEmpty())
        {
            return;
        }

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
        if (PrintNodes.IsEmpty())
        {
            return;
        }

        PrintNodes
           .Should()
           .AllSatisfy(
                node =>
                {
                    if (node.Value.Height == 0)
                    {
                        node
                           .CompiledIndent.Count.Should()
                           .Be(0, "The root is not indented.");
                    }
                    else
                    {
                        node
                           .CompiledIndent.Count.Should()
                           .Be(AdjustedHeight(node.Value.Height));
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
}

public class FixtureData
{
    public static IEnumerable FixtureParams => CreateData();

    private static IEnumerable CreateData()
    {
        var random = new Random();

        var data = 10
           .Times()
           .Select(CreateTestFixtureData);

        return data;

        TestFixtureData CreateTestFixtureData(int _) =>
            new(
                N(), N(), N(),
                N(), N()
            );

        int N() => CreateValue();

        int CreateValue()
        {
            var seed = random.Next(0, 100);
            var val = Next(seed);

            return (seed % 2) switch
            {
                0 => val,
                _ => -val
            };
        }

        int Next(int randomNumberGeneratorSeed) =>
            randomNumberGeneratorSeed % 2 == 0
                ? High()
                : Low();

        int High() => random.Next(0, 100000);

        int Low() => random.Next(0, 10);
    }
}
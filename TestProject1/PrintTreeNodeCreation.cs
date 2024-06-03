using FluentAssertions;
using FluentAssertions.Execution;
using PowerShellStandardModule1.Commands.PrintTree;
using PowerShellStandardModule1.Lib.Extensions;
using PowerShellStandardModule1.Models;

namespace TestProject1;

public class PrintTreeNodeCreation
{
    private PrintTreeService instance = null!;

    private static readonly DirectoryInfo Directory = Utils.GetSolutionDirectory();

    [SetUp]
    public void Setup()
    {
        instance = new PrintTreeService { StartingDirectory = Directory };
    }


    [Test]
    public void TestIntegrationTest()
    {
        var height = 6;
        var nodeWidth = 10;
        var width = 50;
        var take = 20;
        var rootNodeWidth = 100;

        instance.Height = height;
        instance.NodeWidth = nodeWidth;
        instance.Width = width;
        instance.Limit = take;
        instance.RootNodeWidth = rootNodeWidth;


        var res = instance.CreateTreeNodes();

        res
           .Should()
           .HaveCountGreaterThan(0);

        var printNodes = instance
           .CreatePrintNodes()
           .ToList();


        using var scope = new AssertionScope();

        res
           .Should()
           .HaveCountLessThanOrEqualTo(take);
        res.ForEach(
            x => x
               .Height.Should()
               .BeLessThanOrEqualTo(height)
        );
        res
           .Where(x => x.Height != 0)
           .ForEach(
                x => x
                   .Children.Count()
                   .Should()
                   .BeLessThanOrEqualTo(nodeWidth)
            );


        printNodes
           .Count(x => x.IsRoot)
           .Should()
           .Be(1);

        printNodes
           .Count.Should()
           .BeLessThanOrEqualTo(width);


        printNodes
           .GroupBy(x => x.Value.Parent)
           .ForEach(
                group =>
                {
                    group
                       .Sliding()
                       .All(pair => pair.Item1.Index != pair.Item2.Index)
                       .Should()
                       .BeTrue();
                }
            );

        printNodes.ForEach(
            x =>
            {
                if (x.Value.Height is 0)
                {
                    x
                       .CompiledIndent.Count.Should()
                       .Be(0, "The root is not indented.");
                }
                else
                {
                    x
                       .CompiledIndent.Count.Should()
                       .Be(x.Value.Height + 1);
                }
            }
        );

        printNodes
           .ToTreeString()
           .Log();
    }
}
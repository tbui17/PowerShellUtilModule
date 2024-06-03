using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;
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
    public void BaseTest()
    {
        var height = 100;
        var nodeWidth = 100;
        var width = 999;
        var take = 1000;
        var rootNodeWidth = 100;

        instance.Height = height;
        instance.NodeWidth = nodeWidth;
        instance.Width = width;
        instance.Limit = take;
        instance.RootNodeWidth = rootNodeWidth;


        var immutableListOfBfsNodes = instance.CreateTreeNodes();
        // var coreNodes = PrintTreeService.GetBranchesSatisfyingFilter(
        //     immutableListOfBfsNodes, x => x.Value.Name.Contains('b')
        // );

        var visited = new HashSet<DirectoryTreeNode>();

        var predicate =
            new Predicate<DirectoryTreeNode>(x => x.Value.Name.Contains('b', StringComparison.OrdinalIgnoreCase));
        var nodes = immutableListOfBfsNodes;
        var s = @"C:\Users\PCS\RiderProjects\PowerShellStandardModule1";

        foreach (var node in nodes)
        {
            if (!predicate(node)) continue;
            MarkAncestors(node);
        }


        foreach (var node in nodes)
        {
            node.Children = node
               .Children.Where(x => visited.Contains(x))
               .ToList();
        }

        var r = nodes[0]
           .ToPrintNode();
        r.StringValueSelector = x => x.Value.Name;

        r
           .ToPreOrderPrintNodes()
           .ToTreeString()
           .Log();


        void MarkAncestors(DirectoryTreeNode? node)
        {
            while (node is not null && !visited.Contains(node))
            {
                visited.Add(node);
                node = node.Parent;
            }
        }

        //
        //
        //
        // coreNodes
        //    .Select(x => x.Value.Name)
        //    .Should()
        //    .Contain("PowerShellStandardModule1");
        //
        //
        // var root = immutableListOfBfsNodes[0];
        //
        // var queue = new Queue<DirectoryTreeNode>();
        // queue.Enqueue(root);
        //
        // while (queue.Count > 0)
        // {
        //     var next = queue.Dequeue();
        //     var res = next.Children.Where(coreNodes.Contains).ToList();
        //     next.Children = res;
        //     res.ForEach(queue.Enqueue);
        // }
        //
        //
        // root.ToPrintNode().ToPreOrderPrintNodes().ToTreeString().Log();


        // instance.CreatePrintNodes().ToTreeString().Log();
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
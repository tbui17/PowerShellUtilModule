﻿using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;
using PowerShellStandardModule1.Commands.PrintTree;
using PowerShellStandardModule1.Lib;
using PowerShellStandardModule1.Lib.Extensions;
using PowerShellStandardModule1.Models;
using TestNode = PowerShellStandardModule1.Models.TreeNode<string>;
using static TestProject1.Utils;

namespace TestProject1.PrintTree;

public partial class PrintTreeTest : ContainerInit
{
    private TestNode _root = null!;

    private static readonly DirectoryInfo Directory =
        GetSolutionDirectory() ?? throw new InvalidOperationException("Solution directory not found.");


    [SetUp]
    public void Setup()
    {
        InitData();
    }


    [Test]
    public void TestPrintTree()
    {
        var expected = """
                       Root
                       ├── Child1
                       │   ├── GrandChild1
                       │   │   └── GreatGrandChild1
                       │   │       └── GreatGreatGrandChild1
                       │   └── GrandChild2
                       │       └── GreatGrandChild2
                       └── Child2
                           └── GrandChild3
                               └── GreatGrandChild3
                       """;


        var str = _root
           .ToPreOrderPrintNodes()
           .ToTreeString()
           .Trim();
        using var scope = new AssertionScope();


        str
           .Should()
           .BeEquivalentTo(expected.Trim());
        str.Log();
    }

    [Test]
    public void TestMaxHeight()
    {
        var maxHeight = 1;

        var seq = Traversal.BfsDetailed(_root, GetChildren);

        var res = seq
           .TakeWhile(x => x.Height <= maxHeight)
           .Select(x => x.Value);

        using var scope = new AssertionScope();

        res.ForEach(
            x => x
               .Height.Should()
               .BeLessThanOrEqualTo(maxHeight)
        );
    }


    [Test]
    public void TestOrderedHeight()
    {
        var result = Traversal.BfsDetailed(_root, GetChildren);

        var height = 0;
        using var scope = new AssertionScope();
        foreach (var wrapperNode in result)
        {
            var node = wrapperNode.Value;


            wrapperNode
               .Value.Height.Should()
               .BeGreaterOrEqualTo(height);
            wrapperNode
               .Height.Should()
               .Be(node.Height);
            height = wrapperNode.Height;
        }

        height
           .Should()
           .Be(4);
    }

    [Test]
    public void TestOrdering()
    {
        var instance = new PrintTreeService(
            startingDirectory: Directory,
            descending: true,
            orderBy: "creationtime",
            height: 500,
            limit: 500,
            width: 500,
            nodeWidth: 500,
            rootNodeWidth: 500
        );

        // given flat list of nodes, from a tree, when they are grouped by their parent, there will be partitions that will scope the tests
        var res = instance.CreatePrintNodes();


        using var scope = new AssertionScope();
        res
           .GroupBy(x => x.Parent)
           .Should()
           .AllSatisfy(
                x => x
                   .Select(x2 => x2.Value.Value.CreationTime)
                   .Should()
                   .BeInDescendingOrder()
            );
    }

    [Test]
    public void TestBfs()
    {
        var instance = new PrintTreeService(
            startingDirectory: Directory,
            descending: true,
            orderBy: "creationtime",
            height: 500,
            limit: 500,
            width: 500,
            nodeWidth: 500,
            rootNodeWidth: 500
        );

        var res = instance.CreateTreeNodes();
        res
           .Count.Should()
           .BeGreaterThan(5);
    }

    [Test]
    public void TestFilters()
    {
        using var scope = new AssertionScope();


        var widthFilterCreator = new WidthFilterCreator(nodeWidth: 15, rootNodeWidth: 25);
        var fileFilterCreator = new FileFilterCreator(false);

        fileFilterCreator
           .CreateFilter()
           .Invoke(new FileInfo("a"))
           .Should()
           .BeFalse();

        fileFilterCreator
           .CreateFilter()
           .Invoke(new DirectoryInfo("a"))
           .Should()
           .BeTrue();


        var widthFilter = widthFilterCreator
           .CreateWidthIsWithinLimitsFilter();

        var instance1 = new TreeNode<FileSystemInfo>
        {
            Value = new DirectoryInfo("a"),
            Index = 500
        };


        widthFilter(instance1)
           .Should()
           .BeFalse();

        var instance2 = new TreeNode<FileSystemInfo>
        {
            Value = new DirectoryInfo("a"),
            Index = 18,
            Height = 10
        };

        widthFilter(instance2)
           .Should()
           .BeFalse();

        var widthFilterCreator2 = new WidthFilterCreator(nodeWidth: 50, rootNodeWidth: 15);

        var instance3 = new TreeNode<FileSystemInfo>
        {
            Value = new DirectoryInfo("a"),
            Index = 12,
            Height = 1
        };

        widthFilterCreator2
           .CreateWidthIsWithinLimitsFilter()(instance3)
           .Should()
           .BeTrue();

        widthFilterCreator2
           .CreateWidthIsWithinLimitsFilter()(instance2)
           .Should()
           .BeTrue();
    }


    [Test]
    public void TestWithin()
    {
        var instance = new PrintTreeService(
            startingDirectory: Directory,
            descending: true,
            height: 500,
            limit: 500,
            width: 500,
            nodeWidth: 500,
            rootNodeWidth: 500,
            within: true,
            filter: x => x.Name.Contains("print", StringComparison.OrdinalIgnoreCase)
        );

        var res = instance.CreateTreeNodes();
        using var scope = new AssertionScope();
        res
           .Should()
           .Contain(x => x.Value.Name.Contains("PrintTree", StringComparison.OrdinalIgnoreCase))
           .And.HaveCountGreaterThan(2);
    
    }


    [Test]
    public void TestPreserveTerminalNodes()
    {
        var bfsImpl = new BfsExecutor<FileSystemInfo> { ChildProvider = FsUtil.GetChildren };
        var res = bfsImpl
           .Invoke(GetTestDataDirectory())
           .ToList();

        var initialCount = res.Count;


        var impl = new PreserveTerminalNodeChildrenImpl(filter: x => x.Name == "level3", CancellationToken.None);

        impl.Invoke(res);

        var seq = Traversal
           .Bfs(res[0], x => x.Children)
           .ToList();

        using var scope = new AssertionScope();


        seq
           .Should()
           .Contain(x => x.Value.Name == "level7")
           .And.NotContain(x => x.Value.Name == "level2d")
           .And.HaveCountLessThan(initialCount);
    }
   
}

public partial class PrintTreeTest
{
    IEnumerable<TestNode> GetChildren(TestNode node) => node.Children;

    private void InitData()
    {
        // Arrange
        var root = new TestNode
        {
            Value = "Root",
            Height = 0
        };
        var child1 = new TestNode
        {
            Value = "Child1",
            Height = 1,
            Parent = root,
            Index = 0
        };
        var child2 = new TestNode
        {
            Value = "Child2",
            Height = 1,
            Parent = root,
            Index = 1
        };
        var grandChild1 = new TestNode
        {
            Value = "GrandChild1",
            Height = 2,
            Parent = child1,
            Index = 0
        };
        var grandChild2 = new TestNode
        {
            Value = "GrandChild2",
            Height = 2,
            Parent = child1,
            Index = 1
        };
        var grandChild3 = new TestNode
        {
            Value = "GrandChild3",
            Height = 2,
            Parent = child2,
            Index = 0
        };
        var greatGrandChild1 = new TestNode
        {
            Value = "GreatGrandChild1",
            Height = 3,
            Parent = grandChild1,
            Index = 0
        };
        var greatGrandChild2 = new TestNode
        {
            Value = "GreatGrandChild2",
            Height = 3,
            Parent = grandChild2,
            Index = 0
        };
        var greatGrandChild3 = new TestNode
        {
            Value = "GreatGrandChild3",
            Height = 3,
            Parent = grandChild3,
            Index = 0
        };
        var greatGreatGrandChild1 = new TestNode
        {
            Value = "GreatGreatGrandChild1",
            Height = 4,
            Parent = greatGrandChild1,
            Index = 0
        };

        root.Children = new List<TestNode>
        {
            child1,
            child2
        };
        child1.Children = new List<TestNode>
        {
            grandChild1,
            grandChild2
        };
        child2.Children = new List<TestNode> { grandChild3 };
        grandChild1.Children = new List<TestNode> { greatGrandChild1 };
        grandChild2.Children = new List<TestNode> { greatGrandChild2 };
        grandChild3.Children = new List<TestNode> { greatGrandChild3 };
        greatGrandChild1.Children = new List<TestNode> { greatGreatGrandChild1 };
        _root = root;
    }
}
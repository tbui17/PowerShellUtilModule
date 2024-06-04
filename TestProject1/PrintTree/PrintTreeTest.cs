using Autofac;
using FluentAssertions;
using FluentAssertions.Execution;
using PowerShellStandardModule1.Commands.PrintTree;
using PowerShellStandardModule1.Lib.Extensions;
using TestNode = PowerShellStandardModule1.Models.TreeNode<string>;

namespace TestProject1.PrintTree;

public partial class PrintTreeTest
{
    private TestNode _root = null!;

    private static readonly DirectoryInfo Directory =
        GetSolutionDirectory() ?? throw new InvalidOperationException("Solution directory not found.");


    [SetUp]
    public void Setup()
    {
        Init(Initializer);
        InitData();
    }

    private static void Initializer(ContainerBuilder b)
    {
        b.Register<StringValueSelector>(_ => x => x.Value.Name);

        b.RegisterInstance(Directory);
        b
           .RegisterType<PrintTreeService>()
           .PropertiesAutowired();

        b
           .RegisterType<PrintTreeCommand>()
           .WithProperty("StartingDirectory", Directory.Name)
           .PropertiesAutowired();
    }

    private static DirectoryInfo? GetSolutionDirectory(string? currentPath = null)
    {
        currentPath ??= System.IO.Directory.GetCurrentDirectory();

        var directory = new DirectoryInfo(currentPath);

        while (directory is not null && HasNoSolution())
        {
            directory = directory.Parent;
        }

        return directory;

        bool HasNoSolution()
        {
            var files = directory.GetFiles("*.sln");
            return files.Length == 0;
        }
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
        var instance = new PrintTreeService
        {
            StartingDirectory = Directory,
            Descending = true,
            OrderBy = "creationtime",
            Height = 500,
            Limit = 500,
            Width = 500,
            NodeWidth = 500,
            RootNodeWidth = 500
        };

        // given flat list of nodes, from a tree, when they are grouped by their parent, there will be partitions that will scope the tests
        var res = instance.CreatePrintNodes();

        using var scope = new AssertionScope();
        res
           .GroupBy(x => x.Parent)
           .ForEach(
                x => x
                   .Select(x2 => x2.Value.Value.CreationTime)
                   .Should()
                   .BeInDescendingOrder()
            );
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
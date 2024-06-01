using Autofac;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;
using PowerShellStandardModule1.Commands.PrintTree;
using PowerShellStandardModule1.Delegates;
using PowerShellStandardModule1.Lib.Extensions;
using PowerShellStandardModule1.Models;
using TestNode = PowerShellStandardModule1.Models.TreeNode<string>;

namespace TestProject1;

using ChildProvider = Func<PrintNode<DirectoryInfo>, IEnumerable<TreeNode<DirectoryInfo>>>;

public partial class PrintTreeTest
{
    private TestNode _root = null!;

    private ChildProvider _childProvider = PrintNode.DefaultChildProvider;


    [SetUp]
    public void Setup()
    {
        Init(Initializer);
        InitData();
    }

    private void Initializer(ContainerBuilder b)
    {
        b.Register<StringValueSelector>(_ => x => x.Value.Name);
        var dir = EnvVars
           .HOME_DIRECTORY.Get()
           .Pipe(x => new DirectoryInfo(x));
        b.RegisterInstance(dir);

        b
           .RegisterType<PrintTreeService>()
           .PropertiesAutowired();
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
    public void TestTreeRunner()
    {
        var height = 3;
        var nodeWidth = 10;
        var width = 100;
        var take = 400000;
        var rootNodeWidth = 100;


        var instance = Resolve<PrintTreeService>();

        instance.Height = height;
        instance.NodeWidth = nodeWidth;
        instance.Width = width;
        instance.Limit = take;
        instance.RootNodeWidth = rootNodeWidth;


        var res = instance.CreateTreeNodes();
        res
           .Should()
           .HaveCountGreaterThan(0);
        var root = res[0];


        var printNodes = instance
           .CreatePrintNodes(root)
           .ToList();


        using var scope = new AssertionScope();

        res
           .Should()
           .HaveCountLessThan(instance.Limit);
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
           .All(x => x.ChildProvider == _childProvider)
           .Should()
           .Be(true, "Nodes should inherit child provider from the root");

        // printNodes.ForEach(
        //     x =>
        //     {
        //         if (x.Value.Height is 0)
        //         {
        //             x
        //                .CompiledIndent.Count.Should()
        //                .Be(0, "The root is not indented.");
        //             
        //         }
        //         else
        //         {
        //             x
        //                .CompiledIndent.Count.Should()
        //                .Be(x.Value.Height);
        //         }
        //
        //        
        //
        //         // (x.Value.Height, x.CompiledIndent.Serialize()).Log();
        //     }
        // );


        printNodes
           .Select(x => x.CompiledIndent.Serialize())
           .Serialize(Formatting.Indented)
           .Log();
        
        
        printNodes
           .ToTreeString()
           .Log();
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
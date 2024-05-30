using FluentAssertions;
using FluentAssertions.Execution;
using PowerShellStandardModule1;
using static PowerShellStandardModule1.Extensions;
using TestNode = PowerShellStandardModule1.Structs.TreeNode<string>;


namespace TestProject1;

public class PrintTreeExtensionsTest
{
    private TestNode _root = null!;


    [SetUp]
    public void Setup()
    {
        // Arrange
        var root = new TestNode { Value = "Root", Height = 0 };
        var child1 = new TestNode { Value = "Child1", Height = 1, Parent = root, Index = 0 };
        var child2 = new TestNode { Value = "Child2", Height = 1, Parent = root, Index = 1 };
        var grandChild1 = new TestNode { Value = "GrandChild1", Height = 2, Parent = child1, Index = 0 };
        var grandChild2 = new TestNode { Value = "GrandChild2", Height = 2, Parent = child1, Index = 1 };
        var grandChild3 = new TestNode { Value = "GrandChild3", Height = 2, Parent = child2, Index = 0 };
        var greatGrandChild1 = new TestNode { Value = "GreatGrandChild1", Height = 3, Parent = grandChild1, Index = 0 };
        var greatGrandChild2 = new TestNode { Value = "GreatGrandChild2", Height = 3, Parent = grandChild2, Index = 0 };
        var greatGrandChild3 = new TestNode { Value = "GreatGrandChild3", Height = 3, Parent = grandChild3, Index = 0 };
        var greatGreatGrandChild1 = new TestNode
            { Value = "GreatGreatGrandChild1", Height = 4, Parent = greatGrandChild1, Index = 0 };

        root.Children = new List<TestNode> { child1, child2 };
        child1.Children = new List<TestNode> { grandChild1, grandChild2 };
        child2.Children = new List<TestNode> { grandChild3 };
        grandChild1.Children = new List<TestNode> { greatGrandChild1 };
        grandChild2.Children = new List<TestNode> { greatGrandChild2 };
        grandChild3.Children = new List<TestNode> { greatGrandChild3 };
        greatGrandChild1.Children = new List<TestNode> { greatGreatGrandChild1 };
        _root = root;
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


        var str = _root.ToTreeString().Trim();
        using var scope = new AssertionScope();
        str.Log();
        str.Should().BeEquivalentTo(expected.Trim());
    }

    [Test]
    public void TestMaxHeight()
    {
        var maxHeight = 1;

        var seq = BfsDetailed(_root, GetChildren);

        var res = seq.TakeWhile(x => x.Height <= maxHeight).Select(x => x.Value);

        using var scope = new AssertionScope();

        res.ForEach(x => x.Height.Should().BeLessThanOrEqualTo(maxHeight));
    }

    [Test]
    public void TestIntegration()
    {
        var dir = EnvVars.HOME_DIRECTORY.Get().Pipe(x => new DirectoryInfo(x));
        var maxHeight = 5;
        var maxWidth = 10;

        var baseGetter = ChildGetterFactory.CreateDirectoryChildGetter();

        // inject node width limiter (children count) into getter, which acts within bfs function
        var getterWithWidthLimitedNodes = baseGetter.Compose(x => x.Take(maxWidth));

        var treeNodes = BfsDetailed(dir, getterWithWidthLimitedNodes)
           .TakeWhile(x => x.Height < maxHeight)
           .ToList(); // must materialize to populate children


        var treeNodeTestCases = treeNodes.Select(
            Action (node) => () =>
            {
                node.Height.Should().BeLessThanOrEqualTo(maxHeight);
                node.Children.Count().Should().BeLessThanOrEqualTo(maxWidth);
            }
        );

        var printNodes = treeNodes
           .First()
           .ToPreOrderPrintNodes()
           .Select(outerNode => outerNode with { StringValueSelector = node => node.Value.Name })
           .ToList(); // modify formatting of all involved nodes

        var printNodeTestCases = printNodes.Select(
            Action (node) => () => { node.StringValue.Should().NotContainAny("/", "\\"); }
        );

        var allTestCases = treeNodeTestCases
           .Concat(printNodeTestCases)
           .ToList();

        allTestCases.Count.Should().BeGreaterThan(0);

        using var scope = new AssertionScope();
        allTestCases.ForEach(x => x());
    }


    [Test]
    public void TestOrderedHeight()
    {
        var result = BfsDetailed(_root, GetChildren);

        var height = 0;
        using var scope = new AssertionScope();
        foreach (var wrapperNode in result)
        {
            var node = wrapperNode.Value;


            wrapperNode.Value.Height.Should().BeGreaterOrEqualTo(height);
            wrapperNode.Height.Should().Be(node.Height);
            height = wrapperNode.Height;
        }

        height.Should().Be(4);
    }

    IEnumerable<TestNode> GetChildren(TestNode node) => node.Children;
}
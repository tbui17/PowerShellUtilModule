using Autofac;
using FluentAssertions;
using FluentAssertions.Execution;
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

    private static readonly DirectoryInfo Directory =
        GetSolutionDirectory() ?? throw new InvalidOperationException("Solution directory not found.");


    [SetUp]
    public void Setup()
    {
        Init(Initializer);
        InitData();
    }

    private void Initializer(ContainerBuilder b)
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
    public void TestIntegrationTest()
    {
        var height = 6;
        var nodeWidth = 10;
        var width = 10;
        var take = 20;
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


    [Test]
    public void TestSerialization()
    {
        using var scope = new AssertionScope { FormattingOptions = { MaxDepth = 100 } };
        var n = TreeNode.JsonParse<string>(JsonData)!;
        n
           .Should()
           .NotBeNull();
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
    private const string JsonData = """
                                    {
                                      "Parent": null,
                                      "Children": [
                                        {
                                          "Parent": null,
                                          "Children": [
                                            {
                                              "Parent": null,
                                              "Children": [
                                                {
                                                  "Parent": null,
                                                  "Children": [],
                                                  "Height": 3,
                                                  "Index": 0,
                                                  "Value": ".idea"
                                                }
                                              ],
                                              "Height": 2,
                                              "Index": 0,
                                              "Value": ".idea.PowerShellStandardModule1"
                                            }
                                          ],
                                          "Height": 1,
                                          "Index": 0,
                                          "Value": ".idea"
                                        },
                                        {
                                          "Parent": null,
                                          "Children": [
                                            {
                                              "Parent": null,
                                              "Children": [],
                                              "Height": 2,
                                              "Index": 0,
                                              "Value": "Attributes"
                                            },
                                            {
                                              "Parent": null,
                                              "Children": [
                                                {
                                                  "Parent": null,
                                                  "Children": [
                                                    {
                                                      "Parent": null,
                                                      "Children": [],
                                                      "Height": 4,
                                                      "Index": 0,
                                                      "Value": "net6.0"
                                                    },
                                                    {
                                                      "Parent": null,
                                                      "Children": [],
                                                      "Height": 4,
                                                      "Index": 1,
                                                      "Value": "net8.0"
                                                    },
                                                    {
                                                      "Parent": null,
                                                      "Children": [],
                                                      "Height": 4,
                                                      "Index": 2,
                                                      "Value": "netstandard2.0"
                                                    }
                                                  ],
                                                  "Height": 3,
                                                  "Index": 0,
                                                  "Value": "Debug"
                                                },
                                                {
                                                  "Parent": null,
                                                  "Children": [
                                                    {
                                                      "Parent": null,
                                                      "Children": [
                                                        {
                                                          "Parent": null,
                                                          "Children": [
                                                            {
                                                              "Parent": null,
                                                              "Children": [],
                                                              "Height": 6,
                                                              "Index": 0,
                                                              "Value": "publish"
                                                            }
                                                          ],
                                                          "Height": 5,
                                                          "Index": 0,
                                                          "Value": "win-x64"
                                                        }
                                                      ],
                                                      "Height": 4,
                                                      "Index": 0,
                                                      "Value": "net8.0"
                                                    }
                                                  ],
                                                  "Height": 3,
                                                  "Index": 1,
                                                  "Value": "Release"
                                                }
                                              ],
                                              "Height": 2,
                                              "Index": 1,
                                              "Value": "bin"
                                            },
                                            {
                                              "Parent": null,
                                              "Children": [
                                                {
                                                  "Parent": null,
                                                  "Children": [],
                                                  "Height": 3,
                                                  "Index": 0,
                                                  "Value": "Bfs"
                                                },
                                                {
                                                  "Parent": null,
                                                  "Children": [],
                                                  "Height": 3,
                                                  "Index": 1,
                                                  "Value": "Fuzzy"
                                                },
                                                {
                                                  "Parent": null,
                                                  "Children": [],
                                                  "Height": 3,
                                                  "Index": 2,
                                                  "Value": "PrintTree"
                                                },
                                                {
                                                  "Parent": null,
                                                  "Children": [],
                                                  "Height": 3,
                                                  "Index": 3,
                                                  "Value": "Sample"
                                                }
                                              ],
                                              "Height": 2,
                                              "Index": 2,
                                              "Value": "Commands"
                                            },
                                            {
                                              "Parent": null,
                                              "Children": [],
                                              "Height": 2,
                                              "Index": 3,
                                              "Value": "Delegates"
                                            },
                                            {
                                              "Parent": null,
                                              "Children": [
                                                {
                                                  "Parent": null,
                                                  "Children": [],
                                                  "Height": 3,
                                                  "Index": 0,
                                                  "Value": "Extensions"
                                                }
                                              ],
                                              "Height": 2,
                                              "Index": 4,
                                              "Value": "Lib"
                                            },
                                            {
                                              "Parent": null,
                                              "Children": [],
                                              "Height": 2,
                                              "Index": 5,
                                              "Value": "Models"
                                            },
                                            {
                                              "Parent": null,
                                              "Children": [
                                                {
                                                  "Parent": null,
                                                  "Children": [
                                                    {
                                                      "Parent": null,
                                                      "Children": [
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 0,
                                                          "Value": "ref"
                                                        },
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 1,
                                                          "Value": "refint"
                                                        }
                                                      ],
                                                      "Height": 4,
                                                      "Index": 0,
                                                      "Value": "net6.0"
                                                    },
                                                    {
                                                      "Parent": null,
                                                      "Children": [
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 0,
                                                          "Value": "ref"
                                                        },
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 1,
                                                          "Value": "refint"
                                                        }
                                                      ],
                                                      "Height": 4,
                                                      "Index": 1,
                                                      "Value": "net8.0"
                                                    },
                                                    {
                                                      "Parent": null,
                                                      "Children": [],
                                                      "Height": 4,
                                                      "Index": 2,
                                                      "Value": "netstandard2.0"
                                                    }
                                                  ],
                                                  "Height": 3,
                                                  "Index": 0,
                                                  "Value": "Debug"
                                                },
                                                {
                                                  "Parent": null,
                                                  "Children": [
                                                    {
                                                      "Parent": null,
                                                      "Children": [
                                                        {
                                                          "Parent": null,
                                                          "Children": [
                                                            {
                                                              "Parent": null,
                                                              "Children": [],
                                                              "Height": 6,
                                                              "Index": 0,
                                                              "Value": "ref"
                                                            },
                                                            {
                                                              "Parent": null,
                                                              "Children": [],
                                                              "Height": 6,
                                                              "Index": 1,
                                                              "Value": "refint"
                                                            }
                                                          ],
                                                          "Height": 5,
                                                          "Index": 0,
                                                          "Value": "win-x64"
                                                        }
                                                      ],
                                                      "Height": 4,
                                                      "Index": 0,
                                                      "Value": "net8.0"
                                                    }
                                                  ],
                                                  "Height": 3,
                                                  "Index": 1,
                                                  "Value": "Release"
                                                }
                                              ],
                                              "Height": 2,
                                              "Index": 6,
                                              "Value": "obj"
                                            },
                                            {
                                              "Parent": null,
                                              "Children": [],
                                              "Height": 2,
                                              "Index": 7,
                                              "Value": "Structs"
                                            }
                                          ],
                                          "Height": 1,
                                          "Index": 1,
                                          "Value": "PowerShellStandardModule1"
                                        },
                                        {
                                          "Parent": null,
                                          "Children": [
                                            {
                                              "Parent": null,
                                              "Children": [
                                                {
                                                  "Parent": null,
                                                  "Children": [
                                                    {
                                                      "Parent": null,
                                                      "Children": [
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 0,
                                                          "Value": "cs"
                                                        },
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 1,
                                                          "Value": "de"
                                                        },
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 2,
                                                          "Value": "es"
                                                        },
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 3,
                                                          "Value": "fr"
                                                        },
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 4,
                                                          "Value": "it"
                                                        },
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 5,
                                                          "Value": "ja"
                                                        },
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 6,
                                                          "Value": "ko"
                                                        },
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 7,
                                                          "Value": "pl"
                                                        },
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 8,
                                                          "Value": "pt-BR"
                                                        },
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 9,
                                                          "Value": "ru"
                                                        }
                                                      ],
                                                      "Height": 4,
                                                      "Index": 0,
                                                      "Value": "net8.0"
                                                    }
                                                  ],
                                                  "Height": 3,
                                                  "Index": 0,
                                                  "Value": "Debug"
                                                }
                                              ],
                                              "Height": 2,
                                              "Index": 0,
                                              "Value": "bin"
                                            },
                                            {
                                              "Parent": null,
                                              "Children": [
                                                {
                                                  "Parent": null,
                                                  "Children": [
                                                    {
                                                      "Parent": null,
                                                      "Children": [
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 0,
                                                          "Value": "ref"
                                                        },
                                                        {
                                                          "Parent": null,
                                                          "Children": [],
                                                          "Height": 5,
                                                          "Index": 1,
                                                          "Value": "refint"
                                                        }
                                                      ],
                                                      "Height": 4,
                                                      "Index": 0,
                                                      "Value": "net8.0"
                                                    }
                                                  ],
                                                  "Height": 3,
                                                  "Index": 0,
                                                  "Value": "Debug"
                                                }
                                              ],
                                              "Height": 2,
                                              "Index": 1,
                                              "Value": "obj"
                                            }
                                          ],
                                          "Height": 1,
                                          "Index": 2,
                                          "Value": "TestProject1"
                                        }
                                      ],
                                      "Height": 0,
                                      "Index": 0,
                                      "Value": "PowerShellStandardModule1"
                                    }
                                    """;

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
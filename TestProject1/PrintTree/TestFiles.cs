using FluentAssertions;
using FluentAssertions.Execution;
using PowerShellStandardModule1.Commands.PrintTree;
using PowerShellStandardModule1.Models;

namespace TestProject1.PrintTree;

class PrintFilesTest : ContainerInit
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void TestDisabled()
    {
       
        
        var instance = new PrintTreeService(
            startingDirectory: Utils.GetSolutionDirectory(),
            height: 500,
            limit: 500,
            width: 500,
            nodeWidth: 500,
            rootNodeWidth: 500,
            filter: _ => true,
            file: false
        );
        

        instance
           .CreateTreeNodes()
           .Should()
           .AllSatisfy(
                x => x
                   .Value.Should()
                   .NotBeAssignableTo<FileInfo>()
            );
    }

    [Test]
    public void TestEnabled()
    {
       
        
        
        var instance = new PrintTreeService(
            startingDirectory: Utils.GetSolutionDirectory(),
            height: 500,
            limit: 500,
            width: 500,
            nodeWidth: 500,
            rootNodeWidth: 500,
            filter: _ => true,
            file: true
        );

        var res = instance
           .CreateTreeNodes()
           .Select(x => x.Value)
           .ToList();


        res
           .Should()
           .ContainItemsAssignableTo<FileInfo>();
    }

    [Test]
    public void TestCreateFilter()
    {
        var instance = new PrintTreeService(
            startingDirectory: Utils.GetSolutionDirectory(),
            height: 500,
            limit: 500,
            width: 500,
            nodeWidth: 500,
            rootNodeWidth: 500,
            filter: _ => true,
            file: true
        );


        var widthFilterCreator = new WidthFilterCreator(nodeWidth: 500, rootNodeWidth: 500);
        var filter = instance.CreateShouldContinueFilter(widthFilterCreator);

        var file = new FileInfo("a");
        var dir = new DirectoryInfo("b");

        var fileNode = new TreeNode<FileSystemInfo> { Value = file };
        var dirNode = new TreeNode<FileSystemInfo> { Value = dir };

        using var scope = new AssertionScope();
        filter(fileNode)
           .Should()
           .Be(true);
        filter(dirNode)
           .Should()
           .Be(true);
    }

    [Test]
    public void TestCreateFilterDisabledFile()
    {
       
        
        var instance = new PrintTreeService(
            startingDirectory: Utils.GetSolutionDirectory(),
            height: 500,
            limit: 500,
            width: 500,
            nodeWidth: 500,
            rootNodeWidth: 500,
            filter: _ => true,
            file: false
        );
        
        
        var widthFilterCreator = new WidthFilterCreator(nodeWidth: 500, rootNodeWidth: 500);
        var filter = instance.CreateShouldContinueFilter(widthFilterCreator);

        var file = new FileInfo("a");
        var dir = new DirectoryInfo("b");

        var fileNode = new TreeNode<FileSystemInfo> { Value = file };
        var dirNode = new TreeNode<FileSystemInfo> { Value = dir };

        using var scope = new AssertionScope();
        filter(fileNode)
           .Should()
           .Be(false);
        filter(dirNode)
           .Should()
           .Be(true);
    }
}
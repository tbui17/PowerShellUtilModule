using FluentAssertions;
using PowerShellStandardModule1.Commands.PrintTree;

namespace TestProject1.PrintTree;

public class PrintFilesTest : ContainerInit
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

}
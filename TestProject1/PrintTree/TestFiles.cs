using System.Management.Automation.Language;
using FluentAssertions;
using PowerShellStandardModule1.Commands.PrintTree;
using PowerShellStandardModule1.Lib.Extensions;

namespace TestProject1.PrintTree;

class PrintFilesTest :ContainerInit
{
    
    
    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public void TestFiles()
    {
        var instance = Resolve<PrintTreeService>();

        instance.File = false;

        instance
           .GetPropertiesOfType<int>()
           .ForEach(x => x.SetValue(instance, 1000));

        instance
           .CreateTreeNodes()
           .Should()
           .AllSatisfy(
                x => x
                   .Value.Should()
                   .NotBeAssignableTo<FileInfo>()
            );




    }
}
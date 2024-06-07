using AutoMapper;
using FluentAssertions;
using FluentBuilder;
using PowerShellStandardModule1.Commands.PrintTree;
using PowerShellStandardModule1.Lib.Extensions;

namespace TestProject1;

public class ReflectionTests
{
    [Test]
    public void TestDynamicAssignment()
    {
        var vals = new
        {
            Height = 1,
            NodeWidth = 2,
            Width = 3,
            Limit = 4
        };


        var mappedData = vals
           .ToPairsFromProperties<int>()
           .SelectValues(x => x + 100)
           .ToDictionary();

        var instance = new TestClass()
        {
            Height = -235,
            NodeWidth = -523626,
            Width = -253235,
            Limit = -53225
        };

        mappedData.ForEach(x => instance.SetPropertyValue(x.Key, x.Value));

        var joined = mappedData.Join(instance.ToPairsFromProperties<int>());

        var res = joined.SelectValues(x => x.Item2);

        res
           .ToDictionary()
           .Should()
           .BeEquivalentTo(mappedData);
    }

    [Test]
    public void TestMember()
    {
        var cfg = new MapperConfiguration(
            c =>
            {
                c
                   .CreateMap<PrintTreeCommand, PrintTreeService>()
                   .MapProperty(s => new DirectoryInfo(s.StartingDirectory), d => d.StartingDirectory);
            }
        );
        var mapper = cfg.CreateMapper();

        var cmd = new PrintTreeCommand()
        {
            Depth = 11,
            NodeChildren = 12,
            StartingDirectory = "MyDir",
            Width = 13,
            RootNodeChildren = 14,
            Limit = 15,
        };
        var instance = mapper.Map<PrintTreeService>(cmd);
        instance
           .StartingDirectory.Name.Should()
           .Be("MyDir");
    }

   
}

[AutoGenerateBuilder]
public partial class TestClass
{
    public int Height { get; set; }
    public int NodeWidth { get; set; }
    public int Width { get; set; }
    public int Limit { get; set; }
    public string ExtraProp { get; set; } = "q";
}
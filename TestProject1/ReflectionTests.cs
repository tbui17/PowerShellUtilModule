using AutoMapper;
using FluentAssertions;
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

        var instance = new PrintTreeService
        {
            StartingDirectory = new DirectoryInfo("a"),
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
            Height = 11,
            NodeWidth = 12,
            StartingDirectory = "MyDir",
            Width = 13,
            RootNodeWidth = 14,
            Limit = 15,
        };
        var instance = mapper.Map<PrintTreeService>(cmd);
        instance
           .StartingDirectory.Name.Should()
           .Be("MyDir");
    }
}
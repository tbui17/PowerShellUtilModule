using FluentAssertions;
using FluentAssertions.Execution;
using FuzzySharp;
using PowerShellStandardModule1;
using PowerShellStandardModule1.Commands;
using PowerShellStandardModule1.Commands.Fuzzy;
using PowerShellStandardModule1.Models;

namespace TestProject1;

public class FuzzyTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestFuzzy()
    {
        var str1 = "hello";
        var str2 = "hello2";
        var score = Fuzz.Ratio(str1, str2);
        var command = new SelectFuzzyCommand
        {
            String1 = str1,
            String2 = str2,
            Strategy = "Ratio"
        };

        var res = command.Run();
        var data = new FuzzyResult("hello", "hello2", score);

        res.Should().BeEquivalentTo(data);
    }

    [Test]
    public void TestFuzzyReflect()
    {
        // var strategies = typeof(Fuzz)
        //     .GetMethods()
        //     .Where(x => x
        //         .Name
        //         .ToLowerInvariant()
        //         .Contains("ratio")
        //     )
        //     .Select(x => x.Name)
        //     .Distinct();
        // SelectFuzzyCommand.GetFuzzyStrategy("")
        string[] strategies =
        [
            "PartialRatio", "PartialTokenAbbreviationRatio", "PartialTokenDifferenceRatio",
            "PartialTokenInitialismRatio", "PartialTokenSetRatio", "PartialTokenSortRatio", "Ratio",
            "TokenAbbreviationRatio", "TokenDifferenceRatio", "TokenInitialismRatio", "TokenSetRatio", "TokenSortRatio",
            "WeightedRatio"
        ];


        var cases = strategies.Select(x =>
        {
            var action = () =>
            {
                var fn = SelectFuzzyCommand.GetFuzzyStrategy(x);
                fn.Should().NotBeNull();
                fn("Render", "end").Should().BeGreaterThanOrEqualTo(0);
            };
            return action;
        });


        using var scope = new AssertionScope();
        foreach (var testCase in cases)
        {
            testCase.Should().NotThrow();
        }
    }
    
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using FuzzySharp;
using PowerShellStandardModule1.Models;

namespace PowerShellStandardModule1.Commands.Fuzzy;

using FuzzyFunc = Func<string, string, int>;

public enum FuzzyStrategy
{
    PartialRatio,
    PartialTokenAbbreviationRatio,
    PartialTokenDifferenceRatio,
    PartialTokenInitialismRatio,
    PartialTokenSetRatio,
    PartialTokenSortRatio,
    Ratio,
    TokenAbbreviationRatio,
    TokenDifferenceRatio,
    TokenInitialismRatio,
    TokenSetRatio,
    TokenSortRatio,
    WeightedRatio
}

[Cmdlet(VerbsCommon.Select, "Fuzzy")]
[Alias("Fuzzy")]
[OutputType(typeof(FuzzyResult))]
public class SelectFuzzyCommand : PSCmdlet
{
    [Parameter(
        Mandatory = true,
        Position = 0,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The first string to compare."
    )]
    public required string String1;

    [Parameter(
        Mandatory = true,
        Position = 1,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The second string to compare."
    )]
    public required string String2;

    [Parameter(
        Position = 2, HelpMessage = """
                                    Strategy to use for fuzzy matching. Defaults to 'Ratio'.
                                    Options: "PartialRatio", "PartialTokenAbbreviationRatio", "PartialTokenDifferenceRatio",
                                    "PartialTokenInitialismRatio", "PartialTokenSetRatio", "PartialTokenSortRatio", "Ratio",
                                    "TokenAbbreviationRatio", "TokenDifferenceRatio", "TokenInitialismRatio", "TokenSetRatio", "TokenSortRatio",
                                    "WeightedRatio"
                                    """
    )]
    public FuzzyStrategy Strategy = FuzzyStrategy.Ratio;


    protected override void ProcessRecord()
    {
        var res = Run();
        WriteObject(res);
    }

    public FuzzyResult Run()
    {
        var score = FuzzyFuncs
           .GetValueOrDefault(Strategy, Fuzz.Ratio)(String1, String2);
        return new FuzzyResult(String1, String2, score);
    }

    public static FuzzyFunc GetFuzzyStrategy(string strategy)
    {
        var arity = typeof(FuzzyFunc)
           .GetMethod("Invoke")!
           .GetParameters()
           .Length;


        var res = typeof(Fuzz)
               .GetMethods()
               .Where(x => x.IsStatic)
               .Where(
                    x => x.GetParameters()
                           .Length ==
                        arity
                )
               .FirstOrDefault(x => string.Equals(x.Name, strategy, StringComparison.InvariantCultureIgnoreCase))
             ?
            .CreateDelegate(typeof(FuzzyFunc)) ??
            throw new ArgumentException($"Invalid strategy: {strategy}");

        return (FuzzyFunc)res;
    }

    public static Dictionary<FuzzyStrategy, FuzzyFunc> FuzzyFuncs = typeof(Fuzz)
       .GetMethods()
       .Where(x => x.IsStatic)
       .Where(
            x => x.GetParameters()
                   .Length ==
                2
        )
       .Where(x => x.Name.Contains("ratio", StringComparison.OrdinalIgnoreCase))
       .DistinctBy(x => x.Name)
       .Select(x => KeyValuePair.Create(Enum.Parse<FuzzyStrategy>(x.Name), x.CreateDelegate<FuzzyFunc>()))
       .ToDictionary();
}
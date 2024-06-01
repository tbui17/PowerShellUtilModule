﻿using System;
using System.Linq;
using System.Management.Automation;
using FuzzySharp;
using PowerShellStandardModule1.Models;


namespace PowerShellStandardModule1.Commands.Fuzzy;

using FuzzyFunc = Func<string, string, int>;

[Cmdlet(VerbsCommon.Select, "Fuzzy")]
[Alias("Fuzzy")]
[OutputType(typeof(FuzzyResult))]
public class SelectFuzzyCommand : PSCmdlet
{
    [Parameter(
        Mandatory = true,
        Position = 0,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true
    )]
    public required string String1;

    [Parameter(
        Mandatory = true,
        Position = 1
    )]
    public required string String2;

    [Parameter(Position = 2)]
    public string Strategy = "Ratio";


    protected override void ProcessRecord()
    {
        var res = Run();
        WriteObject(res);
    }

    public FuzzyResult Run()
    {
        var score = GetFuzzyStrategy(Strategy)(String1, String2);
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
}
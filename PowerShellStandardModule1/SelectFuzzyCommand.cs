using System;
using System.Linq;
using System.Management.Automation;
using FuzzySharp;

public delegate int FuzzyFunc(string s1, string s2);

namespace PowerShellStandardModule1
{
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

        [Parameter(
            Position = 2
        )]
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

            return (FuzzyFunc)typeof(Fuzz)
                .GetMethods()
                .Where(x => x.IsStatic)
                .Where(x => x.GetParameters().Length == arity)
                .First(x => string.Equals(x.Name, strategy, StringComparison.InvariantCultureIgnoreCase))
                .CreateDelegate(typeof(FuzzyFunc));
        }
    }
}
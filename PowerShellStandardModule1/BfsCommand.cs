using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace PowerShellStandardModule1
{
    [Cmdlet(verbName: VerbsCommon.Get, nounName: "Bfs")]
    
    [Alias("Bfs")]
    [OutputType(typeof(DirectoryInfo))]
    public class BfsCommand : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "The pattern to search for. Follows same conventions as other PowerShell commands."
        )]
        public required string Pattern;

        [Parameter(
            Position = 1,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The directory to start the search from. Defaults to the current directory."
        )]
        public readonly string StartingDirectory = Directory.GetCurrentDirectory();

        [Parameter(HelpMessage = "Whether or not to ignore casing when comparing names. True by default.")]
        public bool IgnoreCase = true;

        [Parameter(HelpMessage = "How many results to take before stopping.")]
        public int First = 1;

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            IEnumerable<DirectoryInfo> res;
            try
            {
                res = new BfsRunner(Pattern, StartingDirectory, IgnoreCase,take:First).Run();
            }
            catch (DirectoryNotFoundException e)
            {
                WriteError(new ErrorRecord(e, "DirectoryNotFound", ErrorCategory.ObjectNotFound, StartingDirectory));
                return;
            }

            foreach (var x in res)
            {
                WriteObject(x);
            }
        }
    }
}
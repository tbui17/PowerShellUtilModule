using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PowerShellStandardModule1
{
    [Cmdlet(verbName: VerbsCommon.Get, nounName: "Bfs")]
    [Alias("Bfs")]
    [OutputType(typeof(PSObject))]
    public class BfsCommand : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "The pattern to search for. Follows same conventions as other PowerShell commands."
        )]
        public required string Pattern;

        private string? _startingDirectory;

        [Parameter(
            Position = 1,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The directory to start the search from. Defaults to the current directory."
        )]
        public string StartingDirectory
        {
            get => _startingDirectory ?? CurrentProviderLocation("FileSystem").ProviderPath;
            set => _startingDirectory = value;
        }

        [Parameter(HelpMessage = "Whether or not to ignore casing when comparing names. True by default.")]
        public bool IgnoreCase = true;

        [Parameter(
            HelpMessage = "How many results to take before stopping. Defaults to 1. Negative numbers are rounded to 0."
        )]
        public int First = 1;

        [Parameter(
            HelpMessage =
                "How long to continue search before timing out in ms, if not all directories have been exhausted. Defaults to int32 max. Negative numbers are rounded to 0."
        )]
        public int Timeout = int.MaxValue;

        [Parameter(
            HelpMessage =
                "The maximum amount of items to process, regardless if they match the pattern. Defaults to int32 max. Negative numbers rounded to 0."
        )]
        public int Limit = int.MaxValue;

        private CancellationTokenSource _cts = null!;

        protected override void BeginProcessing()
        {
            _cts = new CancellationTokenSource();
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {

            IEnumerable<PSObject> result;
            var runner = new BfsRunner(Pattern, StartingDirectory, IgnoreCase, itemsToReturn: First, limit: Limit);
            _cts.CancelAfter(Math.Max(0, Timeout));


            try
            {
                // run task in a new thread then serialize result to PSObject
                var task = Task.Run(() => runner.Run(_cts.Token), _cts.Token);
                task.Wait();
                IEnumerable<DirectoryInfo> taskResult = task.GetAwaiter().GetResult();

                result = taskResult.Select(x => new PSObject(x));
            }
            catch (DirectoryNotFoundException e)
            {
                WriteError(new ErrorRecord(e, "DirectoryNotFound", ErrorCategory.ObjectNotFound, StartingDirectory));
                return;
            }
            catch (OperationCanceledException)
            {
                var record = new InformationRecord(
                    new
                    {
                        message = "Operation was cancelled.",
                        directory = StartingDirectory,
                        pattern = Pattern,
                        ignoreCase = IgnoreCase,
                        first = First,
                    },
                    "BfsCommand.OperationCancelled"
                );
                WriteInformation(record);
                return;
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "UnknownError", ErrorCategory.NotSpecified, StartingDirectory));
                return;
            }

            foreach (var x in result) WriteObject(x);
        }

        protected override void StopProcessing()
        {
            _cts.Cancel();
        }
    }
}
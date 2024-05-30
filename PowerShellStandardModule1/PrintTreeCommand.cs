using System;

using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;


namespace PowerShellStandardModule1
{
    [Cmdlet(verbName: VerbsCommon.Get, nounName: "Bfs")]
    [Alias("PrintTree")]
    [OutputType(typeof(string))]
    public class PrintTreeCommand : PSCmdlet
    {
        private string? _startingDirectory;

        [Parameter(
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The directory to start the search from. Defaults to the current directory."
        )]
        public string StartingDirectory
        {
            get => _startingDirectory ?? CurrentProviderLocation("FileSystem").ProviderPath;
            set => _startingDirectory = value;
        }

        [Parameter(Position = 1, HelpMessage = "The maximum depth to search. Defaults to int32 max.")]
        public int Height = Int32.MaxValue;

        private CancellationTokenSource _cts = null!;

        protected override void BeginProcessing()
        {
            _cts = new CancellationTokenSource();
        }


        protected override void ProcessRecord()
        {
            string result = "";

            var childGetter = ChildGetterFactory.CreateDirectoryChildGetter();

            var dir = new DirectoryInfo(StartingDirectory);


            try
            {
                var seq = Extensions.BfsDetailed(dir, childGetter);

                seq.TakeWhile(x => x.Height <= Height).First();
            }
            catch (DirectoryNotFoundException e)
            {
                WriteError(new ErrorRecord(e, "DirectoryNotFound", ErrorCategory.ObjectNotFound, StartingDirectory));
                return;
            }

            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "UnknownError", ErrorCategory.NotSpecified, StartingDirectory));
                return;
            }

            WriteObject(result);
        }

        protected override void StopProcessing()
        {
            _cts.Cancel();
        }
    }
}
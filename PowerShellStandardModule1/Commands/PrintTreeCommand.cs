using System;
using System.IO;
using System.Management.Automation;
using System.Threading;

namespace PowerShellStandardModule1.Commands
{
    [Cmdlet(verbName: VerbsCommon.Get, nounName: "PrintTree")]
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

        [Parameter(
            Position = 1,
            HelpMessage = "The maximum depth to search. Defaults to 3. Negative numbers are rounded to 0."
        )]
        public int Height = 3;

        [Parameter(
            HelpMessage =
                "The maximum amount of lines an individual node should have. Defaults to int32 max. Negative numbers are rounded to 0."
        )]
        public int NodeWidth = Int32.MaxValue;

        [Parameter(
            HelpMessage =
                "How many results to process before stopping. Defaults to int32 max. Negative numbers are rounded to 0."
        )]
        public int Limit = Int32.MaxValue;

        [Parameter(
            HelpMessage =
                "The maximum amount of lines the entire tree should have. Defaults to int32 max. Negative numbers are rounded to 0."
        )]
        public int Width = Int32.MaxValue;

        [Parameter(
            HelpMessage =
                "Maximum width for the root node. Defaults to a negative value (-1), which will coerce it into the NodeWidth if this is the case."
        )]
        public int RootNodeWidth = -1;

        private CancellationTokenSource _cts = null!;

        protected override void BeginProcessing()
        {
            _cts = new CancellationTokenSource();
        }

        private static int Constrain(int value) => int.Clamp(value, 0, int.MaxValue);


        protected override void ProcessRecord()
        {
            string result;

            var instance = new PrintTreeRunner
            {
                TargetDirectory = new DirectoryInfo(StartingDirectory),
                Height = Constrain(Height),
                NodeWidth = Constrain(NodeWidth),
                Width = Constrain(Width),
                Take = Constrain(Limit),
                Token = _cts.Token,
                RootNodeWidth = RootNodeWidth
            };


            try
            {
                result = instance.Invoke();
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
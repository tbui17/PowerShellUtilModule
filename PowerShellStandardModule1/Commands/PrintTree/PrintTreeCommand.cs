using System;
using System.IO;
using System.Management.Automation;
using System.Threading;
using PowerShellStandardModule1.Delegates;
using PowerShellStandardModule1.Lib;
using NodeOrderer =
    System.Func<
        System.Collections.Generic.IEnumerable<PowerShellStandardModule1.Models.TreeNode<System.IO.DirectoryInfo>>,
        System.Collections.Generic.IEnumerable<PowerShellStandardModule1.Models.TreeNode<System.IO.DirectoryInfo>>>;

namespace PowerShellStandardModule1.Commands.PrintTree
{
    [Cmdlet(verbName: VerbsCommon.Get, nounName: "PrintTree")]
    [Alias("PrintTree")]
    [OutputType(typeof(string))]
    public class PrintTreeCommand : PSCmdlet
    {
        public const string Set1 = "Set1";
        public const string Set2 = "Set2";
        private string? _startingDirectory;

        [Parameter(
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The directory to start the search from. Defaults to the current directory."
        )]
        public string StartingDirectory
        {
            get =>
                _startingDirectory ??
                CurrentProviderLocation("FileSystem")
                   .ProviderPath;
            set => _startingDirectory = value;
        }


        [Parameter(
            Position = 1,
            HelpMessage = "The maximum depth to search. Defaults to 3. Negative numbers are rounded to 0."
        )]
        public int Height { get; set; } = 3;


        [Parameter(
            HelpMessage =
                "The maximum amount of lines an individual node should have. Defaults to int32 max. Negative numbers are rounded to 0."
        )]
        public int NodeWidth { get; set; } = Int32.MaxValue;


        [Parameter(
            HelpMessage =
                "How many results to process before stopping. Defaults to int32 max. Negative numbers are rounded to 0."
        )]
        public int Limit { get; set; } = Int32.MaxValue;


        [Parameter(
            HelpMessage =
                "The maximum amount of lines the entire tree should have. Defaults to int32 max. Negative numbers are rounded to 0."
        )]
        public int Width { get; set; } = Int32.MaxValue;

        [Parameter(
            HelpMessage =
                "Maximum width for the root node. Defaults to a negative value (-1), which will coerce it into the NodeWidth if this is the case."
        )]
        public int RootNodeWidth { get; set; } = -1;

        [Parameter(
            HelpMessage =
                """
                Type: Func<DirectoryInfo,object> A scriptblock to select the string to display for each node. It should return a serializable object at minimum. Defaults to the Name property.
                Properties of the DirectoryInfo object are available here
                https://learn.microsoft.com/en-us/dotnet/api/system.io.directoryinfo?view=net-8.0
                """
        )]
        public ScriptBlock? StringSelector { get; set; }

        [Parameter(
            HelpMessage = """
                          The property to sort by of a DirectoryInfo object. Available options are:
                          Name, CreationTime, LastAccessTime, LastWriteTime, Extension, Attributes, Exists, Root
                          Defaults to Name.
                          If an invalid option is selected, it will default to name.
                          """,
            ParameterSetName = Set1
        )]
        public string OrderBy { get; set; } = "Name";

        [Parameter(
            HelpMessage = "Sort order is ascending by default. Enable to sort in descending order.",
            ParameterSetName = Set1
        )]
        public SwitchParameter Descending { get; set; }

        [Parameter(
            HelpMessage =
                "Type: Func<DirectoryInfo, bool> Script block which determines whether or not to include a node. Defaults to always true."
        )]
        public ScriptBlock? Where { get; set; }


        private CancellationTokenSource _cts = new();

        public CancellationToken Token => _cts.Token;

        protected override void BeginProcessing()
        {
            _cts = new CancellationTokenSource();
        }

        private static int Constrain(int value) => int.Clamp(value, 0, int.MaxValue);

        private StringValueSelector CreateSelector() =>
            StringSelector is null
                ? PrintTreeService.DefaultStringValueSelector
                : StringSelector.ToStringValueSelector();

        private Func<DirectoryInfo, bool> CreateFilter()
        {
            return Where is null
                ? _ => true
                : Filter;

            bool Filter(DirectoryInfo info) =>
                Where
                   .InvokeWithValue(info)
                   .ValidateGetFirst<bool>();
        }


        protected override void ProcessRecord()
        {
            string result;


            var instance = new PrintTreeService
            {
                StartingDirectory = new DirectoryInfo(StartingDirectory),
                Height = Constrain(Height),
                NodeWidth = Constrain(NodeWidth),
                Width = Constrain(Width),
                Limit = Constrain(Limit),
                Token = _cts.Token,
                RootNodeWidth = RootNodeWidth,
                StringValueSelector = CreateSelector(),
                Filter = CreateFilter()
            };


            try
            {
                result = instance.Invoke();
            }
            catch (DirectoryNotFoundException e)
            {
                WriteError(
                    new ErrorRecord(
                        e, "DirectoryNotFound", ErrorCategory.ObjectNotFound,
                        StartingDirectory
                    )
                );
                return;
            }

            catch (PSInvalidOperationException e)
            {
                var record = new ErrorRecord(
                    e, "ScriptBlockError", ErrorCategory.InvalidOperation,
                    StringSelector
                );
                WriteError(record);
                return;
            }

            catch (Exception e)
            {
                WriteError(
                    new ErrorRecord(
                        e, "UnknownError", ErrorCategory.NotSpecified,
                        StartingDirectory
                    )
                );
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
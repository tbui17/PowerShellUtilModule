using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;

namespace PowerShellStandardModule1.Commands.Bfs;

[Cmdlet(verbName: VerbsCommon.Get, nounName: "Bfs")]
[Alias("Bfs")]
[OutputType(
    [typeof(DirectoryInfo)],
    ParameterSetName = [Set1]
)]
[OutputType(
    [typeof(FileInfo)],
    ParameterSetName = [Set2]
)]
public class BfsCommand : PSCmdlet
{
    public const string Set1 = "Set1";
    public const string Set2 = "Set2";

    [Parameter(
        Mandatory = false,
        Position = 0,
        HelpMessage = "The pattern to search for. Follows same conventions as other PowerShell commands.",
        ParameterSetName = Set1
    )]
    public string Pattern = "*";

    private string? _startingDirectory;

    [Parameter(
        Position = 1,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The directory to start the search from. Defaults to the current directory.",
        ParameterSetName = Set1
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
        HelpMessage = "Enable to be have case sensitive search.",
        ParameterSetName = Set1
    )]
    public SwitchParameter CaseSensitive;

    [Parameter(
        HelpMessage = "Enable to receive files in addition to directories.",
        ParameterSetName = Set1
    )]
    public SwitchParameter File;

    [Parameter(
        HelpMessage = "Max depth to search. Defaults to 10.",
        ParameterSetName = Set1
    )]
    public int Depth { get; set; } = 10;

    [Parameter(
        HelpMessage = "How many results to take before stopping. Defaults to 1. Negative numbers are rounded to 0.",
        ParameterSetName = Set1
    )]
    public int First = 1;

    [Parameter(
        HelpMessage =
            "The maximum amount of items to process, regardless if they match the pattern. Defaults to int32 max. Negative numbers rounded to 0.",
        ParameterSetName = Set1
    )]
    public int Limit = int.MaxValue;


    private CancellationTokenSource _cts = null!;

    protected override void BeginProcessing()
    {
        _cts = new CancellationTokenSource();
    }

    protected override void ProcessRecord()
    {
        var ignoreCase = !CaseSensitive;

        var runner = new BfsController(
            pattern: Pattern, startingDirectory: new DirectoryInfo(StartingDirectory), ignoreCase: ignoreCase,
            file: File, height: Depth, limit: Limit,
            take: First
        );


        try
        {
            IEnumerable<PSObject> res = runner
               .Invoke(_cts.Token)
               .Select(x => new PSObject(x.Value));

            foreach (var result in res)
            {
                WriteObject(result);
            }
        }
        catch (DirectoryNotFoundException e)
        {
            WriteError(
                new ErrorRecord(
                    e, "DirectoryNotFound", ErrorCategory.ObjectNotFound,
                    StartingDirectory
                )
            );
        }
        catch (OperationCanceledException)
        {
            var record = new InformationRecord(
                new
                {
                    message = "Operation was cancelled.",
                    directory = StartingDirectory,
                    pattern = Pattern,
                    caseSensitive = CaseSensitive
                },
                "BfsCommand.OperationCancelled"
            );
            WriteInformation(record);
        }
        catch (Exception e)
        {
            WriteError(
                new ErrorRecord(
                    e, "UnknownError", ErrorCategory.NotSpecified,
                    StartingDirectory
                )
            );
        }
    }

    protected override void StopProcessing()
    {
        _cts.Cancel();
    }
}
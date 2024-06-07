using System;
using System.IO;
using System.Management.Automation;
using System.Threading;
using PowerShellStandardModule1.Lib;

namespace PowerShellStandardModule1.Commands.PrintTree;

public partial class PrintTreeCommand : PSCmdlet
{
    private string? _startingDirectory;

    private CancellationTokenSource _cts = new();

    public CancellationToken Token => _cts.Token;


    private static int Constrain(int value) => int.Clamp(value, 0, int.MaxValue);

    private StringValueSelector CreateSelector() =>
        StringSelector is null
            ? PrintTreeService.DefaultStringValueSelector
            : ToStringValueSelector(StringSelector);

    public static StringValueSelector ToStringValueSelector(ScriptBlock stringSelector)
    {
        return StringSelector;

        string StringSelector(FileSystemInfoTreeNode node) =>
            stringSelector
               .InvokeWithValue(node.Value)
               .SerializePsResult();
    }

    private Func<FileSystemInfo, bool> CreateFilter()
    {
        return Where is null
            ? _ => true
            : Filter;

        bool Filter(FileSystemInfo info) =>
            Where
               .InvokeWithValue(info)
               .GetFirst<bool>();
    }

    protected override void BeginProcessing()
    {
        _cts = new CancellationTokenSource();
    }

    protected override void ProcessRecord()
    {
        var dir = new DirectoryInfo(StartingDirectory);
        if (!dir.Exists)
        {
            var e = new DirectoryNotFoundException($"Directory not found: {StartingDirectory}");
            WriteError(
                new ErrorRecord(
                    e, "DirectoryNotFound", ErrorCategory.ObjectNotFound,
                    StartingDirectory
                )
            );
            return;
        }

     
        var instance = new PrintTreeService(
            startingDirectory: new DirectoryInfo(StartingDirectory),
            height: Constrain(Depth),
            nodeWidth: Constrain(NodeChildren),
            width: Constrain(Width),
            limit: Constrain(Limit),
            token: Token,
            rootNodeWidth: RootNodeChildren,
            stringValueSelector: CreateSelector(),
            filter: CreateFilter(),
            orderBy: OrderBy.ToString(),
            descending: Descending,
            within: Within,
            file: File
        );
        
        
        


        try
        {
            var strResult = instance.Invoke();
            WriteObject(strResult);
        }

        catch (OperationCanceledException)
        {
            var record = new InformationRecord("Operation was cancelled.", "PrintTreeCommand.OperationCancelled");
            WriteInformation(record);
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

        catch (PSInvalidOperationException e)
        {
            var record = new ErrorRecord(
                e, "ScriptBlockError", ErrorCategory.InvalidOperation,
                StringSelector
            );
            WriteError(record);
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
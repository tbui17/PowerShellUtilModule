using System;
using System.IO;
using System.Management.Automation;
using System.Threading;
using PowerShellStandardModule1.Delegates;
using PowerShellStandardModule1.Lib;

namespace PowerShellStandardModule1.Commands.PrintTree;


public partial class PrintTreeCommand : PSCmdlet
{
    
    private const string Set1 = "Set1";
    private string? _startingDirectory;

    private CancellationTokenSource _cts = new();

    public CancellationToken Token => _cts.Token;

       

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

    protected override void BeginProcessing()
    {
        _cts = new CancellationTokenSource();
    }

    protected override void ProcessRecord()
    {
        


        var instance = new PrintTreeService
        {
            StartingDirectory = new DirectoryInfo(StartingDirectory),
            Height = Constrain(Height),
            NodeWidth = Constrain(NodeWidth),
            Width = Constrain(Width),
            Limit = Constrain(Limit),
            Token = Token,
            RootNodeWidth = RootNodeWidth,
            StringValueSelector = CreateSelector(),
            Filter = CreateFilter(),
            OrderBy = OrderBy,
            Descending = Descending
        };


        try
        {
            var strResult = instance.Invoke();
            WriteObject(strResult);
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
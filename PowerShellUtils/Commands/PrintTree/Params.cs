using System;
using System.Management.Automation;

namespace PowerShellStandardModule1.Commands.PrintTree;

[Cmdlet(verbName: VerbsCommon.Get, nounName: "PrintTree")]
[Alias("PrintTree")]
[OutputType(typeof(string))]
public partial class PrintTreeCommand
{
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
            Type: Func<FileSystemInfo,object> A scriptblock to select the string to display for each node. It should return a serializable object at minimum. Defaults to the Name property.
            Properties of the FileSystemInfo object are available here
            https://learn.microsoft.com/en-us/dotnet/api/system.io.filesysteminfo?view=net-8.0
            """
    )]
    public ScriptBlock? StringSelector { get; set; }

    [Parameter(
        HelpMessage = """
                      The property to sort by of a FileSystemInfo object. Available options are:
                      Name, CreationTime, LastAccessTime, LastWriteTime, Extension, Attributes
                      Defaults to Name.
                      If an invalid option is selected, it will default to name.
                      """
    )]
    public string OrderBy { get; set; } = "Name";

    [Parameter(HelpMessage = "Sort order is ascending by default. Enable to sort in descending order.")]
    public SwitchParameter Descending { get; set; }

    [Parameter(
        HelpMessage =
            "Type: Func<FileSystemInfo, bool> Script block which determines whether or not to include a directory. Defaults to always true."
    )]
    public ScriptBlock? Where { get; set; }

    [Parameter(
        HelpMessage =
            "Modifies behavior of Where clause. All ancestor directories of nodes that meet this filter will be included."
    )]
    public SwitchParameter Within { get; set; }

    [Parameter(HelpMessage = "Enable to also receive files. Related options such as StringSelector or Where will also receive files.")]
    public SwitchParameter File { get; set; }
    
    
}
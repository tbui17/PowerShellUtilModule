using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using PowerShellStandardModule1.Structs;
using static PowerShellStandardModule1.Extensions;

namespace PowerShellStandardModule1;

public class PrintTreeRunner
{
    public required DirectoryInfo TargetDirectory;
    public int Height = 3;
    public int Width = int.MaxValue;
    public int NodeWidth = int.MaxValue;
    public int Take = int.MaxValue;

    private static readonly Func<DirectoryInfo, IEnumerable<DirectoryInfo>> BaseGetter =
        ChildGetterFactory.CreateDirectoryChildGetter();


    public IEnumerable<PrintNode<DirectoryInfo>> RunWithFullResults()
    {
        // inject node width limiter (children count) into getter, which acts within bfs function
        var getterWithWidthLimitedNodes = BaseGetter.Compose(x => x.Take(NodeWidth));

        var treeNodes = BfsDetailed(TargetDirectory, getterWithWidthLimitedNodes)
           .TakeWhile(x => x.Height < Height)
           .Take(Take)
           .ToImmutableList(); // must materialize to populate children


        var printNodes = treeNodes
           .First()
           .ToPreOrderPrintNodes()
            // flattened list represents lines of output, truncate excess lines
           .Take(Width)
            // modify formatting of all involved nodes
           .Select(outerNode => outerNode with { StringValueSelector = node => node.Value.Name });

        return printNodes;
    }

    public string Run() => RunWithFullResults().ToTreeString();
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using Autofac;
using Autofac.Core.Activators;
using AutoMapper;
using PowerShellStandardModule1.Attributes;
using PowerShellStandardModule1.Delegates;
using PowerShellStandardModule1.Lib.Extensions;
using PowerShellStandardModule1.Models;
using KeyValuePair = System.Collections.Generic.KeyValuePair;

namespace PowerShellStandardModule1.Commands.PrintTree
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
                "Type: Func<DirectoryInfo,object> A scriptblock to select the string to display for each node. It should return a serializable object at minimum. Defaults to the Name property."
        )]
        public ScriptBlock? StringSelector { get; set; }

        private CancellationTokenSource _cts = new();
        public CancellationToken Token => _cts.Token;

        protected override void BeginProcessing()
        {
            _cts = new CancellationTokenSource();
        }

        private static int Constrain(int value) => int.Clamp(value, 0, int.MaxValue);

        private StringValueSelector CreateSelector()
        {
            if (StringSelector is null)
            {
                return PrintTreeService.DefaultStringValueSelector;
            }

            return SelectString;

            string SelectString(TreeNode<DirectoryInfo> x)
            {
                var res = StringSelector.Invoke(x.Value);
                if (res.Count is 0)
                {
                    var ex = new PSInvalidOperationException("The scriptblock did not return a result.");
                    var record = new ErrorRecord(
                        ex, "ScriptBlockError", ErrorCategory.InvalidOperation,
                        StringSelector
                    );
                    WriteError(record);
                    return "";
                }

                var first = res.First();
                if (first is null)
                {
                    var ex = new PSInvalidOperationException("The scriptblock returned a null result.");
                    var record = new ErrorRecord(
                        ex, "ScriptBlockError", ErrorCategory.InvalidOperation,
                        StringSelector
                    );
                    WriteError(record);
                    return "";
                }

                return first.ToString();
            }
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
                StringValueSelector = CreateSelector()
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

# Description
  
- Powershell utility modules for working with directory structures.  
  
# Installation 
```powershell  

Install-Module -Name PowerShellUtils  
Import-Module -Name PowerShellUtils  

```

# Get-PrintTree

## Description

- Similar to the tree command, this command prints a string visualization of the directory structure from the current working directory.
- Unlike the tree command, this offers querying capability, including the use of script blocks.
- Recursive search is abstracted into a flat sequence search.
```powershell
PowerShellStandardModule1
├── .idea
│   └── .idea.PowershellUtilsSolution
│       └── .idea
├── PowerShellStandardModule1
│   └── bin
│       └── Release
│           └── net8.0
│               └── win-x64
│                   └── publish
│                       └── PowerShellUtils
├── PowerShellUtils
│   ├── Attributes
│   ├── bin
│   │   ├── Debug
│   │   │   ├── net6.0
│   │   │   ├── net8.0
│   │   │   └── netstandard2.0
│   │   └── Release
│   │       └── net8.0
│   │           └── win-x64
│   │               └── publish
│   ├── Commands
│   │   ├── Bfs
│   │   ├── Fuzzy
│   │   ├── PrintTree
│   │   └── Sample
│   ├── Delegates
│   ├── Lib
│   │   └── Extensions
│   ├── Models
│   └── obj
│       ├── Debug
│       │   ├── net6.0
│       │   │   ├── ref
│       │   │   └── refint
│       │   ├── net8.0
│       │   │   ├── ref
│       │   │   └── refint
│       │   └── netstandard2.0
│       └── Release
│           └── net8.0
│               └── win-x64
│                   ├── ref
│                   └── refint
└── TestProject1
    ├── bin
    │   └── Debug
    │       └── net8.0
    │           ├── cs
    │           ├── de
    │           ├── es
    │           ├── fr
    │           ├── it
    │           ├── ja
    │           ├── ko
    │           ├── pl
    │           ├── pt-BR
    │           ├── ru
    │           ├── runtimes
    │           │   └── win
    │           │       └── lib
    │           │           ├── net6.0
    │           │           └── netstandard2.0
    │           ├── tr
    │           ├── zh-Hans
    │           └── zh-Hant
    ├── obj
    │   └── Debug
    │       └── net8.0
    │           ├── ref
    │           └── refint
    └── PrintTree

```
## Parameters

```
Get-Help Get-PrintTree -full

NAME
    Get-PrintTree

SYNTAX
    Get-PrintTree [[-StartingDirectory] <string>] [[-Depth] <int>] [-NodeWidth <int>] [-Limit <int>] [-Width <int>]
    [-RootNodeWidth <int>] [-StringSelector <scriptblock>] [-OrderBy <string>] [-Descending] [-Where <scriptblock>]
    [-Within] [-File] [<CommonParameters>]


PARAMETERS
    -Descending
        Sort order is ascending by default. Enable to sort in descending order.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -File
        Enable to also receive files. Related options such as StringSelector or Where will also receive files.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -Depth <int>
        The maximum depth to search. Defaults to 3. Negative numbers are rounded to 0.

        Required?                    false
        Position?                    1
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -Limit <int>
        How many results to process before stopping. Defaults to int32 max. Negative numbers are rounded to 0.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -NodeWidth <int>
        The maximum amount of lines an individual node should have. Defaults to int32 max. Negative numbers are
        rounded to 0.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -OrderBy <string>
        The property to sort by of a FileSystemInfo object. Available options are:
        Name, CreationTime, LastAccessTime, LastWriteTime, Extension, Attributes, ChildCount
        Defaults to Name.
        If an invalid option is selected, it will default to name.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -RootNodeWidth <int>
        Maximum width for the root node. Defaults to a negative value (-1), which will coerce it into the NodeWidth if
        this is the case.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -StartingDirectory <string>
        The directory to start the search from. Defaults to the current directory.

        Required?                    false
        Position?                    0
        Accept pipeline input?       true (ByValue, ByPropertyName)
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -StringSelector <scriptblock>
        Type: Func<FileSystemInfo,object> A scriptblock to select the string to display for each node. It should
        return a serializable object at minimum. Defaults to the Name property.
        Properties of the FileSystemInfo object are available here
        https://learn.microsoft.com/en-us/dotnet/api/system.io.filesysteminfo?view=net-8.0

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -Where <scriptblock>
        Type: Func<FileSystemInfo, bool> Script block which determines whether or not to include a directory. Defaults
        to always true.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -Width <int>
        The maximum amount of lines the entire tree should have. Defaults to int32 max. Negative numbers are rounded
        to 0.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -Within
        Modifies behavior of Where clause. All ancestor directories of nodes that meet this filter will be included.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    <CommonParameters>
        This cmdlet supports the common parameters: Verbose, Debug,
        ErrorAction, ErrorVariable, WarningAction, WarningVariable,
        OutBuffer, PipelineVariable, and OutVariable. For more information, see
        about_CommonParameters (https://go.microsoft.com/fwlink/?LinkID=113216).


INPUTS
    System.String


OUTPUTS
    System.String


ALIASES
    PrintTree


REMARKS
    None
```


## Usage

## Basic Usage


```Powershell
Get-PrintTree -Depth 20
PowerShellStandardModule1
├── .idea
│   ├── .idea.PowerShellStandardModule1
│   │   └── .idea
│   └── .idea.PowershellUtilsSolution
│       └── .idea
├── PowerShellStandardModule1
│   └── bin
│       └── Release
│           └── net8.0
│               └── win-x64
│                   └── publish
├── PowerShellUtils
│   ├── Attributes
│   ├── bin
│   │   ├── Debug
│   │   │   ├── net6.0
│   │   │   ├── net8.0
│   │   │   └── netstandard2.0
│   │   └── Release
│   │       └── net8.0
│   │           └── win-x64
│   │               └── publish
│   ├── Commands
│   │   ├── Bfs
│   │   ├── Fuzzy
│   │   ├── PrintTree
│   │   └── Sample
│   ├── Delegates
│   ├── Lib
│   │   └── Extensions
│   ├── Models
│   └── obj
│       ├── Debug
│       │   ├── net6.0
│       │   │   ├── ref
│       │   │   └── refint
│       │   ├── net8.0
│       │   │   ├── ref
│       │   │   └── refint
│       │   └── netstandard2.0
│       └── Release
│           └── net8.0
│               └── win-x64
│                   ├── ref
│                   └── refint
└── TestProject1
    ├── bin
    │   └── Debug
    │       └── net8.0
    │           ├── cs
    │           ├── de
    │           ├── es
    │           ├── fr
    │           ├── it
    │           ├── ja
    │           ├── ko
    │           ├── pl
    │           ├── pt-BR
    │           ├── ru
    │           ├── runtimes
    │           │   └── win
    │           │       └── lib
    │           │           ├── net6.0
    │           │           └── netstandard2.0
    │           ├── tr
    │           ├── zh-Hans
    │           └── zh-Hant
    ├── obj
    │   └── Debug
    │       └── net8.0
    │           ├── ref
    │           └── refint
    └── PrintTree
```


## Width, NodeWidth, RootNodeWidth

- Let's review the following behaviors as they are crucial:
	- RootNodeWidth constrains the amount of children the top level directory can have.
	- NodeWidth constrains the amount of children each node other than the top level directory can have.
	- If RootNodeWidth is negative, it defaults to the NodeWidth.
    - Width alters the amount of lines output.

- NodeWidth and RootNodeWidth directly impact the data returned from search results, and can therefore truncate node with children, drastically altering the Width of the tree.
- Width only alters the number of lines in the output, and is not dependent on node connections.

Note the following example, showing how results can drastically change with small changes to NodeWidth:

Original:

```PowerShell

printtree -depth 30000

PowerShellStandardModule1
├── .idea
│   └── .idea.PowershellUtilsSolution
│       └── .idea
├── PowerShellStandardModule1
│   └── bin
│       └── Release
│           └── net8.0
│               └── win-x64
│                   └── publish
│                       └── PowerShellUtils
├── PowerShellUtils
│   ├── Attributes
│   ├── bin
│   │   ├── Debug
│   │   │   ├── net6.0
│   │   │   ├── net8.0
│   │   │   └── netstandard2.0
│   │   └── Release
│   │       └── net8.0
│   │           └── win-x64
│   │               └── publish
│   ├── Commands
│   │   ├── Bfs
│   │   ├── Fuzzy
│   │   ├── PrintTree
│   │   └── Sample
│   ├── Lib
│   │   └── Extensions
│   ├── Models
│   └── obj
│       ├── Debug
│       │   ├── net6.0
│       │   │   ├── ref
│       │   │   └── refint
│       │   ├── net8.0
│       │   │   ├── ref
│       │   │   └── refint
│       │   └── netstandard2.0
│       └── Release
│           └── net8.0
│               └── win-x64
│                   ├── ref
│                   └── refint
└── TestProject1
    ├── bin
    │   └── Debug
    │       └── net8.0
    │           ├── cs
    │           ├── de
    │           ├── es
    │           ├── fr
    │           ├── it
    │           ├── ja
    │           ├── ko
    │           ├── pl
    │           ├── pt-BR
    │           ├── ru
    │           ├── runtimes
    │           │   └── win
    │           │       └── lib
    │           │           ├── net6.0
    │           │           └── netstandard2.0
    │           ├── tr
    │           ├── zh-Hans
    │           └── zh-Hant
    ├── obj
    │   └── Debug
    │       └── net8.0
    │           ├── ref
    │           └── refint
    ├── PrintTree
    └── Resources
        └── TestData
            ├── level1
            │   ├── level2
            │   │   ├── level3
            │   │   │   ├── level4
            │   │   │   │   └── level5
            │   │   │   │       ├── level6
            │   │   │   │       │   └── level7
            │   │   │   │       └── level6b
            │   │   │   └── level4b
            │   │   │       └── level5b
            │   │   └── level3b
            │   │       └── level4c
            │   └── level2b
            │       └── level3c
            ├── level1b
            │   └── level2c
            │       └── level3d
            ├── level1c
            │   └── level2d
            └── level1d
```


Filtered:

```powershell
printtree -width 26 -NodeWidth 2 -RootNodeWidth 300 -depth 99999
PowerShellStandardModule1
├── .idea
│   └── .idea.PowershellUtilsSolution
│       └── .idea
├── PowerShellStandardModule1
│   └── bin
│       └── Release
│           └── net8.0
│               └── win-x64
│                   └── publish
│                       └── PowerShellUtils
├── PowerShellUtils
│   ├── Attributes
│   └── bin
│       ├── Debug
│       │   ├── net6.0
│       │   └── net8.0
│       └── Release
│           └── net8.0
│               └── win-x64
│                   └── publish
└── TestProject1
    ├── bin
    │   └── Debug
    │       └── net8.0
    │           ├── cs
    
```

- Filtered, with 1 extra NodeWidth

```PowerShell
printtree -width 26 -NodeWidth 3 -RootNodeWidth 300 -depth 99999

PowerShellStandardModule1
├── .idea
│   └── .idea.PowershellUtilsSolution
│       └── .idea
├── PowerShellStandardModule1
│   └── bin
│       └── Release
│           └── net8.0
│               └── win-x64
│                   └── publish
│                       └── PowerShellUtils
├── PowerShellUtils
│   ├── Attributes
│   ├── bin
│   │   ├── Debug
│   │   │   ├── net6.0
│   │   │   ├── net8.0
│   │   │   └── netstandard2.0
│   │   └── Release
│   │       └── net8.0
│   │           └── win-x64
│   │               └── publish
│   └── Commands
│       ├── Bfs
│       ├── Fuzzy
│       └── PrintTree
```
- There is now an additional node, whose contents by line count towards the Width limit.

## Where

- Note that orphaned nodes are not included by default with where queries, since a child node cannot be accessed if the parent node gets filtered out prematurely. 


```powershell
printtree -Depth 20 -where {$_.name -like "*Release*"}

PowerShellStandardModule1
```

- A where query without a -within flag is useful for exclusion queries

```powershell
Get-PrintTree -Depth 20 -Where {$_.name -notlike "*.0*"}
PowerShellStandardModule1
├── .idea
│   ├── .idea.PowerShellStandardModule1
│   │   └── .idea
│   └── .idea.PowershellUtilsSolution
│       └── .idea
├── PowerShellStandardModule1
│   └── bin
│       └── Release
├── PowerShellUtils
│   ├── Attributes
│   ├── bin
│   │   ├── Debug
│   │   └── Release
│   ├── Commands
│   │   ├── Bfs
│   │   ├── Fuzzy
│   │   ├── PrintTree
│   │   └── Sample
│   ├── Delegates
│   ├── Lib
│   │   └── Extensions
│   ├── Models
│   └── obj
│       ├── Debug
│       └── Release
└── TestProject1
    ├── bin
    │   └── Debug
    ├── obj
    │   └── Debug
    └── PrintTree
```

## Within


- This flag essentially modifies the where clause to go bottom-up. From a given node that meets a criteria, get its ancestors (until it reaches the starting directory).

```powershell
printtree -Depth 20 -Where {$_.name -eq "PrintTree"} -within

PowerShellStandardModule1
├── PowerShellUtils
│   └── Commands
│       └── PrintTree
└── TestProject1
    └── PrintTree
    
```


## File

- In addition to showing files, this also automatically applies relevant query options to the files.

```powershell
printtree -file -Depth 10 -where {$_.name -eq "TestFiles.cs"} -within -StringSelector {"[$($_.GetType().Name)] " + $_.name}
[DirectoryInfo] PowerShellStandardModule1
└── [DirectoryInfo] TestProject1
    └── [DirectoryInfo] PrintTree
        └── [FileInfo] TestFiles.cs
```


## Advanced

- Imagine you are trying to get an overview of the locations of build folders in relation to a project root, but you do not want to get inundated with excessive results. You know that the entry points are somewhere within the top level and there are only a few of them, but there are many to look through, and you do not want that to dictate the length of your search results. You remember vaguely that that the folders had a version number in them containing ".0". You also want to get its last write time. 

```powershell
Get-PrintTree -Depth 9 -NodeWidth 3 -Width 35 -RootNodeWidth 99999999 -Limit 50000 -Where {$_.Name -Like "*.0*"} -Within -StringSelector {"[$($_.Name)] - $($_.LastWriteTime.ToShortDateString()) $($_.LastWriteTime.ToShortTimeString())"} -OrderBy LastWriteTime -Desc
[PowerShellStandardModule1] - 6/4/2024 8:05 PM
├── [TestProject1] - 6/5/2024 5:50 PM
│   ├── [obj] - 6/4/2024 8:07 PM
│   │   └── [Debug] - 5/20/2024 6:18 AM
│   │       └── [net8.0] - 6/5/2024 10:46 PM
│   └── [bin] - 5/20/2024 6:18 AM
│       └── [Debug] - 5/20/2024 6:18 AM
│           └── [net8.0] - 6/5/2024 10:46 PM
│               └── [runtimes] - 5/20/2024 6:09 PM
│                   └── [win] - 5/20/2024 6:09 PM
│                       └── [lib] - 5/21/2024 2:32 PM
│                           ├── [net6.0] - 5/21/2024 2:32 PM
│                           └── [netstandard2.0] - 5/20/2024 6:09 PM
├── [PowerShellUtils] - 6/4/2024 8:06 PM
│   ├── [obj] - 6/4/2024 8:07 PM
│   │   ├── [Release] - 5/29/2024 12:44 PM
│   │   │   └── [net8.0] - 5/29/2024 12:44 PM
│   │   └── [Debug] - 5/20/2024 5:46 AM
│   │       ├── [net8.0] - 6/4/2024 10:19 PM
│   │       ├── [net6.0] - 5/20/2024 5:44 AM
│   │       └── [netstandard2.0] - 5/20/2024 4:47 AM
│   └── [bin] - 5/29/2024 12:44 PM
│       ├── [Release] - 5/29/2024 12:44 PM
│       │   └── [net8.0] - 5/29/2024 12:44 PM
│       └── [Debug] - 5/20/2024 5:46 AM
│           ├── [net8.0] - 6/5/2024 10:46 PM
│           ├── [net6.0] - 5/20/2024 5:44 AM
│           └── [netstandard2.0] - 5/20/2024 5:41 AM
└── [PowerShellStandardModule1] - 6/4/2024 8:05 PM
    └── [bin] - 6/4/2024 8:05 PM
        └── [Release] - 6/4/2024 8:05 PM
            └── [net8.0] - 6/4/2024 8:05 PM
```

# Get-Bfs
## Description

- This is essentially ls -r
- Searches breadth first rather than depth first
- Good for searches where you are already close to the target of interest

## Parameters
```powershell
get-help get-bfs -full

NAME
    Get-Bfs

SYNTAX
    Get-Bfs [[-Pattern] <string>] [[-StartingDirectory] <string>] [-Depth <int>] [-CaseSensitive] [-File] [-First <int>] [-Limit <int>] [<CommonParameters>]


PARAMETERS
    -CaseSensitive
        Enable to be have case sensitive search.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -Depth <int>
        Max depth to search. Defaults to 100.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -File
        Enable to receive files in addition to directories.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -First <int>
        How many results to take before stopping. Defaults to 1. Negative numbers are rounded to 0.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -Limit <int>
        The maximum amount of items to process, regardless if they match the pattern. Defaults to int32 max. Negative numbers rounded to 0.

        Required?                    false
        Position?                    Named
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -Pattern <string>
        The pattern to search for. Follows same conventions as other PowerShell commands.

        Required?                    false
        Position?                    0
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -StartingDirectory <string>
        The directory to start the search from. Defaults to the current directory.

        Required?                    false
        Position?                    1
        Accept pipeline input?       true (ByValue, ByPropertyName)
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    <CommonParameters>
        This cmdlet supports the common parameters: Verbose, Debug,
        ErrorAction, ErrorVariable, WarningAction, WarningVariable,
        OutBuffer, PipelineVariable, and OutVariable. For more information, see
        about_CommonParameters (https://go.microsoft.com/fwlink/?LinkID=113216).


INPUTS
    System.String


OUTPUTS
    System.IO.FileSystemInfo


ALIASES
    Bfs


REMARKS
    None
```


## Usage




``` powershell
bfs *tree* -First 10 | rvpa -Relative

.\TestProject1\PrintTree
.\PowerShellUtils\Commands\PrintTree


```



# Select-Fuzzy

## Description  
- Fuzzy matching between two strings. Returns a score based on similarity.  
- Information for the available strategies can be found in the dependency https://github.com/JakeBayer/FuzzySharp

## Parameters
```powershell
get-help select-fuzzy -full

NAME
    Select-Fuzzy

SYNTAX
    Select-Fuzzy [-String1] <string> [-String2] <string> [[-Strategy] <string>] [<CommonParameters>]


PARAMETERS
    -Strategy <string>
        Strategy to use for fuzzy matching. Defaults to 'Ratio', and will use this option if an invalid option is selected.
        Options: "PartialRatio", "PartialTokenAbbreviationRatio", "PartialTokenDifferenceRatio",
        "PartialTokenInitialismRatio", "PartialTokenSetRatio", "PartialTokenSortRatio", "Ratio",
        "TokenAbbreviationRatio", "TokenDifferenceRatio", "TokenInitialismRatio", "TokenSetRatio", "TokenSortRatio",
        "WeightedRatio"

        Required?                    false
        Position?                    2
        Accept pipeline input?       false
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -String1 <string>
        The first string to compare.

        Required?                    true
        Position?                    0
        Accept pipeline input?       true (ByValue, ByPropertyName)
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    -String2 <string>
        The second string to compare.

        Required?                    true
        Position?                    1
        Accept pipeline input?       true (ByValue, ByPropertyName)
        Parameter set name           (All)
        Aliases                      None
        Dynamic?                     false
        Accept wildcard characters?  false

    <CommonParameters>
        This cmdlet supports the common parameters: Verbose, Debug,
        ErrorAction, ErrorVariable, WarningAction, WarningVariable,
        OutBuffer, PipelineVariable, and OutVariable. For more information, see
        about_CommonParameters (https://go.microsoft.com/fwlink/?LinkID=113216).


INPUTS
    System.String


OUTPUTS
    PowerShellStandardModule1.Models.FuzzyResult


ALIASES
    Fuzzy


REMARKS
    None
```
## Usage
```powershell
select-fuzzy 'hello world' world tokendifferenceratio

String1     String2 Score
-------     ------- -----
hello world world      67
```

# Copyright (c) Microsoft Corporation.  All rights reserved.
function script:GetTableInfo([string]$target)
{
    if ( [String]::IsNullOrEmpty($target) )
    {
        $target = join-path $script:scriptPath "UnmanagedToManaged\StorageGenerator\Data\windows.xml"
    }
    else
    {
        $target= resolve-path $target
    }

    $ds = new-object System.Data.DataSet
    $ds.ReadXml($target) | out-null
    foreach ( $table in $ds.Tables )
    {
        if ( $table.Columns["Name"] -eq $null )
        {
            continue;
        }

        $o = new-object psobject
        $o | add-member -memberType NoteProperty -name "Name" $table.TableName
        $o | add-member -memberType NoteProperty -name "Count" $table.Rows.Count
        $o | add-member -memberType NoteProperty -name "Table" $table
        write-output $o
    }
}

function script:DiffProcedureRow($prefix, $left, $right)
{
    $lVal = $left["DllName"]
    $rVal = $right["DllName"]
    if ( $lVal -ne $rVal )
    {
        write-output ("{0}Changed,{1},{2},{3}" -f $prefix,$left["Name"],$lVal,$rVal)
    }
}

function script:DiffTableInfosCore($prefix, $leftInfo, $rightInfo, [string]$filter)
{
    foreach ( $cur in ($leftInfo | ? { ($filter -eq "") -or ($_.Name -eq $filter)} ))
    {
        $leftTable = $cur.Table
        $rightTable = $rightInfo | ? { $_.Name -eq $leftTable.TableName } 
        $rightTable = $rightTable.Table
        if ( $rightTable -eq $null )
        {
            write-output ("Error: Could not find table {0}" -f $leftTable.TableName)
        }
        else
        {
            [int]$leftAnonCount = 0
            [int]$rightAnonCount = 0
            foreach ( $leftRow in $leftTable.Rows )
            {
                $name = $leftRow["Name"]
                if  ( $name -like "Anonymous*" )
                {
                    $leftAnonCount++;
                    continue;
                }

                $found = $rightTable.Select("Name='$name'");
                if ( $found.Length -eq 0 )
                {
                    write-output ("{0},{1},{2}" -f $prefix,$cur.Name,$name)
                }
                else
                {
                    switch ($leftTable.TableName)
                    {
                        "Procedure" { DiffProcedureRow $prefix $leftRow $found[0]; break } 
                    }
                }
            }

            foreach ( $rightRow in $rightTable.Rows )
            {
                $name = $rightRow["Name"]
                if  ( $name -like "Anonymous*" )
                {
                    $rightAnonCount++;
                }
            }

            if ( $leftAnonCount -ne $rightAnonCount )
            {
                $diff = $rightAnonCount - $leftAnonCount 
                write-output ("{0},{1},<anonymous>({2})" -f $prefix,$cur.Name,$diff)
            }
        }
    }

}

function script:DiffTableInfos($oldInfo, $newInfo, [string]$tableName)
{
    DiffTableInfosCore "Add" $newInfo $oldInfo $tableName
    DiffTableInfosCore "Remove" $oldInfo $newInfo $tableName
}

[string]$script:basePath = split-path -parent $MyInvocation.MyCommand.Definition
[string]$script:oldPath = join-path $basePath "Data\windows_old.xml" 
[string]$script:newPath = join-path $basePath "Data\windows.xml" 
[string]$script:tableName = ""

if ( $args.Leggth -gt 0 )
{
    $oldPath = resolve-path $args[0];
}

if ( $args.Length -gt 1 )
{
    $newPath = resolve-path $args[1]
}

if ( $args.Length -gt 2 )
{
    $tableName = $args[2]
}

write-output "Old: $oldPath"
write-output "New: $newPath"
DiffTableInfos $(GetTableInfo $oldPath) $(GetTableInfo $newPath) $tableName 

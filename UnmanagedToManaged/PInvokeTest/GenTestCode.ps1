$script:scriptPath = split-path -parent $MyInvocation.MyCommand.Definition 

function script:GenMyTestCode()
{
    $envMap = SaveComplus

    $target = "PInvokeTestLib.vb"
    $tempFile = join-path $env:TMP "NativeMethods.vb"
    pushd $scriptPath 
    sd edit $target
    if ( test-path $target ) { del $target }
    if ( test-path $tempFile ) { del $tempFile }
    & ..\ConsoleTool\bin\debug\sigimp.exe /lib:..\..\debug\PInvokeTestLib.dll /useSdk:yes /out:$tempFile ..\PInvokeTestLib\PInvokeTestLib.h
    echo "Namespace PInvokeTestLib" > $target
    gc $tempFile | out-file $target -append
    echo "End Namespace" >> $target

    popd

    RestoreComplus $envMap
}

function script:GenPlatformTestCode()
{
    $envMap = SaveComplus

    pushd ( join-path $script:scriptPath "..") 
    $target = "StorageGenerator\Data\gen.exe" 
    $vbc = join-path $env:SystemRoot "Microsoft.Net\Framework\v2.0.50727\vbc.exe"
    if ( test-path $target ){ del $target }
    copy Engine\bin\debug\sigimplib.dll StorageGenerator\Data\sigimplib.dll
    & $vbc PInvokeTest\_Scripts.vb /out:$target /r:StorageGenerator\Data\sigimplib.dll
    & sd edit "PInvokeTest\Generated.vb"
    & $target "PInvokeTest\Generated.vb"
    popd

    RestoreComplus $envMap
}

function script:SaveComplus()
{
    $map = @{}
    foreach ( $cur in (gci env:\COMPLUS*))
    {
        $map[$cur.Name] = $cur.Value
        rm "env:\$($cur.Name)"
    }
    return $map
}

function script:RestoreComplus($map)
{
    foreach ( $cur in $map.Keys )
    {
        sc "env:\$cur" $map[$cur]
    }
}


GenMyTestCode
GenPlatformTestCode


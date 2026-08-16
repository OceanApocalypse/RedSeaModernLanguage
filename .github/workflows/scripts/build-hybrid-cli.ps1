<#
.SYNOPSIS
This script is used to build RSML's CLI under the hybrid tool aproach.
Learn more about hybrid tools here: https://github.com/richlander/dotnet10-hybrid-tool/

.EXAMPLE
.\build-hybrid-cli.ps1 ${{ matrix.rid-os }} ${{ matrix.arch }}

.NOTES
Created by Matthew. Maintained by Ocean Apocalypse.
This is an helper script and is not meant to be used by the end user.
Please note that on Linux, this script installs the necessary cross-compilation
tools, as the workflow runs on a x64 machine, while it's necessary to build
binaries for ARM and ARM64.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$OperatingSystemName,
    [Parameter(Mandatory = $true)]
    [string]$ProcessorArchitecture
)

$ProjectName = "RSML.CLI"
$rid = "$OperatingSystemName-$ProcessorArchitecture"
$outputDir = "./dist/hybrid-$OperatingSystemName-$ProcessorArchitecture"

$dotnetArgs = @(
    "./src/$ProjectName/$ProjectName.csproj",
    "-c", "Release",
    "-o", $outputDir
)

if ($OperatingSystemName -eq "linux") {
    if ($ProcessorArchitecture -eq "arm") {
        $dotnetArgs += "-p:ObjCopyName=arm-linux-gnueabihf-objcopy"
        $dotnetArgs += "-p:LinkerFlavor=lld"
    }
    elseif ($ProcessorArchitecture -eq "arm64") {
        $dotnetArgs += "-p:ObjCopyName=aarch64-linux-gnu-objcopy"
        $dotnetArgs += "-p:LinkerFlavor=lld"
    }
}

dotnet pack -r $rid -p:IsPackable=true @dotnetArgs

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

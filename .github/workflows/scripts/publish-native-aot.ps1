<#
.SYNOPSIS
This script is used to publish a package using Native AOT.

.DESCRIPTION
This script publishes the given package using Native AOT compilation
aimed towards the given system, defined by OperatingSystemName and ProcessorArchitecture.

.EXAMPLE
.\publish-native-aot.ps1 $ProjectName ${{ matrix.rid-os }} ${{ matrix.arch }}

.NOTES
Created by Matthew. Maintained by Ocean Apocalypse.
This is an helper script and is not meant to be used by the end user.
Please note that on Linux, this script installs the necessary cross-compilation
tools, as the workflow runs on a x64 machine, while it's necessary to build
binaries for ARM and ARM64.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectName,
    [Parameter(Mandatory = $true)]
    [string]$OperatingSystemName,
    [Parameter(Mandatory = $true)]
    [string]$ProcessorArchitecture
)

$rid = "$OperatingSystemName-$ProcessorArchitecture"
$outputDir = "./dist/$ProjectName-$rid"

$dotnetArgs = @(
    "./src/$ProjectName/$ProjectName.csproj",
    "-c", "Release",
    "-r", $rid,
    "-o", $outputDir,
    "--self-contained", "true"
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

dotnet publish @dotnetArgs

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

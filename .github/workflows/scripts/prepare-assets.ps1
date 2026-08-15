<#
.SYNOPSIS
This script is used to prepare the assets for release.

.NOTES
Created by Matthew. Maintained by Ocean Apocalypse.
This is an helper script and is not meant to be used by the end user.
#>

New-Item -ItemType Directory -Force -Path "./release-assets" | Out-Null
Set-Location "./all-binaries"

foreach ($dir in (Get-ChildItem -Directory)) {
    $dirName = $dir.Name
    $destPath = "../release-assets/$dirName"

    if ($dirName -like "*-win-*") {
        Write-Host "Windows: $destPath.zip"
        Compress-Archive -Path "$dirName/*" -DestinationPath "$destPath.zip" -Force
    }
    else {
        Write-Host "Linux/macOS: $destPath.tar.gz"
        tar -czf "$destPath.tar.gz" -C "$dirName" .
    }
}

Set-Location ..

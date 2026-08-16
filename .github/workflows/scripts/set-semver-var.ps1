<#
.SYNOPSIS
This script is used from within a workflow to set the SEMVER environment variable
used to decide which version is to be analyzed and released.

.DESCRIPTION
This script extracts and sets the SEMVER environment variable from the shared
project properties, if the workflow was dispatched manually, or from the tag name
if dispatched via a tag push.

.EXAMPLE
.\set-semver-var.ps1 ${{ github.ref_type }} ${{ github.ref_name }}

.NOTES
Created by Matthew. Maintained by Ocean Apocalypse.
This is an helper script and is not meant to be used by the end user.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$RefType,
    [Parameter(Mandatory = $true)]
    [string]$RefName
)

if ($RefType -eq 'tag') {
    $semver = $RefName.Substring(1)
}
else {
    [xml]$xml = Get-Content ".\props\Versioning.props"
    $versionPrefix = $xml.SelectSingleNode("//VersionPrefix")?.InnerText
    $versionSuffix = $xml.SelectSingleNode("//VersionSuffix")?.InnerText
    $semver = if ($versionSuffix) { "$versionPrefix$versionSuffix" } else { $versionPrefix }
}

"semver=$semver" | Out-File -FilePath $env:GITHUB_OUTPUT -Encoding utf8 -Append

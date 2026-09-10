<#
.SYNOPSIS
  Replaces the iOS and Mac Catalyst lib/ folders of a packed WebRTCme.nupkg with ones built on
  macOS.

.DESCRIPTION
  Mac Catalyst's WebRTC.framework has to be a versioned bundle - Versions/A plus symlinks - or
  macOS refuses to codesign an app that embeds it. Git on Windows cannot check symlinks out, and a
  zip written on Windows cannot record them, so the .resources.zip produced by a Windows build
  carries a flat framework and its Mac Catalyst slice is unusable. No single machine can build all
  five slices: macOS cannot build net10.0-windows either.

  So the Apple slices are built on macOS and swapped in here.

  The one rule that matters: the .resources.zip files are copied byte for byte and never unpacked.
  The symlinks live *inside* them, and rewriting one on Windows would flatten the framework again -
  reintroducing exactly the bug this works around.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $Package,
    [Parameter(Mandatory)][string] $AppleArtifactDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

if (-not (Test-Path $Package))                { throw "Package not found: $Package" }
if (-not (Test-Path $AppleArtifactDirectory)) { throw "Apple artifacts not found: $AppleArtifactDirectory" }

# The artifact is <tfm-dir>/<files> as produced on macOS, e.g. net10.0-ios/WebRTCme.dll.
$replacements = @{}
foreach ($file in Get-ChildItem $AppleArtifactDirectory -Recurse -File) {
    $tfm = $file.Directory.Name
    $replacements["$tfm/$($file.Name)"] = $file.FullName
}
if ($replacements.Count -eq 0) { throw "No files found under $AppleArtifactDirectory" }

Write-Host "Apple artifacts to merge:"
$replacements.Keys | Sort-Object | ForEach-Object { Write-Host "  $_" }

$source = [System.IO.Path]::GetFullPath($Package)
$target = "$source.merged"
$used   = New-Object System.Collections.Generic.HashSet[string]

$in  = [System.IO.Compression.ZipFile]::OpenRead($source)
try {
    $out = [System.IO.Compression.ZipFile]::Open($target, 'Create')
    try {
        foreach ($entry in $in.Entries) {
            $name = $entry.FullName.Replace('\', '/')

            # lib/net10.0-ios26.0/WebRTCme.dll -> match on the TFM's base name plus the file name,
            # because the packed folder carries a platform version the artifact directory does not.
            $swap = $null
            if ($name -like 'lib/*') {
                $parts = $name.Split('/')
                if ($parts.Length -eq 3) {
                    $packedTfm = $parts[1]     # e.g. net10.0-ios26.0
                    $leaf      = $parts[2]
                    foreach ($key in $replacements.Keys) {
                        $keyTfm, $keyLeaf = $key.Split('/', 2)
                        if ($leaf -eq $keyLeaf -and $packedTfm.StartsWith($keyTfm)) {
                            $swap = $replacements[$key]
                            [void]$used.Add($key)
                            break
                        }
                    }
                }
            }

            $new = $out.CreateEntry($name, [System.IO.Compression.CompressionLevel]::Optimal)
            $writer = $new.Open()
            try {
                if ($swap) {
                    Write-Host "  replacing $name"
                    $bytes = [System.IO.File]::ReadAllBytes($swap)   # byte for byte; never unpacked
                    $writer.Write($bytes, 0, $bytes.Length)
                }
                else {
                    $reader = $entry.Open()
                    try { $reader.CopyTo($writer) } finally { $reader.Dispose() }
                }
            }
            finally { $writer.Dispose() }
        }
    }
    finally { $out.Dispose() }
}
finally { $in.Dispose() }

# Every artifact must have landed somewhere. One that matched nothing means the packed folder names
# changed - and silently shipping the Windows-built Apple slice is the failure this exists to stop.
$unused = $replacements.Keys | Where-Object { -not $used.Contains($_) }
if ($unused) {
    Remove-Item $target -Force
    throw "These Apple artifacts matched no entry in the package: $($unused -join ', ')"
}

Move-Item $target $source -Force
Write-Host ""
Write-Host "Merged $($used.Count) Apple file(s) into $([System.IO.Path]::GetFileName($source))."

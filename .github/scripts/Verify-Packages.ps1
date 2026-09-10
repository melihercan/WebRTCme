<#
.SYNOPSIS
  Checks that the two WebRTCme packages contain what they are supposed to.

.DESCRIPTION
  `dotnet pack` drops things quietly. Build output whose extension is not on
  AllowedOutputExtensionsInPackageBuildOutputFolder is discarded without a warning, so a package
  can lose the Android Java classes or an Apple framework and still restore, compile and pass a
  consumer's build - it fails later, on a device, when the native side is not there. This asserts
  the payloads are present before anything is published.

  It also asserts that no internal project leaked into the dependency groups. Those are folded
  into the packages with PrivateAssets="all"; if that is ever dropped, pack records a dependency
  on a package that was never published, at version 1.0.0, and every consumer fails to restore.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $PackageDirectory,
    [string] $Version,

    # Release-only. Asserts the Mac Catalyst framework inside the package is a versioned bundle,
    # which it can only be if that slice was built on macOS. A Windows build produces a flat one
    # that macOS refuses to codesign - and nothing else in the package looks any different, which
    # is why this has to be checked rather than assumed.
    [switch] $RequireAppleNativeLayout
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

# With a version, the exact file is named. Without one, the id is matched against whatever
# version happens to be there - but not loosely: "WebRTCme.*.nupkg" would also match
# WebRTCme.Middleware, so the part after the id has to start with a digit.
function Resolve-Package {
    param([string] $Directory, [string] $Id, [string] $Version)

    if ($Version) { return Join-Path $Directory "$Id.$Version.nupkg" }

    $match = Get-ChildItem -Path $Directory -Filter "$Id.*.nupkg" -ErrorAction SilentlyContinue |
             Where-Object { $_.Name -match "^$([regex]::Escape($Id))\.\d" } |
             Select-Object -First 1
    if ($match) { return $match.FullName }
    return Join-Path $Directory "$Id.nupkg"   # reported as missing below
}

# Folder names under lib/ carry a platform version the projects never state (net10.0-android
# becomes net10.0-android36.0), so these are matched as patterns rather than literals.
$expected = @{
    'WebRTCme' = @{
        Entries = @(
            'lib/net10.0/WebRTCme.dll'
            'lib/net10.0/WebRTCme.Bindings.Blazor.dll'
            'lib/net10.0-android*/WebRTCme.dll'
            'lib/net10.0-android*/WebRTCme.Bindings.Maui.Android.dll'
            'lib/net10.0-android*/libwebrtc.aar'                                  # classes.jar + the .so
            'lib/net10.0-ios*/WebRTCme.dll'
            'lib/net10.0-ios*/WebRTCme.Bindings.Maui.iOS.dll'
            'lib/net10.0-ios*/WebRTCme.Bindings.Maui.iOS.resources.zip'           # WebRTC.xcframework
            'lib/net10.0-maccatalyst*/WebRTCme.dll'
            'lib/net10.0-maccatalyst*/WebRTCme.Bindings.Maui.MacCatalyst.dll'
            'lib/net10.0-maccatalyst*/WebRTCme.Bindings.Maui.MacCatalyst.resources.zip'
            'lib/net10.0-windows*/WebRTCme.dll'
            'lib/net10.0-windows*/WebRTCme.Bindings.Maui.Windows.dll'
            'runtimes/win-x64/native/WebRtcInterop.dll'
            'staticwebassets/JsInterop.js'                                        # _content/WebRTCme/
        )
        AllowedWebRTCmeDependencies = @()
    }
    'WebRTCme.Middleware' = @{
        Entries = @(
            'lib/net10.0/WebRTCme.Middleware.dll'
            'lib/net10.0/WebRTCme.Connection.dll'
            'lib/net10.0/WebRTCme.Connection.MediaSoup.dll'
            'lib/net10.0/WebRTCme.Connection.Signaling.dll'
            'lib/net10.0/WebRTCme.Connection.Signaling.Proxy.dll'
            'lib/net10.0-android*/WebRTCme.Middleware.dll'
            'lib/net10.0-android*/WebRTCme.Connection.dll'
            'lib/net10.0-ios*/WebRTCme.Middleware.dll'
            'lib/net10.0-ios*/WebRTCme.Connection.dll'
            'lib/net10.0-maccatalyst*/WebRTCme.Middleware.dll'
            'lib/net10.0-windows*/WebRTCme.Middleware.dll'
        )
        AllowedWebRTCmeDependencies = @('WebRTCme')
    }
}

$failures = New-Object System.Collections.Generic.List[string]

foreach ($id in $expected.Keys | Sort-Object) {
    $path = Resolve-Package -Directory $PackageDirectory -Id $id -Version $Version
    if (-not (Test-Path $path)) {
        $failures.Add("$id : package not found at $path")
        continue
    }

    $zip = [System.IO.Compression.ZipFile]::OpenRead($path)
    try {
        $names = $zip.Entries | ForEach-Object { $_.FullName.Replace('\', '/') }

        foreach ($pattern in $expected[$id].Entries) {
            if (-not ($names | Where-Object { $_ -like $pattern })) {
                $failures.Add("$id : missing $pattern")
            }
        }

        $nuspec = $zip.Entries | Where-Object { $_.FullName -like '*.nuspec' } | Select-Object -First 1
        $reader = New-Object System.IO.StreamReader($nuspec.Open())
        try { $xml = [xml]$reader.ReadToEnd() } finally { $reader.Dispose() }

        $deps = $xml.package.metadata.dependencies.group.dependency |
                Where-Object { $_ -and $_.id -like 'WebRTCme*' } |
                ForEach-Object { $_.id } |
                Sort-Object -Unique
        $allowed = $expected[$id].AllowedWebRTCmeDependencies
        foreach ($d in $deps) {
            if ($allowed -notcontains $d) {
                $failures.Add("$id : unexpected dependency on $d - a folded project lost PrivateAssets=""all""")
            }
        }

        $size = '{0:N0}' -f (Get-Item $path).Length
        Write-Host "$id : $($names.Count) entries, $size bytes, WebRTCme deps: $(if ($deps) { $deps -join ', ' } else { 'none' })"
    }
    finally {
        $zip.Dispose()
    }
}

if ($RequireAppleNativeLayout) {
    $webrtcme = Resolve-Package -Directory $PackageDirectory -Id 'WebRTCme' -Version $Version
    $zip = [System.IO.Compression.ZipFile]::OpenRead($webrtcme)
    try {
        $entry = $zip.Entries |
                 Where-Object { $_.FullName -like 'lib/net10.0-maccatalyst*/*.resources.zip' } |
                 Select-Object -First 1
        if (-not $entry) {
            $failures.Add("WebRTCme : no Mac Catalyst .resources.zip to inspect")
        }
        else {
            # Read the inner zip's central directory without extracting it - extracting on Windows
            # would lose the symlinks, which are the whole point.
            $ms = New-Object System.IO.MemoryStream
            $stream = $entry.Open()
            try { $stream.CopyTo($ms) } finally { $stream.Dispose() }
            $ms.Position = 0
            $inner = New-Object System.IO.Compression.ZipArchive($ms, [System.IO.Compression.ZipArchiveMode]::Read)
            try {
                $names = $inner.Entries | ForEach-Object { $_.FullName }
                $versioned = $names | Where-Object { $_ -like '*WebRTC.framework/Versions/A/WebRTC' }
                if ($versioned) {
                    Write-Host "WebRTCme : Mac Catalyst framework is versioned (built on macOS)"
                }
                else {
                    $failures.Add("WebRTCme : Mac Catalyst framework is not versioned - that slice was built on Windows and macOS will refuse to codesign it")
                }
            }
            finally { $inner.Dispose(); $ms.Dispose() }
        }
    }
    finally { $zip.Dispose() }
}

if ($failures.Count -gt 0) {
    Write-Host ""
    Write-Host "Package verification failed:" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    exit 1
}

Write-Host ""
Write-Host "Package verification passed."

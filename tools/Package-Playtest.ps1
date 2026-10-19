param([ValidatePattern('^[a-zA-Z0-9.-]+$')][string]$Version = '0.3.0-3d-playtest')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$buildRoot = Join-Path $projectRoot 'Builds\Windows'
if (-not (Test-Path -LiteralPath (Join-Path $buildRoot 'AbisalStealness.exe'))) { throw 'Build Windows first.' }
foreach ($document in @('LEEME PARA JUGAR.txt', 'CREDITS.md', 'LICENSES.md')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $document) -Destination $buildRoot -Force
}
$releaseRoot = Join-Path $projectRoot 'Builds\Releases'
New-Item -ItemType Directory -Force -Path $releaseRoot | Out-Null
$zipPath = Join-Path $releaseRoot ("Abisal-Stealness-Windows-$Version.zip")
if (Test-Path -LiteralPath $zipPath) { throw "Package exists: $zipPath. Use a new version." }
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$stream = [IO.File]::Open($zipPath, [IO.FileMode]::CreateNew)
$archive = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create)
try {
    Get-ChildItem -LiteralPath $buildRoot -File -Recurse | ForEach-Object {
        $relative = $_.FullName.Substring($buildRoot.Length).TrimStart('\')
        if ($relative -match 'DoNotShip|\.pdb$|\.log$') { return }
        $name = 'Abisal-Stealness/' + $relative.Replace('\', '/')
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $_.FullName, $name, [IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
}
finally { $archive.Dispose(); $stream.Dispose() }
$hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
Set-Content -LiteralPath ($zipPath + '.sha256') -Value ($hash + '  ' + [IO.Path]::GetFileName($zipPath))
Write-Output $zipPath
Write-Output "SHA256 $hash"

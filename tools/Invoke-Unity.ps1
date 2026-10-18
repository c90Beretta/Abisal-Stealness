param(
    [ValidateSet('Compile', 'Generate', 'GeneratePort', 'EditMode', 'PlayMode', 'Build', 'Capture', 'CapturePort')]
    [string]$Task = 'EditMode',
    [string]$ProjectPath = (Split-Path $PSScriptRoot -Parent),
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.5.9f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path -LiteralPath $ProjectPath).Path
if (-not (Test-Path -LiteralPath $UnityPath)) { throw "Unity executable not found: $UnityPath" }
$lockPath = Join-Path $projectRoot 'Temp\UnityLockfile'
if (Test-Path -LiteralPath $lockPath) {
    try {
        $lockCheck = [System.IO.File]::Open($lockPath, 'Open', 'ReadWrite', 'None')
        $lockCheck.Dispose()
    }
    catch {
        throw 'This project is open in Unity. Close it or pass a separate validation copy with -ProjectPath.'
    }
}
$resultRoot = Join-Path $projectRoot 'TestResults'
New-Item -ItemType Directory -Force -Path $resultRoot | Out-Null
$logPath = Join-Path $resultRoot ($Task + '.log')
$unityArgs = @('-batchmode', '-projectPath', ('"' + $projectRoot + '"'), '-logFile', ('"' + $logPath + '"'))
switch ($Task) {
    'Compile' { $unityArgs += @('-nographics', '-quit') }
    'Generate' { $unityArgs += @('-nographics', '-quit', '-executeMethod', 'Zombtoy.Editor.FoundationBuilder.Generate') }
    'GeneratePort' { $unityArgs += @('-nographics', '-quit', '-executeMethod', 'Zombtoy.Editor.PortMapBuilder.Generate') }
    'EditMode' { $unityArgs += @('-nographics', '-runTests', '-testPlatform', 'EditMode', '-testResults', ('"' + (Join-Path $resultRoot 'editmode.xml') + '"')) }
    'PlayMode' { $unityArgs += @('-runTests', '-testPlatform', 'PlayMode', '-testResults', ('"' + (Join-Path $resultRoot 'playmode.xml') + '"')) }
    'Build' { $unityArgs += @('-quit', '-executeMethod', 'Zombtoy.Editor.FoundationBuilder.BuildWindows') }
    'Capture' { $unityArgs += @('-quit', '-executeMethod', 'Zombtoy.Editor.FoundationBuilder.CaptureScene') }
    'CapturePort' { $unityArgs += @('-quit', '-executeMethod', 'Zombtoy.Editor.PortMapBuilder.Capture') }
}
$startedUtc = [DateTime]::UtcNow
$process = Start-Process -FilePath $UnityPath -WindowStyle Hidden -ArgumentList $unityArgs -PassThru -Wait
if ($process.ExitCode -ne 0) { throw "Unity $Task failed with exit code $($process.ExitCode). Log: $logPath" }
if ($Task -in @('EditMode', 'PlayMode')) {
    $file = Join-Path $resultRoot ($Task.ToLower() + '.xml')
    if (-not (Test-Path -LiteralPath $file)) { throw "No test result produced: $file" }
    if ((Get-Item -LiteralPath $file).LastWriteTimeUtc -lt $startedUtc) { throw "Unity did not write fresh test results: $file" }
    [xml]$report = Get-Content -Raw -LiteralPath $file
    if ($report.'test-run'.result -ne 'Passed') { throw "Tests did not pass: $file" }
    Write-Output "PASS $Task : $($report.'test-run'.passed) tests"
}
Write-Output "Unity $Task completed. Log: $logPath"

param(
    [ValidateRange(10,1800)][int]$Seconds=600,
    [string]$BuildPath=(Join-Path (Split-Path $PSScriptRoot -Parent) 'Builds/Windows/AbisalStealness.exe'),
    [string]$OutputPath=(Join-Path (Split-Path $PSScriptRoot -Parent) 'Artifacts/IndustrialBenchmark'),
    [switch]$ShowWindow
)
$ErrorActionPreference='Stop'
$executable=(Resolve-Path -LiteralPath $BuildPath).Path
$output=[IO.Path]::GetFullPath($OutputPath)
if(Test-Path -LiteralPath (Join-Path $output 'benchmark.json')) {throw 'Choose a new output directory; keep prior measurements immutable.'}
New-Item -ItemType Directory -Path $output -Force | Out-Null
$arguments=@('-force-d3d11','-screen-fullscreen','0','-screen-width','1920','-screen-height','1080','-abisal-benchmark',$Seconds,'-abisal-output',('"'+$output+'"'),'-logFile',('"'+(Join-Path $output 'player.log')+'"'))
$started=[DateTime]::UtcNow
$windowStyle=if($ShowWindow){'Normal'}else{'Hidden'}
$process=Start-Process -FilePath $executable -WorkingDirectory (Split-Path $executable -Parent) -WindowStyle $windowStyle -ArgumentList $arguments -PassThru
if(-not $process.WaitForExit(($Seconds+180)*1000)) {$process.Kill();throw 'The benchmark process exceeded its bounded measurement window.'}
if($process.ExitCode -ne 0) {throw "Benchmark rejected (exit $($process.ExitCode)); GPU samples or rendering may be unavailable with a hidden window. Inspect $output/player.log and benchmark.json. An interactive run with -ShowWindow may be required; these numbers do not certify FPS."}
$file=Get-Item -LiteralPath (Join-Path $output 'benchmark.json')
if($file.LastWriteTimeUtc -lt $started){throw 'Stale benchmark result'}
$result=Get-Content -Raw -LiteralPath $file.FullName | ConvertFrom-Json
if(-not $result.graphicsEnabled -or -not $result.validResolution -or $result.gpuTimingSamples -le 0 -or $result.measuredSeconds -lt $Seconds){throw 'Invalid rendering or measurement evidence'}
$result | Select-Object kind,version,gpu,graphicsApi,width,height,measuredSeconds,frames,frameP50ms,frameP95ms,frameP99ms,cpuP95ms,gpuP95ms,peakAlive,peakDynamicBodies,peakEffects
Write-Output "Synthetic evidence only; GTX1060 and human playtest gates remain separate. Files: $output"

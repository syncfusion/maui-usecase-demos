# Build + smoke-launch the Windows target. Exit code 0 = pass.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

dotnet build -f net10.0-windows10.0.19041.0 -v q --nologo
if ($LASTEXITCODE -ne 0) { Write-Error 'Build failed'; exit 1 }

$exe = Get-ChildItem "$root\bin\Debug\net10.0-windows10.0.19041.0\win-x64\SprintManagementBoard.exe" | Select-Object -First 1
$p = Start-Process $exe.FullName -PassThru
Start-Sleep 10
if ($p.HasExited) { Write-Error "App exited early with code $($p.ExitCode)"; exit 1 }
Stop-Process -Id $p.Id
Write-Host 'verify: build OK, app launched and stayed running'

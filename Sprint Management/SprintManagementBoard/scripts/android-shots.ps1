# Boots an AVD (if none running), deploys the app, captures Light/Dark screenshots into docs/revamp.
# Debug builds start slowly on emulators, so we wait for the splash to go away.
param([string]$Avd = 'pixel_7_-_api_36_0', [string]$Tag = 'android-phone', [switch]$SkipInstall)
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root
$sdk = 'C:\Program Files (x86)\Android\android-sdk'
$adb = "$sdk\platform-tools\adb.exe"
$pkg = 'com.companyname.sprintmanagementboard'
if (-not (& $adb devices | Select-String 'emulator-\d+\s+device')) {
  Start-Process "$sdk\emulator\emulator.exe" -ArgumentList "-avd $Avd -no-snapshot-save -no-audio" -WindowStyle Minimized
  & $adb wait-for-device
  do { Start-Sleep 3 } until ((& $adb shell getprop sys.boot_completed 2>$null).Trim() -eq '1')
  Start-Sleep 5
}
if (-not $SkipInstall) { dotnet build -f net10.0-android -t:Install -c Debug -v q --nologo 2>&1 | Select-String 'Error\(s\)| error ' }
foreach ($mode in 'no', 'yes') {
  & $adb shell cmd uimode night $mode | Out-Null
  Start-Sleep 3
  & $adb shell am force-stop $pkg
  & $adb shell monkey -p $pkg -c android.intent.category.LAUNCHER 1 2>&1 | Out-Null
  Start-Sleep 45
  $name = if ($mode -eq 'yes') { 'dark' } else { 'light' }
  & $adb shell screencap -p /sdcard/s.png
  & $adb pull /sdcard/s.png "docs\revamp\shot-$Tag-$name.png" | Out-Null
}

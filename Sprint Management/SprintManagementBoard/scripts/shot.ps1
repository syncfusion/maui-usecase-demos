# Usage: shot.ps1 -Name win-light [-Dark]  -> docs/revamp/shot-<Name>.png (window crop, DPI aware)
param([string]$Name = 'win-light', [switch]$Dark, [int]$Wait = 9, [string]$View = '')
$env:SPRINT_START_VIEW = $View
$Dark = [bool]$Dark
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root
Add-Type -TypeDefinition 'using System;using System.Runtime.InteropServices;public class W{[DllImport("user32.dll")]public static extern bool SetProcessDPIAware();[DllImport("user32.dll")]public static extern bool SetForegroundWindow(IntPtr h);[DllImport("user32.dll")]public static extern bool ShowWindow(IntPtr h,int c);[DllImport("user32.dll")]public static extern bool GetWindowRect(IntPtr h,out R r);public struct R{public int L,T,Rt,B;}}'
[W]::SetProcessDPIAware() | Out-Null
Add-Type -AssemblyName System.Windows.Forms, System.Drawing
$key = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize'
$old = (Get-ItemProperty $key).AppsUseLightTheme
Set-ItemProperty $key AppsUseLightTheme ([int](-not $Dark))
try {
  $p = Start-Process 'bin\Debug\net10.0-windows10.0.19041.0\win-x64\SprintManagementBoard.exe' -PassThru
  Start-Sleep $Wait; $p.Refresh()
  [W]::ShowWindow($p.MainWindowHandle, 9) | Out-Null; [W]::SetForegroundWindow($p.MainWindowHandle) | Out-Null; Start-Sleep 2
  $r = New-Object W+R; [W]::GetWindowRect($p.MainWindowHandle, [ref]$r) | Out-Null
  $b = [Windows.Forms.Screen]::PrimaryScreen.Bounds
  $bmp = New-Object Drawing.Bitmap $b.Width, $b.Height
  [Drawing.Graphics]::FromImage($bmp).CopyFromScreen(0, 0, 0, 0, $bmp.Size)
  $rect = New-Object Drawing.Rectangle ([Math]::Max(0,$r.L)), ([Math]::Max(0,$r.T)), ([Math]::Min($b.Width,$r.Rt)-[Math]::Max(0,$r.L)), ([Math]::Min($b.Height,$r.B)-[Math]::Max(0,$r.T))
  $bmp.Clone($rect, $bmp.PixelFormat).Save("docs\revamp\shot-$Name.png")
  Stop-Process -Id $p.Id
} finally { Set-ItemProperty $key AppsUseLightTheme $old }

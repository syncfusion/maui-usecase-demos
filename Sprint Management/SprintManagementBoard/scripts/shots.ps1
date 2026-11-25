# Captures Light/Dark screenshots of every view into docs/revamp (Windows). Build first.
$here = $PSScriptRoot
Get-Process SprintManagementBoard -ErrorAction SilentlyContinue | Stop-Process -Force
foreach ($mode in 'light', 'dark') {
  foreach ($view in 'board', 'scheduler', 'datagrid', 'popup') {
    $a = @{ Name = "win-$mode-$view"; Wait = 12; View = $(if ($view -eq 'board') { 'none' } else { $view }); Dark = ($mode -eq 'dark') }
    & "$here\shot.ps1" @a
  }
}

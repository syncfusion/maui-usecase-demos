$root = Split-Path $PSScriptRoot -Parent
Set-Location $root
$f = 'MainPage.xaml.cs'
$s = [IO.File]::ReadAllText($f)
if ($s -notmatch 'OnSizeAllocated') {
  $i = $s.IndexOf('base.OnAppearing();')
  $k = $s.LastIndexOf('{', $i)
  $ins = @'
protected override void OnSizeAllocated(double w, double h)
	{
		base.OnSizeAllocated(w, h);
		try
		{
			var sv = (ScrollView)((Grid)Content).Children[0];
			System.IO.File.AppendAllText(@"C:\Users\AnandhGanesan\dbg.txt", "page " + w + "x" + h + " sv " + sv.Width + " content " + ((View)sv.Content).Width + " root " + ((Grid)Content).Width + "\n");
		}
		catch { }
	}

	protected override void OnAppearing()
	
'@
  $s = $s.Insert($k, $ins.TrimEnd())
  [IO.File]::WriteAllText($f, $s)
}
dotnet build -f net10.0-windows10.0.19041.0 -v q --nologo 2>&1 | Select-String ' error |Elapsed'
Remove-Item C:\Users\AnandhGanesan\dbg.txt -ErrorAction SilentlyContinue
$p = Start-Process 'bin\Debug\net10.0-windows10.0.19041.0\win-x64\SprintManagementBoard.exe' -PassThru
Start-Sleep 8
Stop-Process -Id $p.Id
Get-Content C:\Users\AnandhGanesan\dbg.txt | Select-Object -Last 4

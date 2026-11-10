$ErrorActionPreference = 'Stop'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { throw '.NET Framework 4.x C# compiler not found.' }
$root = $PSScriptRoot
$output = Join-Path $root 'bin'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$resources = @()
$names = @('windows', 'ios', 'android')
$present = @($names | Where-Object { Test-Path -LiteralPath (Join-Path $root "assets\$_.png") })
if ($present.Count -ne 0 -and $present.Count -ne 3) { throw 'Provide all three licensed PNG assets, or none.' }
if ($present.Count -eq 3) {
    foreach ($name in $names) { $resources += "/resource:$(Join-Path $root "assets\$name.png"),TailscaleAuto.assets.$name.png" }
}
$common = @('/nologo', '/optimize+', '/platform:anycpu', '/reference:System.Windows.Forms.dll', '/reference:System.Drawing.dll', '/reference:System.Web.Extensions.dll') + $resources + @((Join-Path $root 'src\TailscaleAuto.cs'), (Join-Path $root 'src\Stage1UiFix.cs'), (Join-Path $root 'src\Localization.cs'), (Join-Path $root 'src\LanRdpService.cs'), (Join-Path $root 'src\LanRdpHost.cs'))
& $csc /target:winexe "/out:$(Join-Path $output 'TailscaleAuto.exe')" @common
if ($LASTEXITCODE -ne 0) { throw 'Application build failed.' }
& $csc /target:exe "/out:$(Join-Path $output 'TailscaleAuto.Tests.exe')" @common
if ($LASTEXITCODE -ne 0) { throw 'Test build failed.' }

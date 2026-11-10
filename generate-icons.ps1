$ErrorActionPreference = 'Stop'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { throw '.NET Framework 4.x C# compiler not found.' }
$tool = Join-Path $env:TEMP 'TailscaleAuto-GenerateGenericIcons.exe'
& $csc /nologo /target:exe "/out:$tool" /reference:System.Drawing.dll (Join-Path $PSScriptRoot 'assets\GenerateGenericIcons.cs')
if ($LASTEXITCODE -ne 0) { throw 'Icon generator build failed.' }
& $tool (Join-Path $PSScriptRoot 'assets')
if ($LASTEXITCODE -ne 0) { throw 'Icon generation failed.' }

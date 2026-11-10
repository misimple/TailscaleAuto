$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
& (Join-Path $root 'build.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
& (Join-Path $root 'bin\TailscaleAuto.Tests.exe') --self-test
if ($LASTEXITCODE -ne 0) { throw 'Existing tests failed.' }
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$sources = @((Join-Path $root 'src\TailscaleAuto.cs'), (Join-Path $root 'src\Stage1UiFix.cs'), (Join-Path $root 'src\Localization.cs'), (Join-Path $root 'src\LanRdpService.cs'), (Join-Path $root 'src\LanRdpHost.cs'), (Join-Path $PSScriptRoot 'HostMergeQA.cs'))
& $csc /nologo /target:exe /main:HostMergeQA "/out:$(Join-Path $root 'bin\HostMergeQA.exe')" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll @sources
if ($LASTEXITCODE -ne 0) { throw 'LAN integration test build failed.' }
& (Join-Path $root 'bin\HostMergeQA.exe')
if ($LASTEXITCODE -ne 0) { throw 'LAN integration tests failed.' }

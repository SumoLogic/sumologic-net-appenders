<#
.SYNOPSIS
Build solution with MSBuild and create NutGet packages

.DESCRIPTION
Build solution with MSBuild and create NutGet packages

.PARAMETER Config
Debug or Release
#>

param (
  [string]$Config = "Release"
)

$suffix = ""
if (-not $IsWindows) {
  $suffix = ".netstandard"
}

function test-assembly($name) {
  dotnet test --configuration $Config --no-build $name\$name$suffix.csproj
  if ($LastExitCode -ne 0) {
    Write-Error "Failed to test $name [$LastExitCode]"
    exit $LastExitCode
  }
}

Write-Host "======================================"
Write-Host "Test PHASE"
Write-Host "======================================"
test-assembly SumoLogic.Logging.Common.Tests
test-assembly SumoLogic.Logging.NLog.Tests
test-assembly SumoLogic.Logging.Log4Net.Tests
test-assembly SumoLogic.Logging.Serilog.Tests
test-assembly SumoLogic.Logging.AspNetCore.Tests

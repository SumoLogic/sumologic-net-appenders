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

function test-assembly($name) {
  dotnet test --test-adapter-path:. --logger:Appveyor --configuration $Config --no-build $name\$name.csproj
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


#dotnet test --configuration Release --no-build SumoLogic.Logging.Common.Tests\SumoLogic.Logging.Common.Tests.csproj
#dotnet test --configuration Release --no-build SumoLogic.Logging.NLog.Tests\SumoLogic.Logging.NLog.Tests.csproj
#dotnet test --configuration Release --no-build SumoLogic.Logging.Log4Net.Tests\SumoLogic.Logging.Log4Net.Tests.csproj
#dotnet test --configuration Release --no-build SumoLogic.Logging.Serilog.Tests\SumoLogic.Logging.Serilog.Tests.csproj
<#
.SYNOPSIS
Build solution with MSBuild and create NutGet packages

.DESCRIPTION
Build solution with MSBuild and create NutGet packages

.PARAMETER Version
Override a version number (will be used in both .dll and .nutget); if not assiged, will try to read $env:APPVEYOR_REPO_TAG_NAME (for appveyor)

.PARAMETER Config
Debug or Release
#>

param (
  [string]$Version,
  [string]$Config = "Release"
)

# the "final version" used everywhere
$finalVersion = $env:APPVEYOR_REPO_TAG_NAME

if(-not [string]::IsNullOrWhiteSpace($Version)){
    $finalVersion = $Version
}

if([string]::IsNullOrWhiteSpace($finalVersion)){
    Write-Error "Unable to determine release version"
    exit -1
}

Write-Host "Building solution - [$finalVersion] [$Config]"

Write-Output "======================================"
Write-Output "Clean PHASE"
Write-Output "======================================"
if(Test-Path .\SumoLogic.Logging.Nuget) 
{
    Remove-Item -Recurse -Force .\SumoLogic.Logging.Nuget
}
dotnet clean SumoLogic.Logging.sln --configuration $Config
if ($LastExitCode -ne 0) {
    Write-Error "Failed to clean [$LastExitCode]"
    exit $LastExitCode
}

Write-Output "======================================"
Write-Output "Restore PHASE"
Write-Output "======================================"
dotnet restore SumoLogic.Logging.sln /p:Version=$finalVersion
if ($LastExitCode -ne 0) {
    Write-Error "Failed to restore [$LastExitCode]"
    exit $LastExitCode
}

Write-Output "======================================"
Write-Output "Build PHASE"
Write-Output "======================================"
dotnet build --configuration $Config SumoLogic.Logging.sln /p:Version=$finalVersion
if ($LastExitCode -ne 0) {
    Write-Error "Failed to build [$LastExitCode]"
    exit $LastExitCode
}

function run-pack($name) {
    dotnet pack .\$name\$name.csproj --output "$(Convert-Path .)\SumoLogic.Logging.Nuget" --configuration $Config /p:Version=$finalVersion
    if ($LastExitCode -ne 0) {
        Write-Error "Failed to pack $name [$LastExitCode]"
        exit $LastExitCode
    }
}

Write-Output "======================================"
Write-Output "Pack PHASE"
Write-Output "======================================"
run-pack SumoLogic.Logging.Common
run-pack SumoLogic.Logging.Log4Net
run-pack SumoLogic.Logging.NLog
run-pack SumoLogic.Logging.Serilog

#dotnet pack .\SumoLogic.Logging.Common\SumoLogic.Logging.Common.csproj --output "$(Convert-Path .)\SumoLogic.Logging.Nuget" --configuration $Config /p:Version=$finalVersion
#if ($LastExitCode -ne 0) {
#    Write-Error "Failed to pack SumoLogic.Logging.Common [$LastExitCode]"
#    exit $LastExitCode
#}
# dotnet pack .\SumoLogic.Logging.Log4Net\SumoLogic.Logging.Log4Net.csproj --output "$(Convert-Path .)\SumoLogic.Logging.Nuget" --configuration $Config /p:Version=$finalVersion
# if ($LastExitCode -ne 0) {
#     Write-Error "Failed to pack SumoLogic.Logging.Log4Net [$LastExitCode]"
#     exit $LastExitCode
# }
# dotnet pack .\SumoLogic.Logging.NLog\SumoLogic.Logging.NLog.csproj --output "$(Convert-Path .)\SumoLogic.Logging.Nuget" --configuration $Config /p:Version=$finalVersion
# if ($LastExitCode -ne 0) {
#     Write-Error "Failed to pack SumoLogic.Logging.NLog [$LastExitCode]"
#     exit $LastExitCode
# }
# dotnet pack .\SumoLogic.Logging.Serilog\SumoLogic.Logging.Serilog.csproj --output "$(Convert-Path .)\SumoLogic.Logging.Nuget" --configuration $Config /p:Version=$finalVersion
# if ($LastExitCode -ne 0) {
#     Write-Error "Failed to pack SumoLogic.Logging.Serilog [$LastExitCode]"
#     exit $LastExitCode
# }

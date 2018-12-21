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

$slnName = "SumoLogic.Logging$suffix.sln"

# the "final version" used everywhere
$finalVersion = $env:APPVEYOR_REPO_TAG_NAME

if(-not [string]::IsNullOrWhiteSpace($env:APPVEYOR_REPO_TAG_NAME)){
    $finalVersion = $env:APPVEYOR_REPO_TAG_NAME
} else {
    $finalVersion = $env:APPVEYOR_BUILD_VERSION
}

Write-Host "APPVEYOR_REPO_TAG_NAME = $env:APPVEYOR_REPO_TAG_NAME"
Write-Host "APPVEYOR_BUILD_VERSION = $env:APPVEYOR_BUILD_VERSION"

if([string]::IsNullOrWhiteSpace($finalVersion)){
    Write-Warning "Unable to determine release version, defaults to 0.0.0.0"
    $finalVersion = "0.0.0.0"
}

Write-Host "Building solution - [$finalVersion] [$Config]"

Write-Output "======================================"
Write-Output "Clean PHASE"
Write-Output "======================================"
if(Test-Path ".\SumoLogic.Logging.Nuget$suffix") 
{
    Remove-Item -Recurse -Force ".\SumoLogic.Logging.Nuget$suffix"
}
dotnet clean $slnName --configuration $Config
if ($LastExitCode -ne 0) {
    Write-Error "Failed to clean [$LastExitCode]"
    exit $LastExitCode
}

Write-Output "======================================"
Write-Output "Restore PHASE"
Write-Output "======================================"
dotnet restore $slnName /p:Version=$finalVersion
if ($LastExitCode -ne 0) {
    Write-Error "Failed to restore [$LastExitCode]"
    exit $LastExitCode
}

Write-Output "======================================"
Write-Output "Build PHASE"
Write-Output "======================================"
dotnet build --configuration $Config $slnName /p:Version=$finalVersion
if ($LastExitCode -ne 0) {
    Write-Error "Failed to build [$LastExitCode]"
    exit $LastExitCode
}

function run-pack($name) {
    dotnet pack .\$name\$name$suffix.csproj --output "$(Convert-Path .)\SumoLogic.Logging.Nuget$suffix" --configuration $Config /p:Version=$finalVersion
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

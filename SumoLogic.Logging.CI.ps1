param (
    [string]$Version = $null,
    [bool]$forcePackage = $false

)
# the "final version" aka the actual version we are going to use
$fin_ver = $Version

if(![string]::IsNullOrWhiteSpace($env:APPVEYOR_REPO_TAG_NAME)){
    $fin_ver = $env:APPVEYOR_REPO_TAG_NAME
}

if([string]::IsNullOrWhiteSpace($fin_ver)){
    Write-Warning "warning, unable to determine version defaulting to 0.0.1"
    $fin_ver = "0.0.1"
}
Write-Output ======================================
Write-Output Clean PHASE
Write-Output ======================================
dotnet clean SumoLogic.Logging.sln
if(Test-Path .\SumoLogic.Logging.Nuget) 
{
    Remove-Item -Recurse -Force .\SumoLogic.Logging.Nuget
}

Write-Output ======================================
Write-Output RESTORE PHASE
Write-Output ======================================
dotnet restore SumoLogic.Logging.sln /p:Version=$fin_ver #we have to set the version on restore so project -> project references work
Write-Output ======================================
Write-Output BUILD PHASE
Write-Output ======================================
dotnet build --configuration Release SumoLogic.Logging.sln /p:Version=$fin_ver
# the tests are a unique phase in appveyor
if($env:APPVEYOR -ne "true"){
    Write-Output "Build not running in appveyor, executing tests"
    & .\SumoLogic.Logging.CI.Test.ps1
}
if(($env:APPVEYOR_REPO_TAG -eq "true") -or ($forcePackage -eq $true)) {
    Write-Output ======================================
    Write-Output PACK PHASE
    Write-Output ======================================
    dotnet pack .\SumoLogic.Logging.Common\SumoLogic.Logging.Common.csproj --output "$(Convert-Path .)\SumoLogic.Logging.Nuget" --configuration Release /p:Version=$fin_ver
    dotnet pack .\SumoLogic.Logging.Log4Net\SumoLogic.Logging.Log4Net.csproj --output "$(Convert-Path .)\SumoLogic.Logging.Nuget" --configuration Release /p:Version=$fin_ver
    dotnet pack .\SumoLogic.Logging.NLog\SumoLogic.Logging.NLog.csproj --output "$(Convert-Path .)\SumoLogic.Logging.Nuget" --configuration Release /p:Version=$fin_ver-beta1
}
else{
    Write-Warning "APPVEYOR_REPO_TAG value not set thus we are not making packages on tags, thus we are not making packages. If you wish to make packages set the APPVEYOR_REPO_TAG envionmental variable to the word true"
}

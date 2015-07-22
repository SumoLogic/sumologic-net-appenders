@echo off
echo CREATING NUGET PACKAGES ...
echo ======================================
msbuild SumoLogic.Logging.CI.csproj "/t:MakeNugetPackages" "/v:m"

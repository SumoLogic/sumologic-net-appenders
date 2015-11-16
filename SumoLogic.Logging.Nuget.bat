@echo off
echo "Building Libraries"
msbuild SumoLogic.Logging.CI.csproj 
echo CREATING NUGET PACKAGES ...
echo ======================================
msbuild SumoLogic.Logging.CI.csproj "/t:MakeNugetPackages" "/v:m"

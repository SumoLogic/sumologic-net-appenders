@echo off

goto:setpathanddir

:pauseandexit
rem pause
exit /b %LASTEXITCODE%

:error
echo EXITING WITH ERROR CODE %LASTEXITCODE%
chdir /d "%ORIG_PATH%"
goto:pauseandexit

:success
chdir /d "%ORIG_PATH%"
goto:pauseandexit

:setpathanddir
echo SETTING PATH AND CHANGING DIRECTORY...
echo ======================================
set PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319;%PATH%
set ORIG_PATH=%cd%
set SCRIPT_PATH=%~dp0
chdir /d "%SCRIPT_PATH:~0,-1%"
echo DONE SETTING PATH AND CHANGING DIRECTORY
echo.
:updatepackages
echo UPDATING NUGET PACKAGES ...
echo ===========================
.\.nuget\nuget.exe restore SumoLogic.Logging.sln
dotnet restore SumoLogic.Logging.sln
set LASTEXITCODE=%ERRORLEVEL%
if NOT "%LASTEXITCODE%"=="0" (
   echo ERROR UPDATING NUGET PACKAGES
   goto:error
)
echo DONE UPDATING NUGET PACKAGES
echo.

:build
echo BUILDING SOLUTION ...
echo =====================
dotnet msbuild SumoLogic.Logging.CI.csproj "/v:m"
set LASTEXITCODE=%ERRORLEVEL%
if NOT "%LASTEXITCODE%"=="0" (
   echo ERROR BUILDING SOLUTION
   goto:error
)
echo DONE BUILDING SOLUTION
echo.

:runtests
echo RUNNING TESTS ...
echo =================
dotnet msbuild SumoLogic.Logging.CI.csproj "/t:RunTestsWithCoverage" "/v:m"
set LASTEXITCODE=%ERRORLEVEL%
if NOT "%LASTEXITCODE%"=="0" (
   echo ERROR RUNNING TESTS
   goto:error
)
echo DONE RUNNING TESTS
echo.

goto:success

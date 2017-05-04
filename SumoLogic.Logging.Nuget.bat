@echo off
dotnet pack --output %~dp0SumoLogic.Logging.Nuget --configuration Release SumoLogic.Logging.Common/SumoLogic.Logging.Common.csproj
dotnet pack --output %~dp0SumoLogic.Logging.Nuget --configuration Release SumoLogic.Logging.Log4Net/SumoLogic.Logging.Log4Net.csproj
dotnet pack --output %~dp0SumoLogic.Logging.Nuget --configuration Release SumoLogic.Logging.Nlog/SumoLogic.Logging.Nlog.csproj

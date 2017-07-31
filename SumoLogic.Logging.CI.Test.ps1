Write-Host ======================================
Write-Host TEST PHASE
Write-Host ======================================
dotnet test --configuration Release --no-build SumoLogic.Logging.NLog.Tests\SumoLogic.Logging.NLog.Tests.csproj
dotnet test --configuration Release --no-build SumoLogic.Logging.Log4Net.Tests\SumoLogic.Logging.Log4Net.Tests.csproj
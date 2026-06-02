@echo off

dotnet tool install docfx
dotnet docfx metadata docfx.json --warningsAsErrors
pause

@echo off

dotnet tool install docfx
dotnet docfx build docfx.json --warningsAsErrors
call npx -y pagefind --site _site
pause

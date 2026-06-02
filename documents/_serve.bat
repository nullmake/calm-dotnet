@echo off

dotnet tool install docfx
dotnet docfx metadata docfx.json --warningsAsErrors
dotnet docfx build docfx.json --warningsAsErrors
call npx -y pagefind --site _site
dotnet docfx serve _site --open-browser

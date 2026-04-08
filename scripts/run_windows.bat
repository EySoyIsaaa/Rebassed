@echo off
setlocal

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
  echo [ERROR] .NET SDK no encontrado. Instala .NET 8 SDK desde:
  echo https://dotnet.microsoft.com/download/dotnet/8.0
  exit /b 1
)

echo Restaurando paquetes...
dotnet restore Rebassed.sln || exit /b 1

echo Compilando...
dotnet build Rebassed.sln -c Release || exit /b 1

echo Ejecutando app...
dotnet run --project src\Rebassed.Desktop\Rebassed.Desktop.csproj -c Release

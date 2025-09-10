@echo off
echo M36 Leak Detector - Automaticke spusteni
echo =======================================

echo.
echo 1. Kontrola Node.js...
node --version >nul 2>&1
if errorlevel 1 (
    echo CHYBA: Node.js neni nainstalovany!
    echo Prosim nainstalujte Node.js z https://nodejs.org/
    pause
    exit /b 1
)
echo Node.js OK

echo.
echo 2. Kontrola .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo CHYBA: .NET SDK neni nainstalovany!
    echo Prosim nainstalujte .NET 8 SDK z https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
echo .NET SDK OK

echo.
echo 3. Instalace zavislosti...
call npm install
if errorlevel 1 (
    echo CHYBA pri instalaci Node.js zavislosti!
    pause
    exit /b 1
)

echo.
echo 4. Sestaveni backend...
cd backend\M36Backend
call dotnet restore
call dotnet build
if errorlevel 1 (
    echo CHYBA pri sestaveni backend!
    pause
    exit /b 1
)
cd ..\..

echo.
echo 5. Spoustim aplikaci...
call npm run dev

pause

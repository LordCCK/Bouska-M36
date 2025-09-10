# M36 Leak Detector - Automatické spuštění
# =====================================

Write-Host "M36 Leak Detector - Automaticke spusteni" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green
Write-Host ""

# Kontrola Node.js
Write-Host "1. Kontrola Node.js..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    Write-Host "Node.js OK: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "CHYBA: Node.js neni nainstalovany!" -ForegroundColor Red
    Write-Host "Spustte: winget install OpenJS.NodeJS" -ForegroundColor Yellow
    Read-Host "Stisknete Enter pro ukonceni"
    exit 1
}

# Kontrola .NET SDK
Write-Host ""
Write-Host "2. Kontrola .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host ".NET SDK OK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "CHYBA: .NET SDK neni nainstalovany!" -ForegroundColor Red
    Write-Host "Spustte: winget install Microsoft.DotNet.SDK.8" -ForegroundColor Yellow
    Read-Host "Stisknete Enter pro ukonceni"
    exit 1
}

# Instalace závislostí
Write-Host ""
Write-Host "3. Instalace zavislosti..." -ForegroundColor Yellow
try {
    npm install
    if ($LASTEXITCODE -ne 0) { throw "npm install failed" }
    Write-Host "NPM zavislosti nainstalovany" -ForegroundColor Green
} catch {
    Write-Host "CHYBA pri instalaci Node.js zavislosti!" -ForegroundColor Red
    Read-Host "Stisknete Enter pro ukonceni"
    exit 1
}

# Sestavení backend
Write-Host ""
Write-Host "4. Sestaveni backend..." -ForegroundColor Yellow
try {
    Set-Location "backend\M36Backend"
    dotnet restore
    dotnet build
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }
    Set-Location "..\..\"
    Write-Host "Backend sestaven uspesne" -ForegroundColor Green
} catch {
    Write-Host "CHYBA pri sestaveni backend!" -ForegroundColor Red
    Set-Location "..\..\"
    Read-Host "Stisknete Enter pro ukonceni"
    exit 1
}

# Spuštění aplikace
Write-Host ""
Write-Host "5. Spoustim aplikaci..." -ForegroundColor Yellow
Write-Host "Backend bude dostupny na: http://localhost:5000" -ForegroundColor Cyan
Write-Host "Electron aplikace se otevri automaticky" -ForegroundColor Cyan
Write-Host ""
Write-Host "Pro ukonceni stisknete Ctrl+C" -ForegroundColor Yellow
Write-Host ""

npm run dev

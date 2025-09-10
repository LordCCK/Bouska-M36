# M36 - Rychlé spuštění ⚡

## 🎯 Pro kolegy - 3 způsoby spuštění:

### 1️⃣ Nejjednodušší (doporučeno)
```
Dvojklik na: spustit.bat
```

### 2️⃣ PowerShell
```powershell
.\spustit.ps1
```

### 3️⃣ Manuálně
```powershell
npm install
cd backend\M36Backend && dotnet restore && cd ..\..
npm run dev
```

## 📋 Co potřebujete nainstalovat:
- **Node.js**: `winget install OpenJS.NodeJS`
- **.NET 8 SDK**: `winget install Microsoft.DotNet.SDK.8`

## 🌐 Po spuštění:
- **Backend API**: http://localhost:5000/api/orders
- **Electron app**: Otevře se automaticky

## ❓ Problém?
Viz [INSTALACE.md](INSTALACE.md) pro detailní návod

---
**Vyvíjeno pro M36 Leak Detector System** 🔧

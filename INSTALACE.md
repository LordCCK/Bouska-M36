# M36 Leak Detector - Návod pro spuštění

## 📋 Požadavky na systém

### 1. Nainstalujte Node.js
```powershell
# Pomocí winget (doporučeno)
winget install OpenJS.NodeJS

# NEBO stáhněte z https://nodejs.org/ (verze 16 nebo vyšší)
```

### 2. Nainstalujte .NET SDK
```powershell
# Pomocí winget (doporučeno)
winget install Microsoft.DotNet.SDK.8

# NEBO stáhněte z https://dotnet.microsoft.com/download (verze 8.0)
```

### 3. Ověřte instalaci
```powershell
# Restartujte PowerShell a zkontrolujte:
node --version    # mělo by zobrazit v18+ nebo v20+
npm --version     # mělo by zobrazit 8+ nebo 9+
dotnet --version  # mělo by zobrazit 8.0.x
```

## 🚀 Spuštění projektu

### Krok 1: Otevřete projekt v VS Code
```
1. Otevřete VS Code
2. File → Open Folder
3. Vyberte složku s projektem M36
```

### Krok 2: Otevřete terminál v VS Code
```
Terminal → New Terminal (nebo Ctrl + Shift + `)
```

### Krok 3: Instalace závislostí
```powershell
# Instalace Node.js závislostí
npm install

# Přejděte do backend složky a obnovte .NET závislosti
cd backend\M36Backend
dotnet restore
dotnet build

# Vraťte se do hlavní složky
cd ..\..
```

### Krok 4: Spuštění aplikace

#### Možnost A - Automatické spuštění (doporučeno)
```powershell
# Spustí backend i frontend současně
npm run dev
```

#### Možnost B - Ruční spuštění (2 terminály)
```powershell
# Terminál 1 - Backend
cd backend\M36Backend
dotnet run

# Terminál 2 - Frontend (nový terminál)
npm start
```

## 🔧 Konfigurace před použitím

### 1. Databázové připojení
Upravte soubory v `backend\M36Backend\Services\`:

**IBMSQLService.cs** (řádek 14):
```csharp
_connectionString = "Server=VÁŠ_IBM_SERVER;Database=VÁŠ_DATABASE;UID=USERNAME;PWD=PASSWORD;";
```

**MSSQLService.cs** (řádek 12):
```csharp
_connectionString = "Server=VÁŠ_SQL_SERVER;Database=M36Database;Integrated Security=true;";
```

### 2. Zebra tiskárna
Upravte soubor `backend\M36Backend\Services\ZebraPrinterService.cs` (řádek 14):
```csharp
_printerIP = "192.168.1.XXX"; // IP adresa vaší Zebra tiskárny
```

### 3. Sériový port detektoru
Detektor úniku se automaticky detekuje při spuštění na portu 9600 baud, 8 datových bitů.

## ✅ Ověření funkčnosti

### 1. Backend API test
Otevřete browser a jděte na:
```
http://localhost:5000/api/orders
```
Měli byste vidět JSON s testovacími zakázkami.

### 2. Electron aplikace
- Měla by se otevřít fullscreen aplikace
- Zobrazí se "M36 - Naskenujte kartu"
- Stiskněte Enter pro přechod do hlavního rozhraní

## 🛠️ Řešení problémů

### "npm není rozpoznán"
```powershell
# Restartujte VS Code nebo PowerShell
# Nebo přidejte Node.js do PATH ručně:
$env:PATH += ";C:\Program Files\nodejs"
```

### "dotnet není rozpoznán"
```powershell
# Restartujte VS Code nebo PowerShell
# Zkontrolujte instalaci .NET SDK
dotnet --list-sdks
```

### Backend se nespustí
```powershell
# Zkontrolujte port 5000
netstat -an | findstr :5000

# Pokud je obsazený, ukončete proces nebo změňte port v Program.cs
```

### Electron se nespustí
```powershell
# Zkontrolujte instalaci závislostí
npm install --force

# Spusťte v debug módu
npm run dev
```

### Databázové chyby
- Aplikace funguje i bez databáze (používá testovací data)
- Pro produkční použití nakonfigurujte připojovací řetězce

## 📁 Struktura projektu
```
M36-Leak-Detector/
├── src/                    # Electron frontend
│   ├── main.js            # Hlavní proces
│   ├── renderer.js        # UI logika
│   ├── index.html         # Hlavní stránka
│   └── styles.css         # Styly
├── backend/               # C# API backend
│   └── M36Backend/
│       ├── Controllers/   # API endpoints
│       ├── Services/      # Business logika
│       └── Models/        # Datové modely
├── package.json           # NPM konfigurace
└── README.md             # Dokumentace
```

## 🎯 Základní použití
1. **Spuštění**: `npm run dev`
2. **Načtení karty**: Stiskněte Enter na úvodní obrazovce
3. **Refresh zakázek**: Klikněte na ikonu refresh
4. **Výběr zakázky**: Vyberte ze selectu
5. **Skenování dílů**: Naskenujte čárový kód dílu
6. **Test úniku**: Stiskněte fyzické tlačítko na detektoru
7. **Tisk etiket**: Klikněte na ikonu tiskárny

## 📞 Podpora
Pro technické problémy kontaktujte vývojáře nebo zkontrolujte:
- Console v VS Code (View → Output)
- Browser DevTools (F12) v Electron aplikaci
- Log soubory v backend složce

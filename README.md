# M36 Leak Detector Application

Desktopová aplikace v Electronu s C# backendem pro M36 systém čtení karet a detekce úniku.

## 🚀 Rychlé spuštění

**Pro kolegy - kompletní návod:** Viz [INSTALACE.md](INSTALACE.md)

### Zkrácená verze:
1. Nainstalujte Node.js a .NET 8 SDK
2. `npm install`
3. `cd backend\M36Backend && dotnet restore && cd ..\..`
4. `npm run dev`

## Funkce

- 🔐 **Čtení karet** - Úvodní obrazovka pro načtení karty operátora
- 📋 **Správa zakázek** - Načítání zakázek z IBM SQL databáze
- 🔧 **Testování úniku** - Komunikace se sériovým detektorem úniku (9600 8b)
- 💾 **Ukládání výsledků** - Zápis výsledků testů do MS SQL databáze
- 🖨️ **Tisk etiket** - Podpora Zebra tiskáren pro tisk etiket
- 📊 **Real-time monitoring** - Sledování testů v reálném čase

## Struktura projektu

```
M36-Leak-Detector/
├── src/                    # Electron frontend
│   ├── main.js            # Hlavní Electron proces
│   ├── renderer.js        # Renderer proces (UI logika)
│   ├── index.html         # Hlavní HTML
│   └── styles.css         # Styly aplikace
├── backend/               # C# backend
│   └── M36Backend/
│       ├── Controllers/   # API controllers
│       ├── Services/      # Business logika
│       ├── Models/        # Datové modely
│       └── Program.cs     # Entry point
├── assets/               # Statické soubory
└── package.json          # NPM konfigurace
```

## Instalace a spuštění

### Předpoklady

- Node.js 16+
- .NET 6.0 SDK
- IBM DB2 klient (pro připojení k IBM databázi)
- SQL Server (pro ukládání výsledků)

### 1. Instalace závislostí

```bash
# Instalace npm balíčků pro Electron
npm install

# Instalace .NET závislostí
npm run install-backend
```

### 2. Konfigurace databází

#### IBM SQL Database
Upravte připojovací řetězec v `backend/M36Backend/Services/IBMSQLService.cs`:

```csharp
_connectionString = "Server=your_ibm_server;Database=your_database;UID=your_username;PWD=your_password;";
```

#### MS SQL Database
Upravte připojovací řetězec v `backend/M36Backend/Services/MSSQLService.cs`:

```csharp
_connectionString = "Server=your_server;Database=M36Database;Integrated Security=true;";
```

### 3. Konfigurace Zebra tiskárny

Upravte IP adresu tiskárny v `backend/M36Backend/Services/ZebraPrinterService.cs`:

```csharp
_printerIP = "192.168.1.100"; // IP vaší Zebra tiskárny
```

### 4. Sestavení C# backendu

```bash
npm run build-backend
```

### 5. Spuštění aplikace

```bash
# Vývojový režim
npm run dev

# Produkční spuštění
npm start
```

## Použití aplikace

### 1. Načtení karty
- Po spuštění se zobrazí úvodní obrazovka
- Naskenujte kartu operátora a stiskněte Enter
- Aplikace přejde do hlavního rozhraní

### 2. Výběr zakázky
- Klikněte na tlačítko Refresh pro načtení zakázek z IBM databáze
- Vyberte zakázku ze selectu
- Zobrazí se detaily zakázky a tabulka výrobků

### 3. Testování dílů
- Naskenujte čárový kód dílu
- Díl se označí v tabulce a funkční test se označí jako OK
- Spusťte fyzický test tlačítkem na detektoru
- Aplikace zobrazí real-time data z detektoru
- Po dokončení testu se výsledek automaticky uloží

### 4. Tisk etiket
- Klikněte na ikonu tiskárny
- Vyberte "Tisknout celou zakázku" nebo jednotlivý díl
- Etikety se vytisknou na Zebra tiskárně

## Technické detaily

### Komunikace se sériovým portem
Aplikace automaticky detekuje sériový port detektoru úniku s nastavením:
- Rychlost: 9600 baud
- Datové bity: 8
- Parita: žádná
- Stop bity: 1

### Zpracování dat detektoru
Data z detektoru začínají "TEST" a končí "ROUGH". Aplikace zpracovává:
- SETPOINT hodnoty
- LEAK hodnoty pro průměr z posledních 5 vzorků
- STAND-BY a PASS signály pro konec testu

### Databázové operace
- **IBM SQL**: Čtení zakázek a detailů výrobků
- **MS SQL**: Zápis výsledků testů do tabulky M36

### Formát záznamu testu
```sql
INSERT INTO M36 (
    [ORDER], BARCODE, DATE, TIME, OPERATOR, 
    SETPOINT, LEAK, RESULT, PCN, TYPE
) VALUES (...)
```

## Vývoj

### Struktura kódu

#### Frontend (Electron)
- `main.js` - Hlavní proces, komunikace s backendem
- `renderer.js` - UI logika, zpracování událostí
- `index.html` - Layout aplikace
- `styles.css` - Responsive design

#### Backend (C#)
- `OrdersController` - API pro zakázky a výsledky testů
- `PrintController` - API pro tisk etiket
- `IBMSQLService` - Komunikace s IBM databází
- `MSSQLService` - Komunikace s MS SQL databází
- `ZebraPrinterService` - Tisk na Zebra tiskárny

### Rozšíření funkcí

Pro přidání nových funkcí:

1. **Nový endpoint**: Přidejte controller v `backend/M36Backend/Controllers/`
2. **Business logika**: Vytvořte service v `backend/M36Backend/Services/`
3. **Frontend komunikace**: Přidejte IPC handler v `src/main.js`
4. **UI**: Upravte `src/renderer.js` a `src/index.html`

## Řešení problémů

### Backend se nespustí
- Zkontrolujte, zda je nainstalován .NET 6.0 SDK
- Ověřte, že backend byl sestaven: `npm run build-backend`

### Sériový port nefunguje
- Zkontrolujte, zda je detektor připojen
- Ověřte nastavení portu v Device Manageru
- Restartujte aplikaci

### Databázové chyby
- Ověřte připojovací řetězce
- Zkontrolujte síťové připojení k databázím
- Pro testování jsou k dispozici mock data

### Tiskárna nereaguje
- Zkontrolujte IP adresu tiskárny
- Ověřte síťové připojení
- ZPL příkazy se ukládají do souborů pro debugging

## Licence

MIT License - Viz LICENSE soubor pro detaily.

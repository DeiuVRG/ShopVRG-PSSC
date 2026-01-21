# ShopVRG - Ghid de Instalare și Configurare

## Cerințe Preliminare

### Backend
- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Visual Studio 2022** sau **VS Code** cu extensia C#

### Frontend
- **Node.js 18+** - [Download](https://nodejs.org/)
- **npm** (inclus cu Node.js)

### Opțional
- **Azure CLI** - pentru deployment
- **SQL Server Management Studio** - pentru gestionarea bazei de date

---

## Quick Start (5 minute)

```bash
# 1. Clone repository
git clone https://github.com/DeiuVRG/ShopVRG-PSSC.git
cd ShopVRG-PSSC

# 2. Start Backend
dotnet restore
dotnet run --project ShopVRG.Api
# API disponibil la: http://localhost:5000

# 3. Start Frontend (în alt terminal)
cd shopvrg-frontend
npm install
npm start
# Frontend disponibil la: http://localhost:3000
```

---

## Configurare Detaliată

### 1. Backend Setup

#### Restore Dependencies
```bash
dotnet restore
```

#### Build Project
```bash
dotnet build
```

#### Run API
```bash
dotnet run --project ShopVRG.Api
```

API-ul va fi disponibil la:
- **Swagger UI**: http://localhost:5000
- **API Base**: http://localhost:5000/api

#### Verificare
```bash
# Test endpoint produse
curl http://localhost:5000/api/products
```

---

### 2. Frontend Setup

#### Instalare Dependențe
```bash
cd shopvrg-frontend
npm install
```

#### Start Development Server
```bash
npm start
```

Frontend-ul va fi disponibil la: **http://localhost:3000**

#### Build pentru Producție
```bash
npm run build
```

---

### 3. Database

Proiectul folosește **Azure SQL Database**. Conexiunea este configurată în `ShopVRG.Api/appsettings.json`.

#### Schema Database
Scriptul SQL pentru crearea tabelelor se află în:
- `ShopVRG.Api/Database/CreateTables.sql`

#### Query-uri Utile
Fișierul `database-queries.sql` conține query-uri pentru:
- Vizualizare produse
- Sumar pe categorii
- Produse cu stoc scăzut
- Comenzi recente

#### Vizualizare Database (macOS/Linux)
```bash
chmod +x view-db.sh
./view-db.sh
```

---

## Configurare Azure (Opțional)

### Pentru Development Local cu Azure

Creează fișierul `ShopVRG.Api/appsettings.Development.local.json`:

```json
{
  "ConnectionStrings": {
    "AZURE_SQL_CONNECTIONSTRING": "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=YOUR_DATABASE;User ID=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://YOUR_SERVICEBUS.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR_KEY",
    "UseAzure": true
  }
}
```

> **IMPORTANT**: Fișierul `*.local.json` este exclus din Git (`.gitignore`). Nu publica niciodată credențialele pe GitHub!

### Dezactivare Azure Service Bus (In-Memory Mode)
Pentru development fără Azure Service Bus, setează în appsettings:
```json
{
  "ServiceBus": {
    "UseAzure": false
  }
}
```

---

## Rulare Teste

```bash
# Toate testele
dotnet test

# Doar unit tests
dotnet test --filter "Category=Unit"

# Doar integration tests
dotnet test --filter "Category=Integration"

# Cu verbose output
dotnet test --verbosity detailed

# Cu coverage report
dotnet test --collect:"XPlat Code Coverage"
```

---

## Structura Porturi

| Serviciu | Port | URL |
|----------|------|-----|
| Backend API | 5000 | http://localhost:5000 |
| Swagger UI | 5000 | http://localhost:5000 |
| Frontend | 3000 | http://localhost:3000 |

---

## Troubleshooting

### Port 5000 ocupat
```bash
# macOS/Linux - găsește procesul
lsof -i :5000
# Kill proces
kill -9 <PID>
```

### Eroare CORS
Frontend-ul este configurat să comunice cu backend-ul pe `localhost:5000`. Dacă schimbi portul, actualizează și în frontend (`src/api/`).

### Database Connection Error
1. Verifică connection string în `appsettings.json`
2. Verifică că Azure SQL Database este pornită
3. Verifică firewall rules în Azure Portal

### npm install eșuează
```bash
# Șterge node_modules și reinstalează
rm -rf node_modules package-lock.json
npm install
```

---

## Comenzi Utile

```bash
# Rebuild complet
dotnet clean && dotnet build

# Watch mode (recompilare automată)
dotnet watch run --project ShopVRG.Api

# Format cod
dotnet format

# Update packages
dotnet restore --force
```

---

## IDE Recomandate

### Visual Studio 2022
- Deschide `ShopVRG.sln`
- F5 pentru debug

### VS Code
- Extensii recomandate: C#, C# Dev Kit, REST Client
- Launch configuration inclusă în `.vscode/`

### Rider
- Deschide `ShopVRG.sln`
- Funcționează out-of-the-box

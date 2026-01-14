# ShopVRG Configuration

## Local Development Setup

Pentru a rula aplicația local, creează fișierul `ShopVRG.Api/appsettings.Development.local.json` cu următorul conținut:

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

**IMPORTANT**: Fișierul `*.local.json` este exclus din Git pentru securitate. Nu publica niciodată credențialele Azure pe GitHub!

## Backend Setup

1. Configurează credențialele Azure în `appsettings.Development.local.json`
2. Rulează backend-ul:
   ```bash
   cd ShopVRG.Api
   dotnet run --launch-profile http
   ```
3. Backend va rula pe http://localhost:5000

## Frontend Setup

1. Instalează dependențele:
   ```bash
   cd shopvrg-frontend
   npm install
   ```
2. Pornește frontend-ul:
   ```bash
   npm start
   ```
3. Frontend va rula pe http://localhost:3000

## Database

Scriptul SQL pentru crearea tabelelor se află în `ShopVRG.Api/Database/CreateTables.sql`

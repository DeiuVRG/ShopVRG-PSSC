# ShopVRG - PC Components Store - DDD Lab

## Echipa
- Simedre Patricia Teodora
- Rusu Andrei Ioan
- Plesa Valentin Gabriel

## Domeniul Ales
**E-Commerce pentru Componente de Calculator** - Sistem de gestionare a comenzilor pentru un magazin online de componente PC.

## Descriere
ShopVRG este un backend pentru un magazin de componente de calculator implementat folosind principiile Domain-Driven Design (DDD). Sistemul gestioneaza intreg ciclul de viata al unei comenzi: plasare comanda, procesare plata si expediere.

Proiectul respecta:
- **Principiile SOLID**
- **Cele 4 principii OOP** (Encapsulare, Mostenire, Polimorfism, Abstractie)
- **Domain-Driven Design** cu Value Objects, Entities, Aggregates
- **State Machine Pattern** pentru tranzitii de stare
- **Transform Pattern** pentru operatii pe entitati

## Bounded Contexts Identificate

1. **Order Management Context**: Responsabil pentru plasarea si validarea comenzilor, verificarea stocului, calculul preturilor
2. **Payment Context**: Responsabil pentru validarea si procesarea platilor, integrarea cu gateway-uri de plata
3. **Shipping Context**: Responsabil pentru expedierea comenzilor, generarea tracking numbers, integrarea cu curieri

## Event Storming Results

### Domain Events
- `OrderPlacedEvent` - Comanda a fost plasata cu succes
- `OrderPlacementFailedEvent` - Plasarea comenzii a esuat
- `PaymentProcessedEvent` - Plata a fost procesata
- `PaymentFailedEvent` - Plata a esuat
- `OrderShippedEvent` - Comanda a fost expediata
- `ShippingFailedEvent` - Expedierea a esuat

### Flow
```
[Customer places order] -> OrderPlacedEvent -> [Process payment] -> PaymentProcessedEvent -> [Ship order] -> OrderShippedEvent
```

## Implementare

### Value Objects
- `ProductCode`: Cod unic produs (format: 2-4 litere + 3-6 cifre, ex: CPU001, GPU12345)
- `ProductName`: Nume produs (3-200 caractere)
- `Price`: Pret monetary (0.01 - 1,000,000)
- `Quantity`: Cantitate comanda (1-1000)
- `StockQuantity`: Stoc disponibil (0-100,000)
- `CustomerEmail`: Email valid (RFC 5321)
- `CustomerName`: Nume client (2-100 caractere)
- `ShippingAddress`: Adresa completa (Street, City, PostalCode, Country)
- `OrderId`: Identificator unic comanda (GUID)
- `PaymentId`: Identificator unic plata (GUID)

### Entity States

#### Order States
- `UnvalidatedOrder`: Date brute din API, inainte de validare
- `ValidatedOrder`: Date validate, convertite in Value Objects
- `StockCheckedOrder`: Stoc verificat, preturi calculate
- `PlacedOrder`: Comanda finalizata si persistata
- `InvalidOrder`: Comanda invalida cu lista de erori

#### Payment States
- `UnvalidatedPayment`: Date brute pentru plata
- `ValidatedPayment`: Date validate, card mascat
- `ProcessedPayment`: Plata procesata cu referinta tranzactie
- `InvalidPayment`: Plata invalida cu erori

#### Shipping States
- `UnvalidatedShipping`: Date brute pentru expediere
- `ValidatedShipping`: Date validate, tracking generat
- `ShippedOrder`: Comanda expediata cu data estimata livrare
- `InvalidShipping`: Expediere invalida cu erori

### Operations

1. `ValidateOrderOperation`: Valideaza datele comenzii si converteste la Value Objects
2. `CheckStockOperation`: Verifica stocul si rezerva produsele
3. `PlaceOrderOperation`: Persista comanda in baza de date
4. `ValidatePaymentOperation`: Valideaza datele cardului si suma
5. `ProcessPaymentOperation`: Proceseaza plata cu gateway-ul
6. `ValidateShippingOperation`: Valideaza comanda pentru expediere
7. `ShipOrderOperation`: Marcheaza comanda ca expediata

### Workflows

- `PlaceOrderWorkflow`: Validate -> CheckStock -> PlaceOrder
- `ProcessPaymentWorkflow`: ValidatePayment -> ProcessPayment
- `ShipOrderWorkflow`: ValidateShipping -> ShipOrder

## Structura Proiectului

```
ShopVRG_Hub/
├── ShopVRG.sln
├── ShopVRG.Domain/           # Domain Layer
│   ├── Models/
│   │   ├── ValueObjects/     # Value Objects
│   │   ├── Entities/         # Entity States
│   │   ├── Commands/         # Commands
│   │   └── Events/           # Domain Events
│   ├── Operations/           # Transform Operations
│   ├── Workflows/            # Business Workflows
│   └── Repositories/         # Repository Interfaces
├── ShopVRG.Data/             # Infrastructure Layer
│   ├── ShopDbContext.cs      # EF Core DbContext
│   ├── Models/               # Data Entities
│   └── Repositories/         # Repository Implementations
├── ShopVRG.Events/           # Event Contracts
├── ShopVRG.Events.ServiceBus/# Azure Service Bus + InMemory
└── ShopVRG.Api/              # API Layer
    ├── Controllers/          # REST Controllers
    ├── Models/               # DTOs
    └── Program.cs            # Entry Point
```

## Rulare

```bash
# Compile
dotnet build

# Run API
dotnet run --project ShopVRG.Api

# API va fi disponibil la:
# Swagger UI: http://localhost:5000
# API Base: http://localhost:5000/api
```

## API Endpoints

| Metoda | Endpoint | Descriere |
|--------|----------|-----------|
| GET | /api/products | Lista toate produsele |
| GET | /api/products/active | Produse active |
| GET | /api/products/{code} | Produs dupa cod |
| GET | /api/products/category/{category} | Produse dupa categorie |
| POST | /api/orders | Plaseaza comanda |
| GET | /api/orders/{id}/exists | Verifica existenta |
| POST | /api/payments | Proceseaza plata |
| POST | /api/shipping | Expediaza comanda |
| GET | /api/shipping/carriers | Lista curieri |

## Exemple de Utilizare

### Plasare Comanda
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "shippingStreet": "123 Main Street",
    "shippingCity": "Timisoara",
    "shippingPostalCode": "300001",
    "shippingCountry": "Romania",
    "orderLines": [
      {"productCode": "CPU001", "quantity": 1},
      {"productCode": "GPU001", "quantity": 1}
    ]
  }'
```

### Procesare Plata
```bash
curl -X POST http://localhost:5000/api/payments \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "ORDER_ID_HERE",
    "amount": 2189.98,
    "cardNumber": "4111111111111111",
    "cardHolderName": "John Doe",
    "expiryDate": "12/26",
    "cvv": "123"
  }'
```

### Expediere Comanda
```bash
curl -X POST http://localhost:5000/api/shipping \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "ORDER_ID_HERE",
    "carrier": "FAN_COURIER"
  }'
```

## Tehnologii Utilizate

- **.NET 9** - Framework principal
- **Entity Framework Core 9** - ORM
- **SQLite** - Baza de date locala (pregatit pentru Azure SQL)
- **Swagger/OpenAPI** - Documentatie API
- **Azure Service Bus** - Comunicare asincrona (pregatit)

## Lectii Invatate

### Ce a functionat bine cu AI
- Generarea structurii DDD cu Value Objects si Entity States
- Implementarea Transform Pattern pentru operatii
- Generarea validarilor cu TryCreate pattern
- Crearea workflow-urilor cu compunere de operatii
- Documentarea codului cu XML comments

### Limitari ale AI identificate
- Necesita verificare manuala a regex-urilor pentru validare
- Uneori genereaza cod cu dependente de pachete inexistente
- Seed data trebuie verificat sa respecte regulile de validare

### Prompturi Utile
```
"Creeaza un Value Object pentru [concept] cu validare folosind TryCreate pattern"
```
```
"Implementeaza un workflow DDD cu state machine pentru [proces de business]"
```
```
"Creeaza operatii Transform pentru tranzitia de la [stare1] la [stare2]"
```

## Design Decisions

### De ce State Machine Pattern?
- Previne tranzitii invalide de stare
- Fiecare stare are date specifice garantate
- Compile-time safety prin pattern matching

### De ce TryCreate in loc de constructori?
- Nu arunca exceptii pentru date invalide
- Returneaza erori descriptive
- Permite validare batch fara try-catch

### De ce SQLite local?
- Usor de testat fara infrastructura
- Portabil pentru development
- Pregatit pentru migrare la Azure SQL

## Curieri Suportati

| Cod | Nume | Zile Estimate |
|-----|------|---------------|
| DHL | DHL Express | 3 |
| FEDEX | FedEx | 2 |
| UPS | UPS | 3 |
| DPD | DPD | 4 |
| GLS | GLS | 4 |
| CARGUS | Cargus | 2 |
| FAN_COURIER | Fan Courier | 1 |
| SAMEDAY | Sameday | 1 |

## License

MIT License - Proiect educational PSSC 2024-2025

# RAPORT DE VERIFICARE - SHOPVRG BACKEND

## Rezumat Executiv

| Cerință | Status | Detalii |
|---------|--------|---------|
| Workflows cu stări | ✅ DA | 3 workflows, fiecare cu 4-6 stări explicite |
| Transform functions | ✅ DA | 7 operații de transformare între stări |
| Comunicare între workflows | ✅ DA | Via repositories și verificări de stare |
| Conexiune bază de date | ✅ DA | EF Core + Azure SQL |
| Service Bus | ✅ DA | Azure Service Bus cu 3 topics |
| Evenimente generate | ✅ DA | 7 tipuri de evenimente (success + failure) |
| Comunicare asincronă | ✅ DA | async/await peste tot |

**Toate cerințele sunt îndeplinite în codebase-ul actual.**

---

## 1. WORKFLOWS CU STĂRI ȘI TRANSFORM FUNCTIONS

### 1.1 Workflows Existente

| Workflow | Fișier | Scop |
|----------|--------|------|
| PlaceOrderWorkflow | `ShopVRG.Domain/Workflows/PlaceOrderWorkflow.cs` | Orchestrează crearea comenzii |
| ProcessPaymentWorkflow | `ShopVRG.Domain/Workflows/ProcessPaymentWorkflow.cs` | Procesează plățile |
| ShipOrderWorkflow | `ShopVRG.Domain/Workflows/ShipOrderWorkflow.cs` | Gestionează expedierea |

### 1.2 State Machines (Mașini de Stare)

#### Order State Machine
**Fișier:** `ShopVRG.Domain/Models/Entities/OrderStates.cs`

```
UnvalidatedOrder
    → ValidatedOrder (via ValidateOrderOperation)
    → StockCheckedOrder (via CheckStockOperation)
    → PendingOrder (via PlaceOrderOperation)
    → PlacedOrder (după confirmarea plății)

SAU → InvalidOrder (la orice pas dacă validarea eșuează)
```

**Stări:**
- `UnvalidatedOrder` - Stare inițială din request API
- `ValidatedOrder` - Toate inputurile validate, convertite în value objects
- `StockCheckedOrder` - Disponibilitatea stocului verificată
- `PendingOrder` - Comandă creată, așteaptă confirmarea plății
- `PlacedOrder` - Stare finală de succes
- `InvalidOrder` - Stare de eroare cu motivele validării

#### Payment State Machine
**Fișier:** `ShopVRG.Domain/Models/Entities/PaymentStates.cs`

```
UnvalidatedPayment
    → ValidatedPayment (via ValidatePaymentOperation)
    → ProcessedPayment (via ProcessPaymentOperation)

SAU → InvalidPayment (la orice pas dacă validarea eșuează)
```

**Stări:**
- `UnvalidatedPayment` - Input brut de plată
- `ValidatedPayment` - Detalii plată validate
- `ProcessedPayment` - Stare finală de succes, tranzacție procesată
- `InvalidPayment` - Stare de eroare

#### Shipping State Machine
**Fișier:** `ShopVRG.Domain/Models/Entities/ShippingStates.cs`

```
UnvalidatedShipping
    → ValidatedShipping (via ValidateShippingOperation)
    → ShippedOrder (via ShipOrderOperation)

SAU → InvalidShipping (la orice pas dacă validarea eșuează)
```

**Stări:**
- `UnvalidatedShipping` - Request brut de expediere
- `ValidatedShipping` - Detalii expediere și verificare comandă complete
- `ShippedOrder` - Stare finală de succes, comandă expediată cu tracking
- `InvalidShipping` - Stare de eroare

### 1.3 Transform Functions (Operations)

| Operație | Input → Output | Fișier |
|----------|----------------|--------|
| ValidateOrderOperation | UnvalidatedOrder → ValidatedOrder | `ShopVRG.Domain/Operations/ValidateOrderOperation.cs` |
| CheckStockOperation | ValidatedOrder → StockCheckedOrder | `ShopVRG.Domain/Operations/CheckStockOperation.cs` |
| PlaceOrderOperation | StockCheckedOrder → PendingOrder | `ShopVRG.Domain/Operations/PlaceOrderOperation.cs` |
| ValidatePaymentOperation | UnvalidatedPayment → ValidatedPayment | `ShopVRG.Domain/Operations/ValidatePaymentOperation.cs` |
| ProcessPaymentOperation | ValidatedPayment → ProcessedPayment | `ShopVRG.Domain/Operations/ProcessPaymentOperation.cs` |
| ValidateShippingOperation | UnvalidatedShipping → ValidatedShipping | `ShopVRG.Domain/Operations/ValidateShippingOperation.cs` |
| ShipOrderOperation | ValidatedShipping → ShippedOrder | `ShopVRG.Domain/Operations/ShipOrderOperation.cs` |

**Pattern utilizat:**
```csharp
// Exemplu PlaceOrderWorkflow
IOrder order = new UnvalidatedOrder(...);
order = new ValidateOrderOperation().Transform(order);
order = new CheckStockOperation(...).Transform(order);
order = new PlaceOrderOperation(...).Transform(order);
return order.ToEvent();
```

---

## 2. COMUNICARE ÎNTRE WORKFLOWS

### 2.1 Pattern de Comunicare

Workflows comunică prin **Repository pattern** - OrderRepository este bridge-ul de date între workflows.

### 2.2 Verificări Inter-Workflow

| Workflow | Verificare | Metodă Repository |
|----------|------------|-------------------|
| PaymentWorkflow | Verifică dacă comanda există | `IOrderRepository.ExistsAsync()` |
| PaymentWorkflow | Obține totalul comenzii | `IOrderRepository.GetOrderTotalAsync()` |
| ShippingWorkflow | Verifică dacă plata a fost făcută | `IOrderRepository.IsPaidAsync()` |
| ShippingWorkflow | Obține adresa de livrare | `IOrderRepository.GetShippingAddressAsync()` |

### 2.3 Coordonare prin Controllers

- `OrdersController` execută `PlaceOrderWorkflow`
- `PaymentsController` execută `ProcessPaymentWorkflow`
- `ShippingController` execută `ShipOrderWorkflow`

---

## 3. CONEXIUNE CU BAZA DE DATE

### 3.1 Tehnologie

- **ORM:** Entity Framework Core
- **Database:** Azure SQL Database
- **DbContext:** `ShopVRG.Data/ShopDbContext.cs`
- **Connection String:** `AZURE_SQL_CONNECTIONSTRING` în appsettings.json

### 3.2 Entități

| Entitate | Cheie | Câmpuri Principale |
|----------|-------|-------------------|
| ProductEntity | Code (string, max 10) | Name, Description, Category, Price, Stock, IsActive |
| OrderEntity | OrderId (string) | CustomerName, CustomerEmail, ShippingAddress, TotalPrice, Status |
| OrderLineEntity | Id (auto-increment) | OrderId (FK), ProductCode, Quantity, UnitPrice, LineTotal |
| PaymentEntity | PaymentId (string) | OrderId (FK), Amount, MaskedCardNumber, TransactionReference |
| ShipmentEntity | Id (auto-increment) | OrderId (FK), TrackingNumber, Carrier, EstimatedDelivery |

### 3.3 Repositories

| Interface | Implementare | Locație |
|-----------|--------------|---------|
| IOrderRepository | OrderRepository | `ShopVRG.Domain/Repositories/` → `ShopVRG.Data/Repositories/` |
| IProductRepository | ProductRepository | `ShopVRG.Domain/Repositories/` → `ShopVRG.Data/Repositories/` |
| IPaymentRepository | PaymentRepository | `ShopVRG.Domain/Repositories/` → `ShopVRG.Data/Repositories/` |

---

## 4. COMUNICARE PRIN SERVICE BUS

### 4.1 Infrastructură

| Component | Fișier | Descriere |
|-----------|--------|-----------|
| IEventSender | `ShopVRG.Events/IEventSender.cs` | Interfață pentru publicare evenimente |
| ServiceBusEventSender | `ShopVRG.Events.ServiceBus/ServiceBusEventSender.cs` | Implementare Azure Service Bus |
| InMemoryEventSender | `ShopVRG.Events.ServiceBus/InMemoryEventSender.cs` | Implementare pentru development |
| EventTopics | `ShopVRG.Events/EventTopics.cs` | Constante pentru topics |

### 4.2 Topics Definite

```csharp
public const string OrderPlaced = "order-placed";
public const string PaymentProcessed = "payment-processed";
public const string OrderShipped = "order-shipped";
```

### 4.3 Configurare (Program.cs)

```csharp
if (useAzureServiceBus && !string.IsNullOrEmpty(serviceBusConnectionString))
{
    // Producție - Azure Service Bus
    services.AddSingleton<IEventSender>(new ServiceBusEventSender(connectionString));
}
else
{
    // Development - InMemory
    services.AddSingleton<IEventSender>(new InMemoryEventSender());
}
```

### 4.4 Format Evenimente

- **Specificație:** CloudEvents
- **Content-Type:** `application/cloudevents+json`
- **Serializare:** JSON

---

## 5. EVENIMENTE GENERATE DE WORKFLOWS

### 5.1 Order Events
**Fișier:** `ShopVRG.Domain/Models/Events/OrderEvents.cs`

| Eveniment | Tip | Proprietăți |
|-----------|-----|-------------|
| OrderPlacedEvent | Success | OrderId, CustomerName, CustomerEmail, ShippingAddress, OrderLines[], TotalPrice, PlacedAt |
| OrderPendingPaymentEvent | Intermediate | OrderId, CustomerName, CustomerEmail, ShippingAddress, OrderLines[], TotalPrice, CreatedAt |
| OrderPlacementFailedEvent | Failure | Reasons[] |

### 5.2 Payment Events
**Fișier:** `ShopVRG.Domain/Models/Events/PaymentEvents.cs`

| Eveniment | Tip | Proprietăți |
|-----------|-----|-------------|
| PaymentProcessedEvent | Success | PaymentId, OrderId, Amount, MaskedCardNumber, TransactionReference, ProcessedAt |
| PaymentFailedEvent | Failure | OrderId, Reasons[] |

### 5.3 Shipping Events
**Fișier:** `ShopVRG.Domain/Models/Events/ShippingEvents.cs`

| Eveniment | Tip | Proprietăți |
|-----------|-----|-------------|
| OrderShippedEvent | Success | OrderId, TrackingNumber, Carrier, Destination, ShippedAt, EstimatedDelivery |
| ShippingFailedEvent | Failure | OrderId, Reasons[] |

### 5.4 Conversie la Evenimente

Fiecare state machine are extensie `.ToEvent()` care folosește pattern matching:

```csharp
public static object ToEvent(this IOrder order) => order switch
{
    PlacedOrder placed => new OrderPlacedEvent(...),
    PendingOrder pending => new OrderPendingPaymentEvent(...),
    InvalidOrder invalid => new OrderPlacementFailedEvent(invalid.Reasons),
    _ => throw new InvalidOperationException()
};
```

---

## 6. COMUNICARE ASINCRONĂ

### 6.1 Metode Repository Async

```csharp
Task SaveOrderAsync(...)
Task<bool> ExistsAsync(OrderId orderId)
Task<bool> IsPaidAsync(OrderId orderId)
Task<decimal> GetOrderTotalAsync(OrderId orderId)
Task<ShippingAddress?> GetShippingAddressAsync(OrderId orderId)
Task MarkAsPaidAsync(OrderId orderId)
Task MarkAsShippedAsync(OrderId orderId)
```

### 6.2 Event Publishing Async

```csharp
public interface IEventSender
{
    Task SendAsync<T>(string topic, T @event) where T : class;
}
```

### 6.3 Service Bus Async

```csharp
await sender.SendMessageAsync(new ServiceBusMessage(json)
{
    ContentType = "application/cloudevents+json"
});
```

### 6.4 Controllers Async

```csharp
[HttpPost]
public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
{
    // ... async operations
}
```

---

## 7. DIAGRAME ARHITECTURALE

### 7.1 Flux Order Workflow

```
┌─────────────────┐
│  API Request    │
│ (PlaceOrder)    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ UnvalidatedOrder│
└────────┬────────┘
         │ ValidateOrderOperation.Transform()
         ▼
┌─────────────────┐
│ ValidatedOrder  │
└────────┬────────┘
         │ CheckStockOperation.Transform()
         ▼
┌─────────────────┐
│StockCheckedOrder│
└────────┬────────┘
         │ PlaceOrderOperation.Transform()
         ▼
┌─────────────────┐
│  PendingOrder   │◄─────── Salvat în DB
└────────┬────────┘
         │ .ToEvent()
         ▼
┌─────────────────┐
│OrderPendingEvent│──────► Service Bus (order-placed)
└─────────────────┘
```

### 7.2 Comunicare Inter-Workflow

```
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  PlaceOrder      │     │ ProcessPayment   │     │   ShipOrder      │
│  Workflow        │     │   Workflow       │     │   Workflow       │
└────────┬─────────┘     └────────┬─────────┘     └────────┬─────────┘
         │                        │                        │
         │                        │                        │
         ▼                        ▼                        ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       OrderRepository                               │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐    │
│  │ Exists  │  │GetTotal │  │ IsPaid  │  │GetAddr  │  │MarkPaid │    │
│  └─────────┘  └─────────┘  └─────────┘  └─────────┘  └─────────┘    │
└─────────────────────────────────────────────────────────────────────┘
         │                        │                        │
         ▼                        ▼                        ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       Azure SQL Database                            │
└─────────────────────────────────────────────────────────────────────┘
```

### 7.3 Service Bus Event Flow

```
┌──────────────┐          ┌──────────────┐          ┌──────────────┐
│   Orders     │          │   Payments   │          │   Shipping   │
│  Controller  │          │  Controller  │          │  Controller  │
└──────┬───────┘          └──────┬───────┘          └──────┬───────┘
       │                         │                         │
       ▼                         ▼                         ▼
┌──────────────────────────────────────────────────────────────────┐
│                        IEventSender                              │
│                   (ServiceBusEventSender)                        │
└──────────────────────────────────────────────────────────────────┘
       │                         │                         │
       ▼                         ▼                         ▼
┌──────────────┐          ┌──────────────┐          ┌──────────────┐
│ order-placed │          │  payment-    │          │ order-shipped│
│    topic     │          │  processed   │          │    topic     │
└──────────────┘          └──────────────┘          └──────────────┘
```

---

## 8. LISTA COMPLETĂ DE FIȘIERE

### Domain Layer (`ShopVRG.Domain/`)

| Categorie | Fișier |
|-----------|--------|
| Workflows | `Workflows/PlaceOrderWorkflow.cs` |
| | `Workflows/ProcessPaymentWorkflow.cs` |
| | `Workflows/ShipOrderWorkflow.cs` |
| State Machines | `Models/Entities/OrderStates.cs` |
| | `Models/Entities/PaymentStates.cs` |
| | `Models/Entities/ShippingStates.cs` |
| Operations | `Operations/OrderOperation.cs` |
| | `Operations/ValidateOrderOperation.cs` |
| | `Operations/CheckStockOperation.cs` |
| | `Operations/PlaceOrderOperation.cs` |
| | `Operations/PaymentOperation.cs` |
| | `Operations/ValidatePaymentOperation.cs` |
| | `Operations/ProcessPaymentOperation.cs` |
| | `Operations/ShippingOperation.cs` |
| | `Operations/ValidateShippingOperation.cs` |
| | `Operations/ShipOrderOperation.cs` |
| Events | `Models/Events/OrderEvents.cs` |
| | `Models/Events/PaymentEvents.cs` |
| | `Models/Events/ShippingEvents.cs` |
| Commands | `Models/Commands/PlaceOrderCommand.cs` |
| | `Models/Commands/ProcessPaymentCommand.cs` |
| | `Models/Commands/ShipOrderCommand.cs` |
| Repositories | `Repositories/IOrderRepository.cs` |
| | `Repositories/IProductRepository.cs` |
| | `Repositories/IPaymentRepository.cs` |

### Data Layer (`ShopVRG.Data/`)

| Categorie | Fișier |
|-----------|--------|
| DbContext | `ShopDbContext.cs` |
| Repositories | `Repositories/OrderRepository.cs` |
| | `Repositories/ProductRepository.cs` |
| | `Repositories/PaymentRepository.cs` |

### Events Layer (`ShopVRG.Events/` & `ShopVRG.Events.ServiceBus/`)

| Categorie | Fișier |
|-----------|--------|
| Interface | `ShopVRG.Events/IEventSender.cs` |
| Topics | `ShopVRG.Events/EventTopics.cs` |
| Azure Implementation | `ShopVRG.Events.ServiceBus/ServiceBusEventSender.cs` |
| InMemory Implementation | `ShopVRG.Events.ServiceBus/InMemoryEventSender.cs` |

### API Layer (`ShopVRG.Api/`)

| Categorie | Fișier |
|-----------|--------|
| Controllers | `Controllers/OrdersController.cs` |
| | `Controllers/PaymentsController.cs` |
| | `Controllers/ShippingController.cs` |
| | `Controllers/ProductsController.cs` |
| Configuration | `Program.cs` |
| | `appsettings.json` |

---

## 9. PATTERN-URI ARHITECTURALE IDENTIFICATE

1. **Domain-Driven Design (DDD)** - Aggregate roots, value objects, repositories
2. **State Machine Pattern** - Tranziții explicite de stare via sealed record types
3. **Transform/Pipeline Pattern** - Operații înlănțuite transformări prin ierarhia de stări
4. **CQRS Light** - Commands din API, Events publicate pentru eventual consistency
5. **Event-Driven Architecture** - Comunicare asincronă via Service Bus topics
6. **Dependency Injection** - Parametri funcție pentru dependențe externe (testabilitate)
7. **CloudEvents Specification** - Format standardizat pentru evenimente Service Bus
8. **Repository Pattern** - Abstractizare acces date cu interfețe în Domain

---

**Document generat:** Ianuarie 2026
**Ultima verificare:** 21 Ianuarie 2026 - TOATE CERINȚELE CONFIRMATE ✅
**Proiect:** ShopVRG - PC Components Store
**Echipa:** Rusu Andrei, Plesa Valentin, Simedre Patricia
**Curs:** PSSC - Universitatea Politehnica Timișoara

---

## 10. REZULTATE VERIFICARE FINALĂ

### Fișiere verificate și confirmate:

| Categorie | Fișier | Status |
|-----------|--------|--------|
| Workflow | `PlaceOrderWorkflow.cs` | ✅ Verificat |
| Workflow | `ProcessPaymentWorkflow.cs` | ✅ Verificat |
| Workflow | `ShipOrderWorkflow.cs` | ✅ Verificat |
| State Machine | `OrderStates.cs` (6 stări) | ✅ Verificat |
| State Machine | `PaymentStates.cs` (4 stări) | ✅ Verificat |
| State Machine | `ShippingStates.cs` (4 stări) | ✅ Verificat |
| Events | `OrderEvents.cs` | ✅ Verificat |
| Events | `PaymentEvents.cs` | ✅ Verificat |
| Events | `ShippingEvents.cs` | ✅ Verificat |
| Repository | `IOrderRepository.cs` | ✅ Verificat |
| Repository | `OrderRepository.cs` | ✅ Verificat |
| Service Bus | `ServiceBusEventSender.cs` | ✅ Verificat |
| Service Bus | `EventTopics.cs` | ✅ Verificat |
| Database | `ShopDbContext.cs` | ✅ Verificat |
| Config | `Program.cs` | ✅ Verificat |

### Concluzii verificare:

1. **Workflows**: Toate 3 workflow-uri implementează pattern-ul Transform corect
2. **State Machines**: 14 stări totale, tranziții explicite cu sealed records
3. **Evenimente**: 6 tipuri de evenimente generate prin metoda `.ToEvent()`
4. **Service Bus**: Azure Service Bus cu CloudEvents, fallback InMemory
5. **Comunicare Inter-Workflow**: Via `IOrderRepository` - ExistsAsync, IsPaidAsync, GetOrderTotalAsync
6. **Baza de Date**: EF Core cu Azure SQL, 5 entități mapate

**Verificarea a fost efectuată prin citirea directă a fișierelor sursă.**

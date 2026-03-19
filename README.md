# CQRS POC with MediatR

Proof of Concept che dimostra l'implementazione del pattern **CQRS (Command Query Responsibility Segregation)** con **MediatR** in ASP.NET Core.

## Stack tecnologico

- **.NET 10.0** / ASP.NET Core Web API
- **MediatR 12.3.0** — pattern request/response e pipeline behaviors
- **FluentValidation 12.1.1** — validazione dichiarativa
- **Entity Framework Core 10.0.5** con SQLite
- **Scalar** — documentazione API interattiva

## Architettura

Il progetto segue una **Clean Architecture** a 4 livelli:

```
CQRS.POC.slnx
├── src/Domain          → Entità, regole di business, eccezioni custom
├── src/Application     → Comandi, query, handler, behaviors, astrazioni
├── src/Infrastructure  → EF Core, repository, SQLite
└── src/api             → Controller, middleware, DI
```

### Separazione Command / Query

| Lato | Tipo | Descrizione |
|------|------|-------------|
| Write | `CreateProductCommand` | Crea un prodotto, passa per validazione e handler |
| Read  | `GetProductByIdQuery`  | Legge un prodotto per ID (AsNoTracking) |
| Read  | `GetProductsPagedQuery` | Lista paginata con filtro per testo |

### Pipeline MediatR

```
Request → LoggingBehavior → ValidationBehavior → Handler → Response
```

- **LoggingBehavior** — logga il nome della richiesta, il tempo di esecuzione (warn se >500ms) e gli errori
- **ValidationBehavior** — esegue i validator FluentValidation prima dell'handler; lancia `ValidationException` in caso di errori

### Repository pattern

| Interfaccia | Uso | Note |
|-------------|-----|------|
| `IProductRepository` | Write | Traccia le modifiche EF Core |
| `IProductReadRepository` | Read | Usa `AsNoTracking()`, proietta su `ProductDTO` |

## API Endpoints

| Metodo | Endpoint | Descrizione |
|--------|----------|-------------|
| `POST` | `/api/products` | Crea un nuovo prodotto |
| `GET`  | `/api/products/{id}` | Recupera un prodotto per ID |
| `GET`  | `/api/products?page=1&pageSize=10&search=` | Lista paginata con ricerca opzionale |

La documentazione interattiva è disponibile all'indirizzo `/scalar/v1` una volta avviata l'applicazione.

## Domain Model

```csharp
Product
├── Id          (Guid)
├── Name        (string, max 200)
├── Description (string, max 1000)
├── Price       (decimal, ≥ 0)
├── Stock       (int, ≥ 0)
├── IsActive    (bool)
├── CreatedAt   (DateTime)
└── UpdatedAt   (DateTime)

Metodi: Create(), UpdatePrice(), DecreaseStock(), Deactivate()
```

Le proprietà hanno setter privati per garantire l'immutabilità dopo la creazione. Le violazioni dei vincoli lanciano eccezioni di dominio (`DomainException`).

## Gestione degli errori

Il middleware `GlobalExceptionHandler` intercetta tutte le eccezioni non gestite e le mappa a risposte `ProblemDetails`:

| Eccezione | HTTP Status |
|-----------|-------------|
| `ValidationException` | 422 Unprocessable Entity |
| `NotFoundException` | 404 Not Found |
| `DomainException` | 400 Bad Request |
| Altre | 500 Internal Server Error |

## Avvio rapido

```bash
# Clona il repository
git clone <repo-url>
cd cqrs-mediatR

# Avvia l'API (il database SQLite viene creato automaticamente)
dotnet run --project src/api

# Apri la documentazione API
# http://localhost:<port>/scalar/v1
```

> Non sono necessarie migrazioni: il database viene creato automaticamente all'avvio tramite `EnsureCreated()`.

## Struttura della soluzione

```
src/
├── Domain/
│   ├── Entities/Product.cs
│   └── Exceptions/Exceptions.cs
├── Application/
│   ├── Abstractions/          ← Interfacce repository
│   ├── Common/
│   │   ├── Behaviors/         ← LoggingBehavior, ValidationBehavior
│   │   └── Models/            ← ProductDTO, PagedResult<T>
│   └── Products/
│       ├── Commands/          ← CreateProductCommand + Validator + Handler
│       └── Queries/           ← GetProductByIdQuery, GetProductsPagedQuery
├── Infrastructure/
│   ├── Persistence/           ← AppDbContext, ProductRepository, ProductReadRepository
│   └── DependencyInjection.cs
└── api/
    ├── Controllers/           ← ProductController
    ├── Middleware/            ← GlobalExceptionHandler
    └── Program.cs
```

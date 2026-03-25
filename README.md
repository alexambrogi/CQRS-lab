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

---

## Security — SAST e DAST in CI/CD

La pipeline di sicurezza è composta da due stage sequenziali: prima SAST con SonarQube, poi DAST con OWASP ZAP. Il DAST parte solo se il SAST ha avuto esito positivo (`workflow_run` con `conclusion == 'success'`).

### SAST — SonarQube (self-hosted via tunnel)

SonarQube gira in locale come container Docker. Poiché GitHub Actions non può raggiungere direttamente un'istanza locale, si espone la porta `9000` del container tramite un tunnel pubblico (es. **ngrok** o **Cloudflare Tunnel**), il cui URL viene salvato come secret `SONAR_HOST_URL`.

```
GitHub Actions runner
       │
       │  HTTPS (SONAR_HOST_URL)
       ▼
  Tunnel pubblico  ──►  localhost:9000  ──►  Container SonarQube
```

**Step eseguiti dalla workflow `sonarqube.yml`:**

1. Checkout del repository (full history con `fetch-depth: 0`)
2. Setup .NET 10 + installazione di `dotnet-sonarscanner` e `dotnet-coverage`
3. `sonarscanner begin` — apre la sessione di analisi con chiave progetto `cqrs-lab`
4. `dotnet build` — compila il codice sorgente
5. `dotnet-coverage collect dotnet test` — esegue i test raccogliendo la coverage in formato XML
6. `sonarscanner end` — invia i risultati a SonarQube

**Secrets richiesti:**

| Secret | Valore |
|--------|--------|
| `SONAR_TOKEN` | Token di autenticazione generato da SonarQube |
| `SONAR_HOST_URL` | URL pubblico del tunnel che punta a `localhost:9000` |

---

### DAST — OWASP ZAP (API esposta via Cloudflare Tunnel)

Il DAST viene eseguito da GitHub Actions puntando all'API ASP.NET Core in esecuzione in locale sulla porta `5244` (HTTP). Per renderla raggiungibile dall'esterno si usa **cloudflared** (`cloudflare tunnel`), che crea un tunnel HTTPS pubblico verso `localhost:5244`. L'URL generato viene salvato come secret `APP_URL`.

```
GitHub Actions runner
       │
       │  HTTPS (APP_URL)
       ▼
  Cloudflare Tunnel  ──►  localhost:5244  ──►  ASP.NET Core API
```

**Step eseguiti dalla workflow `dast.yml`:**

1. Checkout del repository
2. Fix permessi cartella di lavoro per OWASP ZAP
3. **ZAP Baseline Scan** sull'endpoint `$APP_URL/scalar/v1` con regole custom in `.zap/rules.tsv`
4. Upload del report (`report_html.html`, `report_json.json`) come artifact con retention 30 giorni

**Secrets richiesti:**

| Secret | Valore |
|--------|--------|
| `APP_URL` | URL pubblico del tunnel Cloudflare che punta a `localhost:5244` |

---

### Esecuzione locale dei tunnel

Prima di fare push su `master` o `develop`, assicurarsi che entrambi i tunnel siano attivi:

```bash
# Tunnel SonarQube (porta 9000 del container Docker)
# Con ngrok:
ngrok http 9000

# Con cloudflared:
cloudflared tunnel --url http://localhost:9000

# Tunnel API ASP.NET Core (porta 5244)
cloudflared.exe tunnel --url http://localhost:5244
```

Aggiornare i secrets GitHub (`SONAR_HOST_URL` e `APP_URL`) con i nuovi URL ogni volta che i tunnel vengono ricreati, poiché gli URL cambiano ad ogni avvio (a meno di usare tunnel named con dominio fisso).

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

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run the API (http://localhost:5244, https://localhost:7134)
dotnet run --project src/api

# Build a specific project
dotnet build src/Application
```

No test project exists yet in this repository.

## Architecture

This is a **CQRS POC** using MediatR with a strict 4-layer Clean Architecture. Dependencies flow inward: `API → Application ← Infrastructure`, with `Domain` having no dependencies.

### Layer responsibilities

- **Domain** (`src/Domain`) — `Product` aggregate root with private setters, factory method `Product.Create()`, and business operations (`UpdatePrice`, `DecreaseStock`, `Deactivate`). All domain violations throw `DomainException`; missing entities throw `NotFoundException`.
- **Application** (`src/Application`) — Commands, queries, handlers, pipeline behaviors, and repository interfaces. `AssemblyMarker` is the anchor class used for MediatR and FluentValidation assembly scanning. No infrastructure references.
- **Infrastructure** (`src/Infrastructure`) — EF Core + SQLite implementations of `IProductRepository` (write, change-tracked) and `IProductReadRepository` (read, `AsNoTracking`, projects to `ProductDTO`). Registered via `AddInfrastructure()`. Database is created automatically on startup with `EnsureCreated()`.
- **API** (`src/api`) — Controllers, `GlobalExceptionHandler` middleware (maps domain exceptions to HTTP status codes), and DI wiring in `Program.cs`.

### MediatR pipeline order

```
Request → LoggingBehavior → ValidationBehavior → Handler
```

This order is intentional: logging captures failed validation attempts. Behaviors are registered explicitly in `Program.cs` — order matters.

### Adding a new feature (e.g. `UpdateProduct`)

1. **Domain** — add a method on `Product` if business logic is involved.
2. **Application** — create a `record` command/query + `IRequestHandler` + `AbstractValidator` in `src/Application/Products/Commands/` or `Queries/`. All three can live in a single file.
3. **Infrastructure** — add methods to `ProductRepository` / `ProductReadRepository` if new data access is needed.
4. **API** — add an endpoint to `ProductController`.

No extra DI registration is needed for handlers or validators — both are discovered via assembly scanning from `AssemblyMarker`.

### Error handling contract

`GlobalExceptionHandler` maps exceptions to HTTP responses:

| Exception | Status |
|-----------|--------|
| `ValidationException` (FluentValidation) | 422 |
| `NotFoundException` | 404 |
| `DomainException` | 400 |
| Other | 500 |

All responses are `ProblemDetails`.

[Back to README](../README.md)

## Technical Implementation

This document describes the technical decisions, architecture, and implementation details of the Sales feature delivered in this project.

---

## Architecture

The solution follows **Clean Architecture** with strict layer separation:

```
src/
├── Domain          — Entities, interfaces, domain events, business rules
├── Application     — Use cases (CQRS via MediatR), DTOs, validators, AutoMapper profiles
├── ORM             — EF Core DbContext, entity configurations, repository implementations, migrations
├── IoC             — Dependency injection wiring
├── Common          — Cross-cutting concerns: security, validation pipeline, logging
└── WebApi          — ASP.NET Core controllers, request/response DTOs, middleware
```

Dependencies flow inward: `WebApi → Application → Domain`. The `ORM` layer implements interfaces defined in `Domain` and is registered by `IoC`.

---

## Domain Layer

### Entities

#### `Sale`

Aggregate root representing a sale transaction. Key behaviors:

- **`SetItems(items)`** — Replaces all items and recalculates `TotalAmount`.
- **`AddItem(item)`** — Appends a single item and recalculates `TotalAmount`.
- **`Cancel()`** — Marks the sale as cancelled and sets `UpdatedAt`.
- **`CancelItem(itemId)`** — Finds and cancels a single item by ID, then recalculates `TotalAmount`. Throws `KeyNotFoundException` if the item does not exist.
- **`Update(customerName, branchName, saleDate)`** — Updates mutable header fields.
- **`RecalculateTotal()`** — Private method that sums `TotalAmount` of all non-cancelled items.
- `TotalAmount` has a `private set` — it can only be changed internally by the aggregate.

#### `SaleItem`

Value-like entity within the `Sale` aggregate. Key behaviors:

- Constructor auto-generates a unique `Id` (`Guid.NewGuid()`).
- **`SetQuantity(quantity)`** — Validates the quantity limit (max 20), applies the correct discount tier, and recalculates `TotalAmount`.
- **`Cancel()`** — Marks the item as cancelled.
- **`CalculateDiscount(quantity)`** — Pure function implementing the business rules:
  - `quantity >= 10` → 20% discount
  - `quantity >= 4` → 10% discount
  - `quantity < 4` → 0% (no discount allowed)
  - `quantity > 20` → throws `InvalidOperationException`

#### External Identity Pattern

`Sale` stores denormalized references to external entities following DDD principles:

```
CustomerId    (Guid)   — reference to the Customer aggregate
CustomerName  (string) — denormalized name at time of sale
BranchId      (Guid)   — reference to the Branch aggregate
BranchName    (string) — denormalized name at time of sale
```

The same pattern applies to `SaleItem.ProductId` and `SaleItem.ProductName`. External data is captured at sale creation time and is not affected by changes in the source systems.

### Repository Interface

`ISaleRepository` is defined in the Domain layer:

```csharp
Task<Sale>  CreateAsync(Sale sale, CancellationToken ct);
Task<Sale?> GetByIdAsync(Guid id, CancellationToken ct);
Task<Sale?> GetBySaleNumberAsync(int saleNumber, CancellationToken ct);
Task<(IEnumerable<Sale> Sales, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct);
Task<Sale>  UpdateAsync(Sale sale, CancellationToken ct);
Task<bool>  DeleteAsync(Guid id, CancellationToken ct);
Task<int>   GetNextSaleNumberAsync(CancellationToken ct);
```

### Domain Events

Domain events are raised as structured log entries (via `ILogger`) during state transitions. They are defined as plain classes in `Domain/Events/` and are ready to be forwarded to a message broker:

| Event | Published by |
|---|---|
| `SaleCreatedEvent` | `CreateSaleHandler` |
| `SaleModifiedEvent` | `UpdateSaleHandler` |
| `SaleCancelledEvent` | `CancelSaleHandler` |
| `ItemCancelledEvent` | `CancelSaleItemHandler` |

---

## Application Layer

### CQRS Pattern

Each use case is implemented as a MediatR `IRequest<TResult>` + `IRequestHandler<TRequest, TResult>` pair. Validation is performed inside each handler using FluentValidation before business logic executes.

| Use Case | Command | Result |
|---|---|---|
| Create Sale | `CreateSaleCommand` | `CreateSaleResult` |
| Get Sale | `GetSaleCommand` | `GetSaleResult` |
| Update Sale | `UpdateSaleCommand` | `UpdateSaleResult` |
| Cancel Sale | `CancelSaleCommand` | `CancelSaleResult` |
| Cancel Sale Item | `CancelSaleItemCommand` | `CancelSaleItemResult` |
| Delete Sale | `DeleteSaleCommand` | `DeleteSaleResult` |
| List Sales | `ListSalesCommand` | `ListSalesResult` |

### Validation Pipeline

FluentValidation rules are applied at two levels:

1. **WebApi level** — `CreateSaleRequestValidator` / `UpdateSaleRequestValidator` validate the incoming HTTP request before it reaches MediatR.
2. **Application level** — `CreateSaleCommandValidator` / `UpdateSaleCommandValidator` validate the command inside the handler.

The MediatR `ValidationBehavior<TRequest, TResponse>` pipeline behavior ensures that any unhandled `ValidationException` is caught globally.

### AutoMapper Profiles

Each use case defines its own profile, following the single-responsibility principle:

```
CreateSaleProfile  — Sale → CreateSaleResult, SaleItem → CreateSaleItemResult
GetSaleProfile     — Sale → GetSaleResult, SaleItem → GetSaleItemResult
UpdateSaleProfile  — Sale → UpdateSaleResult, SaleItem → UpdateSaleItemResult
CancelSaleProfile  — Sale → CancelSaleResult
ListSalesProfile   — Sale → ListSaleItemResult
```

WebApi profiles map between HTTP request/response DTOs and Application commands/results.

---

## ORM Layer

### Entity Configuration (Fluent API)

| Entity | Table | Key constraints |
|---|---|---|
| `Sale` | `Sales` | `SaleNumber` has a unique index |
| `SaleItem` | `SaleItems` | FK to `Sales(Id)` with cascade delete |

Numeric columns use `numeric(18,2)` for monetary values and `numeric(5,4)` for discount ratios. Enum columns (`Status`, `Role`) are stored as `character varying(20)` strings.

### SaleRepository

`SaleRepository` uses EF Core with `Include(s => s.Items)` on all read operations to load the full aggregate in a single query. The `GetAllAsync` method applies server-side pagination via `Skip`/`Take` and returns both the page data and the total count in a single round-trip.

### Migrations

| Migration | Description |
|---|---|
| `20241014011203_InitialMigrations` | Creates the `Users` table |
| `20241015120000_AddSalesTables` | Creates `Sales` and `SaleItems` tables with indexes and FK |
| `20241015130000_AddUserTimestamps` | Adds `CreatedAt` and `UpdatedAt` columns to `Users` |

Migrations are applied automatically on application startup via `db.Database.Migrate()` in `Program.cs`.

---

## WebApi Layer

### Exception Middleware

`ValidationExceptionMiddleware` intercepts exceptions and returns structured JSON responses:

| Exception type | HTTP Status |
|---|---|
| `FluentValidation.ValidationException` | `400 Bad Request` |
| `KeyNotFoundException` | `404 Not Found` |
| `InvalidOperationException` | `409 Conflict` |

### SalesController

All routes are under `/api/sales`. The controller receives HTTP requests, validates them, maps to Application commands via AutoMapper, dispatches through MediatR, and wraps responses in `ApiResponseWithData<T>`.

```
POST   /api/sales
GET    /api/sales
GET    /api/sales/{id}
PUT    /api/sales/{id}
PATCH  /api/sales/{id}/cancel
PATCH  /api/sales/{id}/items/{itemId}/cancel
DELETE /api/sales/{id}
```

---

## Infrastructure / Docker

The application is containerized and orchestrated with Docker Compose. The `docker-compose.yml` defines four services:

| Service | Image | Port | Description |
|---|---|---|---|
| `webapi` | Built from Dockerfile | `8080` | ASP.NET Core 8 API |
| `database` | `postgres:13` | `5432` | Primary relational database |
| `nosql` | `mongo:8.0` | `27017` | Document store (available for future use) |
| `cache` | `redis:7.4.1-alpine` | `6379` | Cache layer (available for future use) |

The WebApi service uses `depends_on` with `condition: service_healthy` tied to a PostgreSQL `pg_isready` healthcheck, ensuring the API only starts after the database is ready to accept connections.

---

## Tests

### Structure

All unit tests reside in `tests/Ambev.DeveloperEvaluation.Unit/` and are organized by layer:

```
tests/Ambev.DeveloperEvaluation.Unit/
├── Application/
│   ├── TestData/                   — Bogus-based test data generators
│   ├── AuthenticateUserHandlerTests.cs
│   ├── CreateUserHandlerTests.cs
│   ├── DeleteUserHandlerTests.cs
│   ├── GetUserHandlerTests.cs
│   ├── CreateSaleHandlerTests.cs
│   ├── UpdateSaleHandlerTests.cs
│   ├── CancelSaleHandlerTests.cs
│   ├── CancelSaleItemHandlerTests.cs
│   ├── DeleteSaleHandlerTests.cs
│   ├── GetSaleHandlerTests.cs
│   └── ListSalesHandlerTests.cs
└── Domain/
    ├── Entities/
    │   ├── TestData/               — Entity test data generators
    │   ├── SaleTests.cs
    │   ├── SaleItemTests.cs
    │   └── UserTests.cs
    ├── Specifications/
    │   └── ActiveUserSpecificationTests.cs
    └── Validation/
        ├── CreateSaleValidatorTests.cs
        ├── EmailValidatorTests.cs
        ├── PasswordValidatorTests.cs
        ├── PhoneValidatorTests.cs
        └── UserValidatorTests.cs
```

### Coverage Summary

| Area | Tests |
|---|---|
| `SaleItem` entity — discount tiers, boundaries, SetQuantity, Cancel | 12 |
| `Sale` entity — Cancel, CancelItem, SetItems, AddItem, Update, totals | 11 |
| `User` entity — Activate, Suspend, Validate | 3 |
| `CreateSaleHandler` | 4 |
| `UpdateSaleHandler` | 3 |
| `CancelSaleHandler` | 3 |
| `CancelSaleItemHandler` | 5 |
| `GetSaleHandler` | 4 |
| `DeleteSaleHandler` | 3 |
| `ListSalesHandler` | 4 |
| `CreateUserHandler` | 4 |
| `GetUserHandler` | 4 |
| `DeleteUserHandler` | 4 |
| `AuthenticateUserHandler` | 5 |
| `CreateSaleCommandValidator` | 8 |
| `ActiveUserSpecification` | 3 |
| Domain validators (Email, Password, Phone, User) | 32 |
| **Total** | **130** |

### Test Tools

| Tool | Purpose |
|---|---|
| **xUnit** | Test framework |
| **NSubstitute** | Mock dependencies (repositories, mappers, loggers) |
| **Bogus** | Generate realistic fake data for entities and commands |
| **FluentAssertions** | Expressive assertion syntax |
| **FluentValidation.TestHelper** | Validate specific fields with `ShouldHaveValidationErrorFor` |

<br/>
<div style="display: flex; justify-content: space-between;">
  <a href="./overview.md">Previous: Overview</a>
  <a href="./sales-api.md">Next: Sales API</a>
</div>

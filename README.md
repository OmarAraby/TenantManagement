# Mini SaaS Tenant Management API

A multi-tenant REST API built with **ASP.NET Core (.NET 10)**, **EF Core 10** and **SQL Server**.
Tenants and their users are managed through a single API where **every user query is automatically
scoped to the calling tenant** by an EF Core global query filter.

---

## Quick start

### Option A — Docker (single command)

```bash
docker compose up --build
```

Brings up SQL Server 2022 and the API, waits for the database to pass its healthcheck, applies
migrations on startup, then serves:

| | URL |
|---|---|
| Swagger UI | http://localhost:8080/swagger |
| Hangfire dashboard | http://localhost:8080/hangfire |

### Option B — Local (SQL Server LocalDB)

```bash
dotnet ef database update -p src/TenantManagement.Infrastructure -s src/TenantManagement.Api
dotnet run --project src/TenantManagement.Api --launch-profile https
```

| | URL |
|---|---|
| Swagger UI | https://localhost:7193/swagger |
| Hangfire dashboard | https://localhost:7193/hangfire |

The default connection string points at `(localdb)\MSSQLLocalDB`, database `TenantManagement`.
Override it with user-secrets or the `ConnectionStrings__DefaultConnection` environment variable.

Requires the .NET 10 SDK. `dotnet tool install --global dotnet-ef` if `dotnet ef` is missing.

---

## Using the API

Every `/api/users` request must carry the tenant header:

```
X-Tenant-Id: <tenant guid>
```

Create a tenant first, then use its returned `id` as the header value.

```bash
# 1. create a tenant
curl -X POST http://localhost:8080/api/tenants \
  -H 'Content-Type: application/json' \
  -d '{"name":"Contoso Ltd","slug":"contoso"}'

# 2. create a user inside it
curl -X POST http://localhost:8080/api/users \
  -H 'Content-Type: application/json' \
  -H 'X-Tenant-Id: 20198240-6801-48b7-9af8-3e06ea24448a' \
  -d '{"fullName":"Ada Lovelace","email":"ada@contoso.com","role":"Admin"}'
```

`requests.http` at the repository root contains every endpoint plus the failure cases
(duplicate slug, cross-tenant access, malformed header, invalid role).

### Endpoints

| Method | Endpoint | Tenant header | Description |
|--------|----------|---------------|-------------|
| POST | `/api/tenants` | no | Create a tenant |
| GET | `/api/tenants/{id}` | no | Get tenant by id |
| GET | `/api/users` | **yes** | List users in the current tenant |
| POST | `/api/users` | **yes** | Create a user in the current tenant |
| PUT | `/api/users/{id}` | **yes** | Update a user in the current tenant |
| DELETE | `/api/users/{id}` | **yes** | Soft-delete (`IsActive = false`) |

### Response shape

Every response — success and failure alike — uses the same envelope:

```jsonc
// 200
{ "success": true,  "message": "Users retrieved successfully", "data": [ ... ], "errors": [] }

// 409
{ "success": false, "message": "A tenant with the same slug already exists.", "data": null, "errors": [] }

// 400
{ "success": false, "message": "One or more validation errors occurred.", "data": null,
  "errors": ["Name: 'Name' must not be empty."] }
```

| Status | When |
|--------|------|
| 400 | validation failure, malformed body, missing or invalid `X-Tenant-Id` |
| 403 | a write that would place a row in another tenant |
| 404 | resource not found **or** belonging to another tenant |
| 409 | duplicate tenant slug, duplicate email within a tenant |
| 500 | unexpected fault — message is generic, details logged server-side |

---

## Architecture

Onion architecture; the dependency rule points inward.

```
TenantManagement.Core            entities, domain exceptions, abstractions   (no dependencies)
TenantManagement.Application     DTOs, services, validators                  -> Core
TenantManagement.Infrastructure  EF Core, repositories, unit of work         -> Application, Core
TenantManagement.Api             controllers, middleware, Hangfire           -> all of the above
```

`Api` references `Infrastructure` only so `Program.cs` can call `AddInfrastructure(...)`. No EF
Core type appears in a controller or a service — the application layer talks to `IUnitOfWork`
and the repository interfaces defined in `Core`.

### Multi-tenancy

Three pieces:

1. **`TenantResolutionMiddleware`** reads `X-Tenant-Id`, parses it, and sets the request-scoped
   `TenantContext`. A malformed header is a 400.
2. **`ITenantContext` / `ITenantContextSetter`** are split. Services and the `DbContext` receive
   the read-only half; only the middleware can set the tenant. A service therefore cannot
   re-scope itself mid-request.
3. **A global query filter** on `User` narrows every query to the current tenant:

```csharp
builder.Entity<User>()
    .HasQueryFilter(u => u.TenantId == CurrentTenantId && u.IsActive);
```

Two details in that design are load-bearing and easy to get wrong:

**The filter references the `DbContext`, not a captured object.** EF caches the model per context
*type*. If the filter closed over an injected service held by a configuration class, that service
would be captured once when the model was built and every later request would silently run under
the **first** request's tenant. Referencing `CurrentTenantId` (a property on `AppDbContext`) makes
EF re-evaluate it per instance.

**`GetByIdAsync` does not use `DbSet.FindAsync`.** `Find` checks the change tracker before
querying. An untracked lookup is filtered correctly, but if the entity is *already tracked*, Find
returns it with the filter never evaluated — handing back another tenant's row. The generic
repository uses an explicit filtered query instead:

```csharp
Entities.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
```

Defence in depth: `SaveChanges` stamps `TenantId` on new users from the current scope, and throws
`ForbiddenException` if a write would create or move a row outside the current tenant.

Because another tenant's rows are *invisible* rather than *forbidden*, a cross-tenant request
returns **404, not 403** — a 403 would confirm the resource exists.

### Data access

Generic Repository + Unit of Work, as specified.

- `IGenericRepository<T>` — CRUD surface shared by all entities.
- `IUserRepository` / `ITenantRepository` — add one entity-specific lookup each
  (`EmailExistsAsync`, `SlugExistsAsync`).
- `IUnitOfWork` — owns the transaction boundary and exposes both repositories.

Repositories accept `Expression<Func<T,bool>>` predicates but **never expose `IQueryable`**. An
escaping `IQueryable` would let callers compose provider-specific query trees, leak the
persistence technology upward, and make the seam untestable without a database.

### Background job

Hangfire runs `active-user-count` every minute, logging the active user count per tenant.
Dashboard at `/hangfire`.

`[DisableConcurrentExecution]` prevents overlapping runs; `[AutomaticRetry(Attempts = 2)]` covers
transient database failures.



---
## Project layout

```
src/
  TenantManagement.Core/            Entities, Enums, Exceptions, Interfaces, Models
  TenantManagement.Application/     Common (ApiResponse), DTOs, Interfaces, Mapping,
                                    Services, Validation
  TenantManagement.Infrastructure/  MultiTenancy, Persistence (Context, Configurations,
                                    Repositories, Migrations), DependencyInjection
  TenantManagement.Api/             Controllers, Filters, Jobs, Middleware, OpenApi, Program.cs
tests/
  TenantManagement.Tests/           (empty)
Dockerfile  docker-compose.yml  requests.http
```

# TaskFlow API

A lightweight task-management REST API built with **.NET 8** to demonstrate Clean Architecture principles, repository pattern, and DTO-based API design.

## Overview

TaskFlow exposes CRUD endpoints for managing tasks (create, list, update status, delete). The codebase is organized into clear layers to keep domain logic independent from infrastructure and API concerns:

```
TaskFlow.Api/
├── Controllers/       # HTTP endpoints (thin, delegate to repository/services)
├── Domain/            # Entities and repository contracts (no external dependencies)
├── Infrastructure/     # EF Core DbContext + repository implementation
├── DTOs/              # Request/response contracts, decoupled from domain entities
├── Program.cs         # Composition root / DI wiring
└── appsettings.json
```

## Tech stack

- C# / .NET 8
- ASP.NET Core Web API
- Entity Framework Core (SQLite for local/dev persistence)
- Swagger / OpenAPI for interactive docs
- Repository pattern + dependency injection

## Design notes

- **Domain isolation**: `Domain/` has zero dependencies on EF Core or ASP.NET — entities and the `ITaskRepository` contract are plain C#.
- **DTOs at the boundary**: controllers never expose domain entities directly; `CreateTaskDto` / `TaskDto` shape what crosses the wire.
- **Swap-friendly persistence**: `ITaskRepository` is injected via DI, so the EF Core/SQLite implementation in `Infrastructure/` could be replaced (e.g., with SQL Server or an in-memory store for tests) without touching controllers or domain logic.

## Running locally

```bash
dotnet restore
dotnet run --project TaskFlow.Api
```

Swagger UI will be available at `https://localhost:5001/swagger`.

## Endpoints

| Method | Route              | Description              |
|--------|---------------------|--------------------------|
| GET    | /api/tasks           | List all tasks           |
| GET    | /api/tasks/{id}       | Get a task by id          |
| POST   | /api/tasks           | Create a new task         |
| PUT    | /api/tasks/{id}/status | Update a task's status   |
| DELETE | /api/tasks/{id}       | Delete a task             |

## Possible next steps

- Add authentication (JWT) and per-user task ownership
- Add pagination/filtering to the list endpoint
- Add an integration test project using `WebApplicationFactory`

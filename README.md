# TaskFlow API

![CI](https://github.com/AlexandreMelloAssis/taskflow-api/actions/workflows/ci.yml/badge.svg)

A lightweight task-management REST API built with **.NET 8** to demonstrate Clean Architecture principles, repository pattern, and DTO-based API design.

## Overview

TaskFlow exposes CRUD endpoints for managing tasks (create, list, update status, delete). The codebase is organized into clear layers to keep domain logic independent from infrastructure and API concerns:

```
TaskFlow.Api/
├── Controllers/       # HTTP endpoints (thin, delegate to controller/repository)
├── Domain/            # Entities and repository contracts (no external dependencies)
├── Infrastructure/    # EF Core DbContext + repository implementation
├── DTOs/              # Request/response contracts, decoupled from domain entities
├── Program.cs         # Composition root / DI wiring
└── appsettings.json

tests/TaskFlow.Api.Tests/
├── TaskRepositoryTests.cs     # Repository behavior against EF Core InMemory
└── TasksControllerTests.cs    # Controller behavior against a fake repository
```

## Tech stack

- C# / .NET 8
- ASP.NET Core Web API
- Entity Framework Core — **SQLite** for local dev, **PostgreSQL** in Docker (swappable via config)
- Swagger / OpenAPI for interactive docs
- xUnit + EF Core InMemory for unit tests
- Docker + Docker Compose
- GitHub Actions CI (build + test on every push/PR)

## Design notes

- **Domain isolation**: `Domain/` has zero dependencies on EF Core or ASP.NET — entities and the `ITaskRepository` contract are plain C#.
- **DTOs at the boundary**: controllers never expose domain entities directly; `CreateTaskDto` / `TaskDto` shape what crosses the wire.
- **Swap-friendly persistence**: the EF Core provider is chosen at startup based on `Database:Provider` in configuration (`Sqlite` locally, `Postgres` in Docker) — controllers and domain logic never change.
- **Tested at two levels**: repository tests run against a real `DbContext` (EF Core InMemory) to catch mapping/behavior issues; controller tests use a fake repository to isolate HTTP-layer logic (validation, status codes).

## Running locally (SQLite, no Docker)

```bash
dotnet restore
dotnet run --project TaskFlow.Api.csproj
```

Swagger UI will be available at `https://localhost:5001/swagger`.

## Running with Docker Compose (API + PostgreSQL)

```bash
docker compose up --build
```

This starts the API on `http://localhost:8080` backed by a PostgreSQL container (`db`), with a named volume so data survives restarts.

## Running tests

```bash
dotnet test tests/TaskFlow.Api.Tests/TaskFlow.Api.Tests.csproj
```

CI runs this same command on every push and pull request to `main` (see `.github/workflows/ci.yml`).

## Endpoints

| Method | Route                   | Description              |
|--------|-------------------------|--------------------------|
| GET    | /api/tasks              | List all tasks           |
| GET    | /api/tasks/{id}         | Get a task by id         |
| POST   | /api/tasks              | Create a new task        |
| PUT    | /api/tasks/{id}/status  | Update a task's status   |
| DELETE | /api/tasks/{id}         | Delete a task            |

## Related project

[taskflow-web](https://github.com/AlexandreMelloAssis/taskflow-web) — Angular 17 front end that consumes this API.

## Possible next steps

- Add authentication (JWT) and per-user task ownership
- Add pagination/filtering to the list endpoint
- Replace `EnsureCreated()` with proper EF Core migrations for production use
- Add an end-to-end integration test using `WebApplicationFactory`

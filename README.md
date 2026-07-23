# TaskFlow API

![CI](https://github.com/AlexandreMelloAssis/taskflow-api/actions/workflows/ci.yml/badge.svg)

A lightweight task-management REST API built with **.NET 8** to demonstrate Clean Architecture principles, repository pattern, DTO-based API design, and event-driven integration.

## Overview

TaskFlow exposes CRUD endpoints for managing tasks (create, list, update status, delete). The codebase is organized into clear layers to keep domain logic independent from infrastructure and API concerns:

```
TaskFlow.Api/
├── Controllers/       # HTTP endpoints (thin, delegate to repository/messaging)
├── Domain/            # Entities, repository contracts, domain events
├── Infrastructure/    # EF Core DbContext + repository + RabbitMQ event publisher
├── DTOs/              # Request/response contracts, decoupled from domain entities
├── Program.cs         # Composition root / DI wiring
└── appsettings.json

tests/TaskFlow.Api.Tests/
├── TaskRepositoryTests.cs     # Repository behavior against EF Core InMemory
└── TasksControllerTests.cs    # Controller behavior + event publishing, against fakes
```

## Tech stack

- C# / .NET 8
- ASP.NET Core Web API
- Entity Framework Core — **SQLite** for local dev, **PostgreSQL** in Docker (swappable via config)
- RabbitMQ — publishes domain events (`task.created`, `task.completed`) for downstream consumers
- Swagger / OpenAPI for interactive docs
- xUnit + EF Core InMemory for unit tests
- Docker + Docker Compose
- GitHub Actions CI (build + test on every push/PR)

## Design notes

- **Domain isolation**: `Domain/` has zero dependencies on EF Core or ASP.NET — entities and the `ITaskRepository` contract are plain C#.
- **DTOs at the boundary**: controllers never expose domain entities directly; `CreateTaskDto` / `TaskDto` shape what crosses the wire.
- **Swap-friendly persistence**: the EF Core provider is chosen at startup based on `Database:Provider` in configuration (`Sqlite` locally, `Postgres` in Docker) — controllers and domain logic never change.
- **Messaging as a side-channel, not a dependency**: `RabbitMqEventPublisher` connects lazily and swallows connection/publish failures (logging a warning instead). If RabbitMQ is down, task CRUD still works — the API's core responsibility never depends on the broker being up.
- **Tested at two levels**: repository tests run against a real `DbContext` (EF Core InMemory) to catch mapping/behavior issues; controller tests use fakes for both the repository and the event publisher to isolate HTTP-layer logic (validation, status codes, and which events get published when).

## Event-driven integration

On task creation and completion, the API publishes a message to a `taskflow.events` topic exchange in RabbitMQ:

| Routing key       | Event               | Published when              |
|--------------------|---------------------|------------------------------|
| `task.created`     | `TaskCreatedEvent`   | A task is successfully created |
| `task.completed`   | `TaskCompletedEvent` | A task's status is set to `Done` |

[taskflow-notifier](https://github.com/AlexandreMelloAssis/taskflow-notifier) is a separate worker service that subscribes to these events — demonstrating that other services can react to what happens in TaskFlow without the API knowing or caring who's listening.

## Running locally (SQLite, no Docker, no RabbitMQ)

```bash
dotnet restore
dotnet run --project TaskFlow.Api.csproj
```

Swagger UI will be available at `https://localhost:5001/swagger`. Task CRUD works fully without RabbitMQ running; you'll just see warnings logged when events can't be published.

## Running with Docker Compose (API + PostgreSQL + RabbitMQ)

```bash
docker compose up --build
```

This starts the API on `http://localhost:8080`, PostgreSQL (`db`), and RabbitMQ (`rabbitmq`, management UI at `http://localhost:15672`, guest/guest).

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

## Related projects

- [taskflow-web](https://github.com/AlexandreMelloAssis/taskflow-web) — Angular 17 front end that consumes this API.
- [taskflow-notifier](https://github.com/AlexandreMelloAssis/taskflow-notifier) — .NET worker service that consumes this API's RabbitMQ events.

## Possible next steps

- Add authentication (JWT) and per-user task ownership
- Add pagination/filtering to the list endpoint
- Replace `EnsureCreated()` with proper EF Core migrations for production use
- Add an end-to-end integration test using `WebApplicationFactory`

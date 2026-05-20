# PayFlow

Payments and billing API built with .NET 8, featuring microservices architecture, CQRS, rich domain model and CNAB file processing.

> 🚧 Work in progress

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Docker Compose                         │
│                                                             │
│  ┌──────────────────┐           ┌──────────────────┐        │
│  │  PayFlow.API     │  publish  │  PayFlow.Worker   │        │
│  │  (.NET 8)        │ ───────►  │  (.NET 8)         │        │
│  │                  │   Kafka   │                   │        │
│  │  • Controllers   │           │  • Kafka Consumer │        │
│  │  • CQRS/MediatR  │           │  • CNAB Parser    │        │
│  │  • Rich Domain   │           │  • Webhooks       │        │
│  │  • Repository    │           │                   │        │
│  └───────┬──────────┘           └───────────────────┘        │
│          │                                                   │
│     ┌────┴────┐                                              │
│     ▼         ▼                                              │
│  ┌──────┐ ┌───────┐                                         │
│  │Postgres│ │ Redis │                                        │
│  └──────┘ └───────┘                                         │
└─────────────────────────────────────────────────────────────┘
```

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# / .NET 8 |
| Database | PostgreSQL |
| Cache | Redis |
| Messaging | Apache Kafka |
| ORM | Entity Framework Core |
| CQRS | MediatR |
| Testing | xUnit, Moq |
| Containers | Docker / Docker Compose |
| Version Control | Git |

## Project Structure

```
PayFlow/
├── src/
│   ├── PayFlow.API/              # Microservice 1 — REST API
│   │   ├── Controllers/          # REST endpoints
│   │   ├── Application/
│   │   │   ├── Commands/         # Write operations (CQRS)
│   │   │   └── Queries/          # Read operations (CQRS)
│   │   └── Middleware/           # Global exception handling
│   │
│   ├── PayFlow.Worker/           # Microservice 2 — Background worker
│   │   ├── Consumers/            # Kafka consumers
│   │   ├── CnabParser/           # CNAB return file processing
│   │   └── Webhooks/             # Client notifications
│   │
│   ├── PayFlow.Domain/           # Rich domain (entities, value objects, rules)
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   └── Exceptions/
│   │
│   └── PayFlow.Infrastructure/   # Data access, cache, messaging
│       ├── Persistence/          # DbContext, Repositories, Migrations
│       ├── Cache/                # Redis
│       └── Messaging/            # Kafka producer/consumer
│
├── tests/
│   ├── PayFlow.Domain.Tests/     # Domain unit tests
│   └── PayFlow.API.Tests/        # API integration tests
│
├── docker-compose.yml
└── README.md
```

## Domain

This project follows a **rich domain model** approach — entities encapsulate business rules, validations and state transitions internally, with private setters.

```csharp
public class Cobranca
{
    public Guid Id { get; private set; }
    public decimal Valor { get; private set; }
    public StatusCobranca Status { get; private set; }

    public Cobranca(decimal valor)
    {
        if (valor <= 0) throw new DomainException("Amount must be greater than zero");
        Id = Guid.NewGuid();
        Valor = valor;
        Status = StatusCobranca.Pendente;
    }

    public void Confirmar()
    {
        if (Status != StatusCobranca.Pendente)
            throw new DomainException("Only pending charges can be confirmed");
        Status = StatusCobranca.Confirmada;
    }

    public void Cancelar()
    {
        if (Status == StatusCobranca.Paga)
            throw new DomainException("Cannot cancel a charge that has already been paid");
        Status = StatusCobranca.Cancelada;
    }
}
```

## Main Endpoints

| Method | Route | Description | Status |
|---|---|---|---|
| `POST` | `/api/cobrancas` | Create new charge | `201 Created` |
| `GET` | `/api/cobrancas/{id}` | Get charge by ID | `200 OK` |
| `GET` | `/api/cobrancas` | List charges with filters | `200 OK` |
| `PATCH` | `/api/cobrancas/{id}/confirmar` | Confirm charge | `204 No Content` |
| `PATCH` | `/api/cobrancas/{id}/cancelar` | Cancel charge | `204 No Content` |
| `POST` | `/api/cnab/upload` | Upload CNAB return file | `202 Accepted` |

## Application Flow

1. Client creates a charge via `POST /api/cobrancas`
2. The API validates through the domain, persists to PostgreSQL and publishes a `CobrancaCriada` event to Kafka
3. The Worker consumes the event and runs async processing
4. Frequent status queries use Redis cache (cache-aside pattern, 5 min TTL)
5. CNAB return files are uploaded, processed by the Worker and update charge statuses accordingly

## Getting Started

```bash
git clone https://github.com/PedroRCampelo/PayFlow.git
cd PayFlow

docker-compose up -d

# API available at http://localhost:5000
# Swagger at http://localhost:5000/swagger
```

## Patterns & Practices

- **CQRS** — command/query separation via MediatR
- **Repository Pattern** — data access abstraction over EF Core
- **Rich Domain Model** — entities with behavior, internal validations and private setters
- **SOLID** — principles applied throughout the codebase
- **Global Exception Handling** — centralized middleware returning ProblemDetails (RFC 7807)
- **Automated Testing** — domain unit tests (xUnit + Moq) and API integration tests (WebApplicationFactory)

## Author

**Pedro Campelo** — [GitHub](https://github.com/PedroRCampelo) · [LinkedIn](https://linkedin.com/in/pedro-campêlo)
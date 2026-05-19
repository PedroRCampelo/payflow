# PayFlow

API de pagamentos e cobranças em .NET 8 com microserviços, CQRS, domínio rico e processamento de arquivos CNAB.

> 🚧 Projeto em desenvolvimento

---

## Arquitetura

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
│  │  • Domain Rico   │           │  • Webhooks       │        │
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

| Camada | Tecnologia |
|---|---|
| Linguagem | C# / .NET 8 |
| Banco de dados | PostgreSQL |
| Cache | Redis |
| Mensageria | Apache Kafka |
| ORM | Entity Framework Core |
| CQRS | MediatR |
| Testes | xUnit, Moq |
| Containerização | Docker / Docker Compose |
| Versionamento | Git |

## Estrutura do Projeto

```
PayFlow/
├── src/
│   ├── PayFlow.API/              # Microserviço 1 — API REST
│   │   ├── Controllers/          # Endpoints REST
│   │   ├── Application/
│   │   │   ├── Commands/         # Operações de escrita (CQRS)
│   │   │   └── Queries/          # Operações de leitura (CQRS)
│   │   └── Middleware/           # Exception handling global
│   │
│   ├── PayFlow.Worker/           # Microserviço 2 — Background worker
│   │   ├── Consumers/            # Kafka consumers
│   │   ├── CnabParser/           # Processamento de arquivos CNAB
│   │   └── Webhooks/             # Notificações para clientes
│   │
│   ├── PayFlow.Domain/           # Domínio rico (entidades, value objects, regras)
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   └── Exceptions/
│   │
│   └── PayFlow.Infrastructure/   # Acesso a dados, cache, mensageria
│       ├── Persistence/          # DbContext, Repositories, Migrations
│       ├── Cache/                # Redis
│       └── Messaging/            # Kafka producer/consumer
│
├── tests/
│   ├── PayFlow.Domain.Tests/     # Testes unitários do domínio
│   └── PayFlow.API.Tests/        # Testes de integração da API
│
├── docker-compose.yml
└── README.md
```

## Domínio

O projeto utiliza **domínio rico** — as entidades encapsulam regras de negócio, validações e transições de estado internamente, com setters privados.

```csharp
// Exemplo simplificado
public class Cobranca
{
    public Guid Id { get; private set; }
    public decimal Valor { get; private set; }
    public StatusCobranca Status { get; private set; }

    public Cobranca(decimal valor)
    {
        if (valor <= 0) throw new DomainException("Valor deve ser positivo");
        Id = Guid.NewGuid();
        Valor = valor;
        Status = StatusCobranca.Pendente;
    }

    public void Confirmar()
    {
        if (Status != StatusCobranca.Pendente)
            throw new DomainException("Só é possível confirmar cobranças pendentes");
        Status = StatusCobranca.Confirmada;
    }

    public void Cancelar()
    {
        if (Status == StatusCobranca.Paga)
            throw new DomainException("Não é possível cancelar cobrança já paga");
        Status = StatusCobranca.Cancelada;
    }
}
```

## Endpoints Principais

| Método | Rota | Descrição | Status |
|---|---|---|---|
| `POST` | `/api/cobrancas` | Criar nova cobrança | `201 Created` |
| `GET` | `/api/cobrancas/{id}` | Consultar cobrança por ID | `200 OK` |
| `GET` | `/api/cobrancas` | Listar cobranças com filtros | `200 OK` |
| `PATCH` | `/api/cobrancas/{id}/confirmar` | Confirmar cobrança | `204 No Content` |
| `PATCH` | `/api/cobrancas/{id}/cancelar` | Cancelar cobrança | `204 No Content` |
| `POST` | `/api/cnab/upload` | Upload de arquivo CNAB retorno | `202 Accepted` |

## Fluxo da Aplicação

1. Cliente cria uma cobrança via `POST /api/cobrancas`
2. A API valida pelo domínio, persiste no PostgreSQL e publica evento `CobrancaCriada` no Kafka
3. O Worker consome o evento e executa processamentos assíncronos
4. Consultas frequentes de status usam cache Redis (pattern cache-aside, TTL 5 min)
5. Arquivos CNAB de retorno são enviados via upload, processados pelo Worker e atualizam o status das cobranças

## Como Rodar

```bash
# Clonar o repositório
git clone https://github.com/PedroRCampelo/PayFlow.git
cd PayFlow

# Subir toda a infraestrutura
docker-compose up -d

# A API estará disponível em http://localhost:5000
# Swagger em http://localhost:5000/swagger
```

## Padrões e Práticas

- **CQRS** — separação de comandos (escrita) e queries (leitura) via MediatR
- **Repository Pattern** — abstração do acesso a dados sobre EF Core
- **Domínio Rico** — entidades com comportamento, validações internas e setters privados
- **SOLID** — princípios aplicados em toda a codebase
- **Exception Handling Global** — middleware centralizado retornando ProblemDetails (RFC 7807)
- **Testes Automatizados** — unitários no domínio (xUnit + Moq) e integração na API (WebApplicationFactory)

## Autor

**Pedro Campelo** — [GitHub](https://github.com/PedroRCampelo) · [LinkedIn](https://linkedin.com/in/pedro-campêlo)

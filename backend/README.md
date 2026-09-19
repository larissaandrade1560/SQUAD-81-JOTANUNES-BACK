# Backend — JotaNunesForms

API ASP.NET Core 8 em arquitetura hexagonal (ports and adapters), com PostgreSQL e EF Core.

## Projetos

| Projeto | Camada | Responsabilidade |
|---------|--------|------------------|
| `src/JotaNunesForms.Domain` | Domínio | Entidades, invariantes e portas (interfaces) |
| `src/JotaNunesForms.Application` | Aplicação | Casos de uso e DTOs |
| `src/JotaNunesForms.Infrastructure` | Infraestrutura | EF Core, PostgreSQL, repositórios e migrations |
| `src/JotaNunesForms.Api` | API | Controllers HTTP, CORS, Swagger e composição da aplicação |
| `tests/*` | Testes | Regras de domínio, casos de uso e contrato HTTP |

## Grafo de dependências

```text
Api ──────────► Application ──► Domain
 │                    ▲
 └──► Infrastructure ─┘
```

**Proibido:**

- Domain ou Application referenciarem EF Core, Npgsql ou ASP.NET
- Controllers acessarem `DbContext` ou o banco diretamente
- Regras de negócio em controllers ou repositórios

## Onde criar cada coisa

| Preciso criar… | Colocar em… |
|----------------|-------------|
| Entidade / invariante | `Domain/Entities` |
| Contrato de persistência | `Domain/Ports` |
| Caso de uso | `Application/UseCases/{Contexto}` |
| DTO de entrada/saída | `Application/DTOs` |
| Mapeamento EF / migration | `Infrastructure/Persistence` |
| Implementação de porta | `Infrastructure/Persistence/Repositories` |
| Endpoint HTTP | `Api/Controllers` |

## Banco local

O PostgreSQL sobe pelo Docker Compose. A API aplica as migrations automaticamente quando `Database__ApplyMigrations=true` (já configurado no `compose.yaml`).

```bash
cp .env.example .env
docker compose up -d postgres api
```

| Campo | Valor |
| --- | --- |
| Host | localhost |
| Porta | 5432 |
| Usuário | `POSTGRES_USER` do `.env` |
| Senha | `POSTGRES_PASSWORD` do `.env` |
| Banco | `POSTGRES_DB` do `.env` |

## Comandos EF Core

A partir de `backend/`:

```bash
dotnet tool restore
dotnet tool run dotnet-ef migrations add NomeDaMigracao \
  --project src/JotaNunesForms.Infrastructure \
  --startup-project src/JotaNunesForms.Api \
  --output-dir Persistence/Migrations

dotnet tool run dotnet-ef database update \
  --project src/JotaNunesForms.Infrastructure \
  --startup-project src/JotaNunesForms.Api
```

No Docker:

```bash
docker compose exec api dotnet tool restore
docker compose exec api dotnet tool run dotnet-ef migrations add NomeDaMigracao \
  --project src/JotaNunesForms.Infrastructure \
  --startup-project src/JotaNunesForms.Api \
  --output-dir Persistence/Migrations
```

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

O repositório fixa o SDK .NET 8 em `../global.json` e o Node 22 em `../.nvmrc`. O seed de usuários existe somente em `Development` e exige `AUTH_SEED_PASSWORD` com pelo menos 16 caracteres; não há senha demo padrão. O Swagger também fica disponível apenas em `Development`.

## Configuração segura de produção

Configure os valores no secret manager do provedor (Render: **Environment**); não os grave no repositório:

| Variável | Requisito |
| --- | --- |
| `Jwt__SigningKey` | Segredo aleatório com pelo menos 32 caracteres; não reutilizar a chave local conhecida. Rotacioná-la invalida todas as sessões existentes. |
| `Jwt__Issuer` / `Jwt__Audience` | Valores explícitos e estáveis para esta API e seus clientes. |
| `Cors__Origins` | Allowlist separada por vírgulas de origens HTTPS exatas; sem `*`, domínio curinga, localhost ou caminhos. |
| `BootstrapAdmin__Enabled` | `false` normalmente. |
| `BootstrapAdmin__Documento`, `BootstrapAdmin__NomeExibicao`, `BootstrapAdmin__Password` | Só configurar durante a criação inicial; senha aleatória de ao menos 16 caracteres. |

O bootstrap é create-only: recusa se já existir qualquer usuário e nunca redefine ou reativa contas. Configure-o temporariamente no secret manager, faça um único startup/deploy, confirme o evento `bootstrap_admin_succeeded` e desative `BootstrapAdmin__Enabled` removendo as credenciais. Não deixe o segredo de bootstrap ativo em deployments seguintes.

Rotação da chave JWT encerra sessões. Agende-a, remova a chave antiga do provedor, publique a nova e avise os usuários para fazer login novamente. A rotação de credenciais de banco, R2 e e-mail deve seguir o procedimento do respectivo provedor e nunca ser registrada em logs.

## Gate de acesso

`specs/004-harden-access-security/contracts/access-matrix.json` é a fonte canônica para operações HTTP. Ao adicionar ou alterar uma rota, atualize a matriz e mantenha `EndpointInventoryTests` passando. Antes de produção, execute `dotnet test JotaNunesForms.sln --configuration Release` em host com Docker disponível; os testes de isolamento e startup usam PostgreSQL efêmero via Testcontainers.

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

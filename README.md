# JotaNunesForms — Squad 81 (Residência de Software IV)

Sistema web para gerenciamento de formulários e gestão de terceirizados.

## Tecnologias

- Frontend: React, Vite e TypeScript
- Backend: ASP.NET Core (.NET 8) em arquitetura hexagonal
- Banco de dados: PostgreSQL 16
- ORM: Entity Framework Core (Npgsql)
- Containers: Docker Compose
- Banco visual: Beekeeper Studio (ou cliente PostgreSQL equivalente)

## Estrutura do projeto

```text
.
├── backend/       # API hexagonal .NET 8
│   ├── src/       # Domain, Application, Infrastructure, Api
│   └── tests/     # Testes por camada
├── frontend/      # Aplicação React
├── docs/          # Documentação (requisitos, specs, planos)
├── compose.yaml   # Serviços Docker
├── .env.example   # Modelo de configurações locais
└── README.md
```

Requisitos funcionais (RF01–RF20): [docs/requisitos/requisitos-funcionais.md](docs/requisitos/requisitos-funcionais.md).

### Organização do frontend

A estrutura interna de pastas (`src/api`, `src/services`, `src/pages`, etc.), convenções de nomes e regras de dependência estão documentadas em [frontend/README.md](frontend/README.md).

### Organização do backend

As camadas hexagonais (`Domain`, `Application`, `Infrastructure`, `Api`), o grafo de dependências e o PostgreSQL local estão documentados em [backend/README.md](backend/README.md).

## Pré-requisitos

- Docker Desktop
- Git
- Visual Studio Code (ou outro editor)
- Beekeeper Studio (opcional, para visualizar o banco)

## Configuração inicial

### 1. Clone o repositório

```bash
git clone URL_DO_REPOSITORIO
cd NOME_DO_REPOSITORIO
```

### 2. Crie suas variáveis locais

```bash
cp .env.example .env
```

Edite o arquivo `.env` se quiser alterar usuário, senha ou nome do banco PostgreSQL.

Importante: nunca envie o arquivo `.env` para o GitHub.

### 3. Instale as dependências do frontend

```bash
docker compose run --rm frontend npm install
```

### 4. Inicie os serviços

```bash
docker compose up -d
```

Para acompanhar os logs:

```bash
docker compose logs -f
```

As migrations do PostgreSQL são aplicadas automaticamente pela API em ambiente Docker (`Database__ApplyMigrations=true`).

## Endereços locais

| Serviço | Endereço |
| --- | --- |
| Frontend | http://localhost:5173 |
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| PostgreSQL | localhost:5432 |

## Beekeeper Studio

Crie uma conexão PostgreSQL com estas informações:

| Campo | Valor |
| --- | --- |
| Host | localhost |
| Porta | 5432 |
| Usuário | Valor de `POSTGRES_USER` no `.env` |
| Senha | Valor de `POSTGRES_PASSWORD` no `.env` |
| Banco | Valor de `POSTGRES_DB` no `.env` |

## Comandos úteis

```bash
# Iniciar os serviços e ver os logs
docker compose up

# Parar os serviços e remover os contêineres
docker compose down

# Ver os serviços em execução
docker compose ps

# Aplicar uma nova migração do Entity Framework
docker compose exec api dotnet tool restore
docker compose exec api dotnet tool run dotnet-ef migrations add NomeDaMigracao \
  --project src/JotaNunesForms.Infrastructure \
  --startup-project src/JotaNunesForms.Api \
  --output-dir Persistence/Migrations
```

## Fluxo de branches

- `main`: versão estável do projeto.
- `develop`: branch de integração do time.
- `feature/nome-da-tarefa`: branch de uma funcionalidade específica.

Para iniciar uma funcionalidade:

```bash
git switch develop
git pull origin develop
git switch -c feature/nome-da-tarefa
```

Ao concluir a tarefa, envie sua branch e abra um Pull Request para `develop`.

Quando uma versão estiver pronta para entrega, abra um Pull Request de `develop` para `main`.

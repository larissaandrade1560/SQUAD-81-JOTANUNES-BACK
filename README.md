# JotaNunesForms — Squad 81 (Residência de Software IV)

Sistema web para gerenciamento de formulários.

## Tecnologias

- Frontend: React, Vite e TypeScript
- Backend: ASP.NET Core (.NET 8)
- Banco de dados: SQL Server 2022
- ORM: Entity Framework Core
- Containers: Docker Compose
- Banco visual: Beekeeper Studio

## Estrutura do projeto

```text
.
├── backend/       # API .NET 8
├── frontend/      # Aplicação React
├── compose.yaml   # Serviços Docker
├── .env.example   # Modelo de configurações locais
└── README.md
```

### Organização do frontend

A estrutura interna de pastas (`src/api`, `src/services`, `src/pages`, etc.), convenções de nomes e regras de dependência estão documentadas em [frontend/README.md](frontend/README.md).
## Pré-requisitos

- Docker Desktop
- Git
– Visual Studio Code (ou outro editor)
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

Edite o arquivo `.env` e defina uma senha forte para o SQL Server:

```text
MSSQL_SA_PASSWORD=suaSenhaForteAqui
```

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

### 5. Aplique as migrações do banco

```bash
docker compose exec api dotnet tool restore
docker compose exec api dotnet tool run dotnet-ef database update
```

## Endereços locais

| Serviço | Endereço |
| --- | --- |
| Frontend | http://localhost:5173 |
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| SQL Server | localhost:1433 |

## Beekeeper Studio

Crie uma conexão SQL Server com estas informações:

| Campo | Valor |
| --- | --- |
| Host | localhost |
| Porta | 1433 |
| Usuário | sa |
| Senha | Valor definido em `.env` |
| Banco | JotaNunesFormsDb |
| Trust Server Certificate | Ativado |

## Comandos úteis

```bash
# Iniciar os serviços e ver os logs
docker compose up

# Parar os serviços e remover os contêineres
docker compose down

# Ver os serviços em execução
docker compose ps

# Aplicar uma nova migração do Entity Framework
docker compose exec api dotnet tool run dotnet-ef migrations add NomeDaMigracao
docker compose exec api dotnet tool run dotnet-ef database update
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

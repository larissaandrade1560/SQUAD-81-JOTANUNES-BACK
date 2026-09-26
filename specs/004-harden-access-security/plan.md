# Implementation Plan: Endurecimento de acesso e isolamento

**Feature ID**: `004-harden-access-security` | **Branch**: ainda não criada | **Date**: 2026-09-26 | **Spec**: [spec.md](./spec.md)

## Summary

Fechar a API por padrão, revalidar a identidade corrente em toda requisição protegida e substituir o escopo opcional/fail-open por um contexto explícito. Endpoints internos receberão policies declarativas; endpoints compartilhados aplicarão ownership antes de ler ou alterar recursos. Produção falhará no startup diante de configuração insegura. Uma matriz versionada e testes HTTP com duas empresas sustentarão a entrega.

## Technical Context

**Language/Version**: C# 12 / .NET 8; TypeScript 6 / React 19

**Primary Dependencies**: ASP.NET Core Authentication/Authorization, JwtBearer, EF Core 8, Npgsql, Swashbuckle, React Router, xUnit, `Microsoft.AspNetCore.Mvc.Testing`; `JsonSchema.Net` exclusivamente nos testes para validar Draft 2020-12

**Storage**: PostgreSQL 16; R2 inalterado; eventos de segurança em logs estruturados, sem nova tabela

**Testing**: xUnit, `WebApplicationFactory<Program>`, PostgreSQL efêmero com migrations reais; Vitest para sessão/navegação

**Target Platform**: API Linux/Render, SPA Cloudflare Pages, Docker Compose local

**Project Type**: Aplicação web com SPA e API hexagonal

**Performance Goals**: No máximo uma consulta indexada de identidade por requisição protegida; quantidade constante de consultas de autorização em listagens, independentemente do número de resultados

**Constraints**: Fail-closed; API como fonte de verdade; sem enumeração cross-tenant; nenhum segredo/token/conteúdo documental em logs; preservar fluxos legítimos

**Scale/Scope**: 19 controllers e 69 operações HTTP inventariadas, três perfis e dois tipos de empresa; sem migration prevista

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

A constituição é um template ainda não ratificado, logo não contém gates executáveis. Foram aplicados os limites documentados do projeto:

- **PASS — hexagonal**: HTTP em Api/Infrastructure; contexto neutro em Application; ownership nos casos de uso/repositórios.
- **PASS — controllers finos**: controllers selecionam policy/contexto, sem regra de negócio.
- **PASS — fail-closed**: fallback policy e allowlist pública explícita.
- **PASS — persistência**: nenhuma nova entidade persistida.
- **PASS — testes por risco**: matriz, tenant, startup, CORS, Swagger e logs são gates.
- **PASS — compatibilidade**: frontend mantém guards de UX; `401` descarta sessão.

### Post-design re-check

Todos os gates permanecem aprovados. `AccessScope` não depende de ASP.NET, a matriz cobre o inventário atual e os contratos separam `401`, `403` e `404`. Não há violação a justificar.

## Project Structure

### Documentation

```text
specs/004-harden-access-security/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── access-matrix.json       # fonte canônica por método+rota
│   ├── access-matrix.schema.json # vocabulário e estrutura independentes
│   ├── access-matrix.md
│   ├── http-security.md
│   └── production-security.md
└── tasks.md                 # /speckit-tasks
```

### Source Code

```text
backend/
├── src/
│   ├── JotaNunesForms.Application/Auth/       # AccessScope/erros
│   ├── JotaNunesForms.Application/UseCases/   # ownership
│   ├── JotaNunesForms.Infrastructure/Auth/     # JWT/bootstrap
│   └── JotaNunesForms.Api/
│       ├── Authorization/                      # identidade/policies/logging
│       ├── Controllers/                        # classificação declarativa
│       └── Program.cs                          # startup/CORS/Swagger
└── tests/
    ├── JotaNunesForms.Application.Tests/
    ├── JotaNunesForms.Infrastructure.Tests/
    └── JotaNunesForms.Api.Tests/
        ├── Infrastructure/
        └── Security/

frontend/src/
├── api/client.ts
├── config/jotanunesNav.ts
├── routes/
└── store/

render.yaml
.github/workflows/ci.yml
```

**Structure Decision**: Manter os projetos atuais. O contexto fica na Application, resolução da identidade na borda HTTP e consultas escopadas via ports/repositories. Não criar novo projeto nem filtro global de banco.

## Design Decisions

1. Fallback policy exige identidade autenticada e ativa; apenas `GET /` como disponibilidade mínima, login e validação/conclusão de convite são anônimos.
2. Novos tokens incluem `usuario_id`; cada requisição revalida usuário, perfil, empresa e tipo atual.
3. `AccessScope.Internal`/`Company` substitui `Guid?` e elimina `null = global`.
4. Policies: `Administrador`, `Interno`, `TerceirizadoAtivo`, `TerceirizadoMaoDeObra`; ownership permanece na Application.
5. `401` para identidade inválida; `403` para perfil/módulo; `404` genérico para recurso alheio/inexistente.
6. JWT tipado/fail-fast, segredo mínimo 32 bytes, CORS exato, Swagger somente Development.
7. Demo somente Development; bootstrap produtivo explícito, create-only, sem fallback, com senha de pelo menos 16 caracteres e evento sanitizado para toda tentativa.
8. `access-matrix.json` é a fonte canônica por método+rota e `access-matrix.schema.json` define seu vocabulário independente; testes e documentação os consomem ou validam, sem segunda matriz manual.
9. Logs sanitizados com `EventId`, reason code e `TraceId`; a Application publica eventos por uma porta neutra implementada na borda.
10. Testes HTTP usam PostgreSQL real, nunca provider em memória como suíte principal.

## Delivery Sequence

1. Configuração segura e identidade ativa.
2. Policies globais e contrato uniforme de negação.
3. Escopo tipado e correção de IDORs.
4. Bloqueio integral de Materiais em módulos MO.
5. Bootstrap, Swagger, CORS, Render e logging.
6. Matriz automatizada e regressões legítimas.
7. Frontend: `401`, menus por tipo e novo login após deploy.

## Complexity Tracking

Nenhuma violação constitucional ou desvio estrutural identificado.

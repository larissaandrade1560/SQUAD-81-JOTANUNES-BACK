# Research: Endurecimento de acesso e isolamento

## Decisões

### Política global

**Decision**: Fallback policy exige identidade ativa; somente a allowlist é anônima.

**Rationale**: `GET /api/formularios` está público por omissão; fail-closed protege endpoints futuros.

**Alternatives considered**: `[Authorize]` manual foi rejeitado por ser fail-open.

### Identidade corrente

**Decision**: Adicionar `usuario_id` ao token e revalidar usuário, perfil, empresa e tipo em toda requisição protegida.

**Rationale**: Sessões atuais conservam privilégios por até oito horas após mudanças cadastrais.

**Alternatives considered**: Claims até expirar não cumprem a spec; security stamp/cache adiciona estado sem eliminar a consulta da empresa.

### Escopo e ownership

**Decision**: `AccessScope.Internal`/`Company`; recursos por ID são resolvidos com ownership antes de mapear, assinar download ou alterar.

**Rationale**: `Guid?` usa `null` como global e falha aberto; processos por ID possuem IDOR e recálculo com efeito colateral.

**Alternatives considered**: Filtro global de tenant foi rejeitado porque internos precisam visão global e bypasses seriam difíceis de auditar.

### Materiais

**Decision**: Policy `TerceirizadoMaoDeObra` mais defesa na Application bloqueiam todo o módulo, inclusive dados legados.

**Rationale**: O bloqueio atual é parcial.

**Alternatives considered**: Esconder menus não impede chamadas diretas.

### Negação

**Decision**: `401` para identidade inválida/inativa, `403` para capacidade, `404` indistinguível para ID alheio/inexistente.

**Rationale**: Evita enumeração sem esconder erro de perfil do usuário legítimo.

**Alternatives considered**: `403` para cross-tenant confirma existência.

### JWT produtivo

**Decision**: Configuração tipada única e validada no startup; issuer/audience/expiração/segredo obrigatórios; segredo ≥32 bytes e fora da denylist.

**Rationale**: Emissão e validação usam defaults separados; Render não declara JWT e cai na chave versionada.

**Alternatives considered**: Validação imperativa duplica regras; OIDC está fora de escopo.

### Seed/bootstrap

**Decision**: Demo somente Development. Bootstrap produtivo desligado por padrão, completo, create-only, com senha de pelo menos 16 caracteres e fora da denylist; nunca reativa/reseta e emite resultado sanitizado para toda tentativa.

**Rationale**: O seed atual usa `senha123`, documentos conhecidos e engole erros.

**Alternatives considered**: SQL manual contorna regras; endpoint público temporário aumenta ataque.

### Swagger/CORS

**Decision**: Swagger só Development; produção usa origens exatas, inicialmente `https://jotanunes-forms.pages.dev`.

**Rationale**: Hoje Swagger é público e qualquer `*.pages.dev` é aceito.

**Alternatives considered**: Swagger autenticado é complexidade sem necessidade; wildcard do provedor é amplo.

### Logging

**Decision**: Handler central para challenge/forbid e evento no tenant-404; registrar reason, route pattern, ator técnico e `TraceId`, nunca credenciais/bodies/PII.

**Rationale**: Middleware genérico não conhece requirements e não cobre bem ownership.

**Alternatives considered**: Logging pós-resposta perde contexto.

### Testes

**Decision**: `WebApplicationFactory`, PostgreSQL efêmero, migrations reais e fakes para R2/e-mail; matriz de inventário, perfis, tenant, startup, CORS/Swagger/logs e happy paths.

**Rationale**: API.Tests cobre somente `/`; EF InMemory não prova Npgsql/filtros.

**Alternatives considered**: Banco CI compartilhado tem pior isolamento; SQLite/InMemory divergem de produção.

## Fontes primárias

- https://learn.microsoft.com/dotnet/core/extensions/options
- https://learn.microsoft.com/aspnet/core/security/authentication/configure-jwt-bearer-authentication
- https://learn.microsoft.com/aspnet/core/security/app-secrets
- https://learn.microsoft.com/aspnet/core/security/cors
- https://learn.microsoft.com/aspnet/core/security/authorization/customizingauthorizationmiddlewareresponse
- https://learn.microsoft.com/aspnet/core/tutorials/getting-started-with-swashbuckle
- https://learn.microsoft.com/aspnet/core/fundamentals/logging

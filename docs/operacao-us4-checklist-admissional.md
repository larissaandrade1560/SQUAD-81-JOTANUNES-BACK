# Operação — US4 checklist admissional

## Deploy

1. Faça backup do PostgreSQL antes de aplicar `20260927140000_AddEpiEIntegracao`.
2. Publique a API primeiro e confirme health/migrations.
3. Publique o frontend em seguida.

## Rollback

- Após escrita real em `movimentos_epi` ou `integracoes_obra`, **não** execute `Down` da migration em produção.
- Prefira rollback de código compatível com as tabelas aditivas ou forward fix.
- Mobilizações existentes permanecem `Aguardando` até evidências válidas.

## Validação mínima pós-deploy

- `GET /api/mobilizacoes/{id}/liberacao` retorna impedimentos estáveis sem dados sensíveis.
- MO consegue registrar EPI; interno registra integração.
- Tenant estrangeiro recebe `404` genérico nas rotas US4.

# RF17 — Histórico de auditoria documental

**Situação:** implementação no repositório; validação automatizada, staging e produção ainda pendentes. A fonte funcional é a [spec](../specs/005-auditoria-documental/spec.md), com arquitetura em [plan](../specs/005-auditoria-documental/plan.md), tarefas em [tasks](../specs/005-auditoria-documental/tasks.md) e procedimento em [quickstart](../specs/005-auditoria-documental/quickstart.md).

## Escopo implementado

- Captura de envio, reenvio, aprovação, rejeição e vencimento para documentos empresariais, documentos de funcionários e versões de checklist.
- Evento e alteração relacional dentro de transação PostgreSQL; chave de negócio única para repetição idempotente e conflito `409 documento_estado_conflitante` para transição incompatível.
- Arquivo versionado para documentos legados de empresa/funcionário; a migration cria uma versão-base vigente por documento existente, sem inventar eventos retroativos. `DocumentoVersao` mantém os históricos existentes e recebe índices únicos de número e versão vigente.
- Tabelas `auditoria_eventos_documentais` e `documentos_arquivos_versoes`, snapshots allowlisted, índices de consulta e trigger PostgreSQL que recusa `UPDATE`/`DELETE` em eventos.
- `GET /api/auditoria/eventos`, protegido por policy `Interno`, com período RFC 3339 (`de` inclusivo e `ate` exclusivo), ação, empresa, escopo, paginação e ordenação estável.
- Tela interna `/auditoria`, com filtros, paginação, estados de carregamento/erro/vazio e navegação para a origem disponível.
- Falha de persistência após upload tenta remover o objeto do R2; falha de compensação é registrada somente por tipo de exceção, sem chave ou conteúdo.

## Privacidade e autorização

A resposta contém apenas ação, instante, retratos mínimos de ator/empresa/funcionário, rótulo documental, referências opacas de origem e motivo/comentário/validade permitidos pelo contrato. Não grava nem retorna CPF/CNPJ, e-mail do ator, senha, token, segredo, nome bruto do arquivo, storage key, hash, conteúdo do arquivo ou corpo bruto da requisição. A captura de ator exige identidade ativa revalidada pelo backend; vencimento automático usa o ator `Sistema`.

Somente Administrador e Analista podem consultar. A matriz canônica está em `specs/004-harden-access-security/contracts/access-matrix.json`; o guard do frontend é apenas UX, e a autorização efetiva permanece na API.

## Persistência e implantação

1. Aplicar primeiro `20260926230000_AddAuditoriaDocumental` e depois `20260926231000_AddAuditoriaDocumentalAppendOnlyTrigger`.
2. Antes da migration-base, confirmar que `documentos_versoes` não possui números repetidos por item nem mais de uma versão vigente; a criação dos índices únicos falha de propósito se houver inconsistência.
3. Fazer backup validado. Não executar `Down` em produção: isso removeria a trilha. Depois de ativar captura, não encaminhar escritas documentais a uma versão anterior à RF17; em falha, suspender mutações ou aplicar forward-fix.
4. Validar banco vazio e upgrade com dados legados em staging. Não usar bucket nem credenciais de produção em testes.
5. Só executar smoke de produção depois de CI verde, migrations aplicadas, backup e autorização operacional. O smoke de produção é somente leitura.

## Evidências disponíveis neste ambiente

- `frontend/node_modules/.bin/tsc -b`: passou.
- `npm run lint` no frontend: passou; há avisos `react(set-state-in-effect)` em páginas preexistentes.
- `npm test` e `npm run build`: não concluídos; o runtime disponível é Node 18.20.8 e o projeto exige Node 22. Vitest/Vite param ao importar `node:util.styleText`.
- Testes/build/migrations backend: não executados; o .NET SDK não está instalado.
- Testes PostgreSQL: não executados; Docker não está disponível nesta distro WSL.
- Staging e produção: não acessados nem alterados nesta implementação.

Portanto, não há evidência de suíte completa verde, aplicação real de migration, aceite em staging ou deploy. Esses gates continuam pendentes conforme o checklist de `tasks.md`.

# Quickstart: validar RF17 — Histórico de Auditoria Documental

## Pré-requisitos

- .NET SDK 8.
- Node.js 22 e dependências do frontend instaladas.
- Docker ativo para PostgreSQL 16 efêmero dos testes.
- Ambiente local/staging com storage de teste, nunca bucket ou credenciais de produção.
- Contas de teste: Administrador, Analista, terceirizado Mão de Obra e terceirizado Materiais.

Contrato: [contracts/auditoria.openapi.yaml](./contracts/auditoria.openapi.yaml). Modelo: [data-model.md](./data-model.md).

## 1. Gates automatizados

Na raiz do repositório:

```bash
dotnet restore backend/JotaNunesForms.sln
dotnet build backend/JotaNunesForms.sln --configuration Release --no-restore
dotnet test backend/JotaNunesForms.sln --configuration Release --no-build
npm --prefix frontend ci
npm --prefix frontend run lint
npm --prefix frontend test
npm --prefix frontend run build
```

Sem Docker, testes PostgreSQL devem falhar com pré-requisito claro; não substituir por provider em memória.

## 2. Migração

A partir de `backend/`, gere/aplique a migração da implementação:

```bash
dotnet tool restore
dotnet tool run dotnet-ef migrations add AddAuditoriaDocumental \
  --project src/JotaNunesForms.Infrastructure \
  --startup-project src/JotaNunesForms.Api \
  --output-dir Persistence/Migrations
dotnet tool run dotnet-ef migrations add AddAuditoriaDocumentalAppendOnlyTrigger \
  --project src/JotaNunesForms.Infrastructure \
  --startup-project src/JotaNunesForms.Api \
  --output-dir Persistence/Migrations
dotnet tool run dotnet-ef database update \
  --project src/JotaNunesForms.Infrastructure \
  --startup-project src/JotaNunesForms.Api
```

A segunda migration recebe a função e o trigger append-only em `Up` e sua remoção segura em `Down`; não reescreva a primeira migration depois de aplicada. Validar as duas, na ordem, em banco vazio e em cópia atualizada da produção:

Antes de aplicar a migration-base em banco legado, confirme que os índices únicos não encontrarão duplicidades:

```sql
SELECT item_checklist_id, numero, COUNT(*)
FROM documentos_versoes
GROUP BY item_checklist_id, numero
HAVING COUNT(*) > 1;

SELECT item_checklist_id, COUNT(*)
FROM documentos_versoes
WHERE vigente = TRUE
GROUP BY item_checklist_id
HAVING COUNT(*) > 1;
```

As duas consultas devem retornar zero linhas. Investigue e corrija inconsistências com backup e revisão antes da migração; não escolha silenciosamente uma versão vigente.

Validar também:

- tabela e índices de auditoria criados vazios;
- nenhum evento retroativo fabricado;
- cada documento empresarial/de funcionário existente recebe exatamente uma versão-base vigente;
- documentos de checklist e análises existentes permanecem inalterados;
- históricos preexistentes de `DocumentoVersao` e `AnaliseDocumento` continuam consultáveis pelos fluxos atuais;
- `UPDATE` ou `DELETE` direto em evento é recusado pelo trigger.

Antes do deploy, fazer backup. Não executar `Down` em produção: remover a tabela apagaria a trilha. Depois que a captura for ativada, não devolver tráfego de escrita a uma versão pré-RF17; em falha, preferir forward-fix ou suspender temporariamente mutações documentais.

## 3. Sequência funcional em staging

Com uma empresa e documentos descartáveis:

1. Enviar um documento novo e confirmar `documento_enviado`, versão 1 e ator correto.
2. Rejeitar com motivo de teste e confirmar `documento_rejeitado` sem conteúdo sensível.
3. Reenviar e confirmar versão 2, vínculo à versão 1 e `documento_reenviado`.
4. Aprovar com comentário/validade e confirmar `documento_aprovado`.
5. Avançar o relógio controlado ou usar dado já expirado no ambiente de teste; confirmar uma única transição/evento `documento_vencido` com ator Sistema.
6. Repetir a verificação de vencimento e a mesma decisão; confirmar ausência de novo evento ou análise.

Repetir o fluxo para documento empresarial, documento de funcionário e requisito versionado.

## 4. Atomicidade e concorrência

Os testes de integração devem injetar falha ao gravar o evento e comprovar:

- estado documental anterior preservado;
- nenhuma análise/versão parcial visível;
- nenhum evento parcial;
- upload recém-criado removido pelo storage fake/compensação.

Executar duas requisições concorrentes para reenvio, decisão e vencimento da mesma versão. O resultado deve conter uma única transição confirmada e um único evento; a requisição perdedora não pode deixar escrita parcial.

Para a mesma transição já confirmada, a repetição deve retornar sucesso idempotente e reutilizar o resultado existente. Vencimento já processado deve ser no-op bem-sucedido. Uma transição incompatível deve retornar `409` com `documento_estado_conflitante`, sem versão, análise ou evento adicional.

O R2 real não participa da transação do banco. Se a exclusão compensatória falhar, o objeto pode ficar órfão, mas não pode possuir referência visível; o erro deve ser sanitizado e encaminhado para limpeza operacional.

## 5. Consulta e autorização

Validar `GET /api/auditoria/eventos` conforme o contrato:

- sem autenticação → `401`, sem itens/total;
- terceirizado MO ou Materiais → `403`, sem itens/total;
- Admin e Analista → `200`;
- consulta válida sem resultados → `200` com `items: []` e `total: 0`;
- `de >= ate`, enum inválido, `page < 1` ou `pageSize > 100` → `400`;
- filtros isolados e combinados usam AND;
- eventos ordenados por `ocorridoEm DESC, id DESC`;
- navegação sobre conjunto estável não duplica nem omite itens;
- origem removida/indisponível mantém snapshot e retorna `disponivel: false`.

Validar motivo e comentário com 2.000 caracteres e com 2.001 caracteres: o limite deve ser aceito; o excesso deve ser rejeitado sem truncamento nem escrita parcial.

Confirmar que `EndpointInventoryTests` reconhece a nova operação como `Internal` na matriz canônica.

## 6. Privacidade e logs

Inspecionar respostas e logs gerados pelos cenários anteriores. Não pode aparecer:

- senha, bearer token, segredo ou body bruto;
- CPF/CNPJ completo ou e-mail do ator;
- storage key, hash ou conteúdo do arquivo;
- nome bruto de arquivo quando puder carregar dado pessoal.

Os logs técnicos podem usar IDs opacos, código do evento e `TraceId`, sem replicar motivo/comentário completos.

## 7. Frontend

Em `/auditoria`, validar:

- menu e rota disponíveis somente para Admin/Analista;
- estados loading, erro com nova tentativa, vazio e resultado;
- filtros de período, ação, empresa e escopo;
- aplicar/limpar filtros retorna à página 1;
- paginação preserva filtros e informa página/total;
- colunas Data, Ação, Origem, Detalhes e Usuário;
- link correto por origem e texto “Origem indisponível” quando aplicável;
- ausência de Exportar CSV.

Fixar o fuso do navegador nos testes: os instantes devem ser exibidos nesse fuso, os filtros devem ser enviados como RFC 3339/UTC e, quando o fuso não estiver disponível, a exibição deve usar UTC.

Em staging, cronometrar sem assistência a localização por período, ação, empresa e escopo para Administrador e Analista. Cada execução deve terminar em até 3 minutos e ter tempo/resultado registrados.

O guard do frontend é somente UX; a recusa do backend continua obrigatória.

## 8. Desempenho

Em PostgreSQL descartável, inserir 100.000 eventos sintéticos sem PII, aquecer a consulta e medir pelo menos dez execuções da primeira página. O p95 deve ser no máximo 3 segundos.

Verificar plano de execução dos filtros principais e ausência de N+1. A quantidade de consultas para uma página deve permanecer constante ao variar o número de itens até o limite da página. Não executar carga sintética em produção.

## 9. Smoke pós-deploy

Após CI verde, backup e migração aplicada:

1. Com Admin e Analista de teste autorizados, consultar a primeira página e um filtro conhecido; esperar `200`.
2. Sem sessão, esperar `401`; com conta terceirizada de teste, esperar `403`.
3. Abrir `/auditoria`, conferir estados e navegação sem criar ou alterar documentos reais.
4. Verificar logs sanitizados e ausência de erro de migração/trigger.

O smoke em produção é somente leitura. Envio, rejeição, aprovação e vencimento devem ser provados em testes automatizados e staging, não em documentos reais.

## Evidências desta execução

Em 26/09/2026, neste ambiente WSL:

- TypeScript (`./node_modules/.bin/tsc -b`): passou.
- Lint frontend (`npm run lint`): passou, com avisos existentes `react(set-state-in-effect)` em outras páginas.
- Vitest e Vite/build: bloqueados por Node 18.20.8; este projeto requer Node 22 e o runtime não exporta `node:util.styleText`.
- Testes/build/migrations backend: bloqueados porque `dotnet` não está instalado.
- Testes de integração PostgreSQL: bloqueados porque Docker não está disponível.
- Staging/produção: não executados.

Os itens de execução correspondentes em `tasks.md` permanecem pendentes; esta evidência não substitui CI em ambiente compatível.

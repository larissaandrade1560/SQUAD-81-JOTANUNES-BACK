# RF12 — Status documental (Em análise e Vencido)

## Objetivo

Completar o fluxo de status dos documentos empresariais e de funcionários:

- **Em análise** — documento entra na fila de validação
- **Vencido** — documento aprovado expirado
- **Nova versão** — terceirizado substitui documento vencido (mesmo fluxo do reenvio RF11)

## Backend

### Domínio

- `DocumentoEmpresa` / `DocumentoFuncionario`:
  - `ValidoAte` — data de validade após aprovação
  - `IniciarAnalise()` — `Pendente` → `EmAnalise`
  - `Aprovar()` — define `ValidoAte` (certidão negativa: 90 dias; demais: 365 dias)
  - `AtualizarVencimentoSeExpirado(utcNow)` — `Aprovado` expirado → `Vencido`
  - `Reenviar()` — aceita `Rejeitado` **ou** `Vencido`

### Migration

- `20260920153000_AddDocumentoValidoAte` — coluna `valido_ate` em `documentos_empresa` e `documentos_funcionario`

### Use cases

- `ListValidacaoFilaUseCase` — ao listar, `Pendente` → `EmAnalise` e persiste
- `ListDocumentosEmpresaUseCase` / `ListDocumentosFuncionarioUseCase` — aplica vencimento na listagem

### API

- DTOs incluem `validoAte`
- Fila de validação inclui `status` e `statusRotulo`

## Frontend

- Coluna **Status** na fila de validação
- Filtro por status nas páginas de documentos
- Coluna **Válido até** quando aplicável
- Botão **Nova versão** para documentos vencidos (terceirizado)

## Testes

- `DocumentoStatusTests.cs` — transições Em análise, validade, vencimento e reenvio de vencido

## Deploy

1. Push na branch `develop`
2. Deploy manual no Render (aplica migration `AddDocumentoValidoAte`)
3. Deploy frontend Cloudflare Pages

## Validação E2E sugerida

1. Terceirizado envia documento → status **Pendente**
2. Admin abre `/validacao` → documento aparece **Em análise**
3. Admin aprova → status **Aprovado** com **Válido até**
4. (Teste) Forçar `valido_ate` no passado no banco → listagem mostra **Vencido**
5. Terceirizado clica **Nova versão** → volta **Pendente** e entra na fila novamente

# RF18 — Consulta de pendências

## Escopo

- **RF18**: listar irregularidades para Admin/Analista (`UserClaims.IsEquipeInterna`).
- Substitui `ModulePlaceholderPage` em `/pendencias`.

## API

- `GET /api/pendencias` → `PendenciaItemResponse[]`
- Agregação em `ListPendenciasUseCase`:
  - Documentos empresa/funcionário **rejeitados** ou **vencidos** (atualiza vencimento antes de listar).
  - Comprovantes **pendentes**, **em atraso** ou **enviados em atraso** (`ObterSituacaoComprovante`).
- Ordenação: severidade desc, data referência desc.

## Frontend

- `PendenciasPage` + `pendenciasService.ts`
- Filtro simples por categoria; links para `/pagamentos`, `/documentos`, docs do funcionário.

## Deploy

Requer deploy **API** (endpoint novo) e **Pages** (rota real).

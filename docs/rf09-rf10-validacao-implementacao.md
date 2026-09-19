# RF09 / RF10 — Validação documental e motivo de rejeição

## Escopo

- **Analista / Admin:** fila de documentos **Pendentes** (empresa + funcionário).
- **RF09:** aprovar ou rejeitar cada arquivo.
- **RF10:** rejeição exige **motivo** (campo `motivo_rejeicao`, visível ao terceirizado).

## API

- `GET /api/validacao/fila`
- `POST /api/validacao/empresa/{id}/aprovar`
- `POST /api/validacao/empresa/{id}/rejeitar` — body `{ "motivo": "..." }`
- `POST /api/validacao/funcionario/{id}/aprovar`
- `POST /api/validacao/funcionario/{id}/rejeitar`

## Frontend

- `/validacao` — `ValidacaoPage` (fila + modal de rejeição).
- Coluna **Motivo rejeição** em `/documentos` e `/funcionarios/:id/documentos`.

## Deploy

Migration `20260919221500_AddDocumentoValidacaoCampos`. Push + manual deploy Render.

## Próximo

- **RF11** — reenvio de documentos rejeitados.

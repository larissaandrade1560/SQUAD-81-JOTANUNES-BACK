# RF08 — Upload documentos do funcionário (PDF / R2)

## Escopo

- **Terceirizado (MO):** envia PDF por tipo (ASO, identificação, etc.) para cada funcionário da empresa; status inicial **Pendente**.
- **Jotanunes (admin/analista):** consulta e **Baixar** (URL assinada R2, 15 min).
- **Storage:** mesmo bucket R2; chave `empresas/{empresaId}/funcionarios/{funcionarioId}/documentos/{id}.pdf`.

## API

- `GET /api/funcionarios/{funcionarioId}/documentos`
- `POST /api/funcionarios/{funcionarioId}/documentos` — multipart `arquivo` + `tipo` (terceirizado)
- `GET /api/documentos-funcionario/{id}/download`

## Frontend

- **Funcionários** → link **Documentos** por linha → `/funcionarios/:id/documentos` (`FuncionarioDocumentosPage`).

## Deploy

Migration `20260919220300_AddDocumentosFuncionario`. Push `develop` + **manual deploy Render**.

Log: `Applying migration '20260919220300_AddDocumentosFuncionario'`.

## Próximos passos

1. **RF09–RF10** — aprovar/rejeitar com motivo na fila de validação.
2. **RF11** — reenvio de documentos rejeitados.

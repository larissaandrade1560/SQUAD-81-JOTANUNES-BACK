# RF07 — Upload documentos empresariais (PDF / R2)

## Escopo

- **Terceirizado (MO):** envia PDF (até 10 MB) por tipo; status inicial **Pendente** (RF12).
- **Jotanunes (admin/analista):** lista todos + **Baixar** (URL assinada R2, 15 min).
- **Storage:** Cloudflare R2 (`jotanunes-docs`), chave `empresas/{empresaId}/documentos/{id}.pdf`.

## Render — variáveis

| Variável | Descrição |
|----------|-----------|
| `R2__AccountId` | Account ID Cloudflare |
| `R2__AccessKeyId` | Access Key ID (S3) |
| `R2__SecretAccessKey` | Secret (marcar Secret) |
| `R2__BucketName` | `jotanunes-docs` |
| `R2__ServiceUrl` | Opcional; default `https://{AccountId}.r2.cloudflarestorage.com` |

## API

- `GET /api/documentos-empresa` — lista (terceirizado: escopo empresa)
- `POST /api/documentos-empresa` — multipart `arquivo` + `tipo` (terceirizado)
- `GET /api/documentos-empresa/{id}/download` — `{ url, expiresAtUtc }`

## Frontend

- `/documentos` — `DocumentosEmpresaPage` (upload terceirizado; consulta interna).

## Deploy

Migration `20260919213000_AddDocumentosEmpresa`. Push + **manual deploy Render**.

Log: `Applying migration '20260919213000_AddDocumentosEmpresa'`.

## Próximos passos

1. **RF08** — documentos por funcionário (mesmo bucket/prefixo).
2. **RF09–RF10** — validação e rejeição com motivo.

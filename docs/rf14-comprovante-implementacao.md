# RF14 — Upload de comprovante bancário

## Backend

- Campos em `pagamentos_funcionario`: `comprovante_nome_arquivo`, `comprovante_storage_key`, `comprovante_content_type`, `comprovante_tamanho_bytes`
- `PagamentoFuncionario.RegistrarComprovante` — grava metadados + `comprovante_enviado_em` (UTC)
- Migration `20260920170000_AddPagamentoComprovanteArquivo` (+ **Designer**)
- `POST /api/pagamentos/{id}/comprovante` — terceirizado, PDF até 10 MB, R2
- `GET /api/pagamentos/{id}/comprovante/download` — URL presignada (15 min); terceirizado escopado à empresa; interno vê todos
- Situação na listagem atualiza para **No prazo** / **Enviado em atraso** (RF16 parcial)

## Frontend

- `/pagamentos`: colunas **Enviado em** e **Ações**
- Terceirizado: **Enviar comprovante** / **Substituir PDF**
- Todos com comprovante: **Visualizar** (download presignado)

## Deploy

Após push em `develop`, Render aplica migrations automaticamente (`Database__ApplyMigrations`).

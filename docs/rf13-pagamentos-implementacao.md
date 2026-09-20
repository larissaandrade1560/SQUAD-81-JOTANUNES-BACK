# RF13 — Registro de pagamentos (+ RF15 prazo)

## Objetivo

Terceirizado (MO) registra **data de pagamento** por funcionário e competência (mês). A equipe Jotanunes consulta todos os registros.

## Backend

- Entidade `PagamentoFuncionario` — `competencia`, `data_pagamento`, `prazo_comprovante` (+3 dias corridos, RF15)
- Migration `20260920163000_AddPagamentosFuncionario` (com **Designer**)
- `GET /api/pagamentos` — terceirizado: escopo empresa; interno: todos
- `POST /api/pagamentos` — terceirizado; unique `(funcionario_id, competencia)`
- Situação na listagem: Pendente / Em atraso (base RF16, sem comprovante ainda)

## Frontend

- `/pagamentos` — terceirizado e interno (menu atualizado)
- Formulário **Registrar pagamento** + tabela com prazo e situação

## Deploy

1. Push `develop`
2. Deploy manual Render (migration `AddPagamentosFuncionario`)
3. Cloudflare Pages (frontend)

## Próximo

- **RF14** — upload de comprovante vinculado ao pagamento (ver `docs/rf14-comprovante-implementacao.md`)
- **RF16** — alertas/dashboard com situação real

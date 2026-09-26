# RF13 — Registro de pagamentos (+ RF15 prazo)

## Objetivo

Terceirizado (MO) registra **data de pagamento** por funcionário e competência (mês). A equipe Jotanunes consulta todos os registros.

## Backend

- Entidade `PagamentoFuncionario` — `competencia`, `data_pagamento`, `prazo_comprovante` (+3 dias corridos, RF15)
- Migration `20260920163000_AddPagamentosFuncionario` (com **Designer**)
- `GET /api/pagamentos` — terceirizado: escopo empresa; interno: todos
- `POST /api/pagamentos` — terceirizado; unique `(funcionario_id, competencia)`
- Situação na listagem: Pendente / Em atraso / No prazo / Enviado em atraso (RF16 parcial; com comprovante via RF14)

## Frontend

- `/pagamentos` — terceirizado e interno (menu atualizado)
- Formulário **Registrar pagamento** + tabela com prazo e situação

## Deploy

Concluído em produção (20/09/2026): migrations `AddPagamentosFuncionario` + `AddPagamentoComprovanteArquivo` (RF14).

## Relacionado

- **RF14** — `docs/rf14-comprovante-implementacao.md`
- **RF16** — alertas/dashboard (pendente)
- **Resumo geral** — `docs/resumo-entregas.md`

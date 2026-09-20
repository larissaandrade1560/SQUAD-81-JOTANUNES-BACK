# RF16 — Alertas de atraso de comprovante (parcial → MVP)

## Backend

- `GET /api/dashboard/resumo` inclui contagem por `SituacaoComprovante` em todos os pagamentos:
  - `comprovantesPendentes`, `comprovantesEmAtraso`, `comprovantesNoPrazo`, `comprovantesEnviadosEmAtraso`
  - `pagamentosTotal`

## Frontend

- **Dashboard (interno):** card “Comprovantes em atraso” e painel “Alertas de comprovante (RF16)” com textos da API; link para `/pagamentos`.
- **Pagamentos:** cards MO-08a (total, aguardando, em atraso, no prazo) derivados da mesma listagem (`resumirComprovantes`).

## Fora deste MVP

- `/pendencias` agregada (RF18), fila validação real no dashboard (RF20), MO-11 dedicado.

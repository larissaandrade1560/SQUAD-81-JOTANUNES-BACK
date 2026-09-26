# RF20 — Dashboard gerencial (parcial → completo)

## Entregue (20/09/2026)

- **Fila de validação real** no dashboard (até 5 itens via `GET /api/validacao/fila`).
- Métrica **Documentos em análise** em `GET /api/dashboard/resumo` (`documentosValidacaoFila`).
- Removida fila demonstrativa (mock Alpha/Beta).
- **RF16** integrado: card e alertas de comprovante no mesmo dashboard.
- Subtitle referencia **RF18** (`/pendencias`).

## Ainda pendente (RF20 completo)

Alinhar ao RF20 em `requisitos-funcionais.md` (visão Jotanunes obrigatória):

1. **Indicadores documentais agregados** (além da fila): ex. contagem rejeitados/vencidos/pendentes validação — pode reutilizar contagens de `/api/pendencias` + fila ou estender `DashboardResumoResponse`.
2. **Atalhos** explícitos: Pendências, Validação, Pagamentos, **Consulta por obra** (RF19).
3. **Cards MO espelhados** no shell interno (Figma T-10 / MO-08a hints) — hoje métricas cadastrais + comprovantes + fila.
4. **Evitar efeito colateral:** dashboard chama `/api/validacao/fila` e move docs para *Em análise* — documentar ou usar endpoint read-only no preview (decisão produto).
5. **Não reintroduzir mocks** — qualquer hint estático deve vir da API ou ser removido.

## Critérios de aceite (incremento)

- Dashboard reflete situação real de pagamentos/comprovantes (RF16) e documentos (RF12) sem placeholders numéricos.
- Link “Ver fila completa” e “Ver pagamentos” já OK; adicionar “Ver pendências” / “Ver obras” quando RF19 fechar.

## Referências

- `frontend/src/pages/DashboardPage.tsx`
- `GetDashboardResumoUseCase.cs`
- Figma: T-10, JN-02 (fila)

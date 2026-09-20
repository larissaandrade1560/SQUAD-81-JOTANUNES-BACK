# Resumo das entregas — Jota Nunes Forms (Squad 81)

Documento consolidado até **20/09/2026**. Detalhes por RF em `docs/rf*-implementacao.md`. Dia 19/09: `docs/resumo-2026-09-19.md`.

**Repositório:** `SQUAD-81-JOTANUNES-BACK` · branch **`develop`**

---

## Ambiente de produção

| Componente | URL / recurso |
|------------|----------------|
| API | https://squad-81-jotanunes-back.onrender.com |
| Frontend | https://jotanunes-forms.pages.dev |
| Postgres | Render (workspace **JOTANUNES**) |
| Arquivos | Cloudflare R2 — bucket `jotanunes-docs` |

**Deploy**

- **Frontend:** push em `develop` → GitHub Actions → Cloudflare Pages (`VITE_API_URL` apontando para a API Render).
- **API:** Docker no Render; após push, **deploy manual** quando há migration ou env. Startup com `Database__ApplyMigrations=true`.
- **R2 na API:** `R2__AccountId`, `R2__AccessKeyId`, `R2__SecretAccessKey`, `R2__BucketName=jotanunes-docs`.

**Usuários de teste (seed)**

| Perfil | Documento | Senha |
|--------|-----------|-------|
| Admin | `00000000001` | `senha123` |
| Terceirizado (MO) | `11122233344` | `senha123` |
| Analista | `12345678900` | `senha123` |

---

## Entregas por requisito funcional

### RF01 — Autenticação JWT

Login integrado ao SPA; sessão `sessionStorage` (`jnf-auth-session`); rotas por perfil (admin / analista / terceirizado).

→ `docs/rf01-autenticacao-implementacao.md`

### RF02 / RF03 — Empresas e classificação MO · Materiais

CRUD empresas; tipo MO vs Materiais restringe menus e fluxos (MO: funcionários, pagamentos, comprovantes).

→ `docs/rf02-empresas-implementacao.md`

### RF04 — Obras

Cadastro e listagem de obras; integração com vínculos e dashboard.

→ `docs/rf04-obras-implementacao.md`

### RF05 — Funcionários (MO)

CRUD funcionários da empresa parceira; terceirizado escopado à própria empresa.

→ `docs/rf05-funcionarios-implementacao.md`

### RF06 — Vínculo funcionário ↔ obra

Associação funcionário–obra; métricas no dashboard.

→ `docs/rf06-funcionario-obras-implementacao.md`

### RF07 — Upload documentos empresariais (R2)

PDF até 10 MB; pendente; download URL presignada (15 min). Chave: `empresas/{empresaId}/documentos/{id}.pdf`.

**Fix crítico R2:** `DisablePayloadSigning` no `PutObject` (Cloudflare ≠ AWS streaming signature) — commit `90d60e0`.

→ `docs/rf07-documentos-empresa-implementacao.md`

### RF08 — Upload documentos do funcionário (R2)

Por funcionário; chave: `empresas/{empresaId}/funcionarios/{funcionarioId}/documentos/{id}.pdf`.

→ `docs/rf08-documentos-funcionario-implementacao.md`

### RF09 / RF10 — Validação documental e motivo de rejeição

Fila unificada; aprovar/rejeitar; rejeição com motivo obrigatório. `/validacao` + colunas de status/motivo nas listagens.

→ `docs/rf09-rf10-validacao-implementacao.md`

### RF11 — Reenvio de documentos rejeitados

Reenvio de PDF substituindo arquivo rejeitado (empresa e funcionário); estende fluxo RF07/RF08.

*(Implementado no pacote pós-RF10; ver commits e páginas de documentos.)*

### RF12 — Status documental (Em análise · Vencido · Nova versão)

- `ValidoAte` após aprovação (90 dias certidão negativa; 365 demais).
- `Pendente` → `EmAnalise` na fila; expirado → `Vencido`; reenvio também para `Vencido`.
- Migration `20260920153000_AddDocumentoValidoAte` (+ **Designer** obrigatório).

**Incidente Render (20/09):** `JotaNunesFormsDbContextModelSnapshot.cs` truncado quebrou build → restaurado (`6708e549`). Coluna `valido_ate` ausente em runtime → faltava `.Designer.cs` da migration → criado (`3ba9bb338`).

→ `docs/rf12-status-documental-implementacao.md`

### RF13 — Registro de pagamentos

- Entidade `PagamentoFuncionario`: competência (1º dia do mês), `data_pagamento`, unique `(funcionario_id, competencia)`.
- `GET/POST /api/pagamentos`; terceirizado registra; interno consulta todos.
- Migration `20260920163000_AddPagamentosFuncionario` (+ Designer).
- Frontend: `/pagamentos`, menu **Pagamentos**, formulário **Registrar pagamento**.

→ `docs/rf13-pagamentos-implementacao.md`

### RF14 — Upload de comprovante bancário

- Metadados + R2: `comprovante_*` em `pagamentos_funcionario`; `RegistrarComprovante`.
- `POST /api/pagamentos/{id}/comprovante` (PDF ≤ 10 MB).
- `GET /api/pagamentos/{id}/comprovante/download` (presign 15 min).
- Chave R2: `empresas/{empresaId}/pagamentos/{pagamentoId}/comprovante-{guid}.pdf`.
- Migration `20260920170000_AddPagamentoComprovanteArquivo` (+ Designer).
- UI: **Enviado em**, **Enviar comprovante**, **Substituir PDF**, **Visualizar**.

**Deploy:** commit `833ca78` — feat RF14; deploy manual Render **live** 20/09/2026.

→ `docs/rf14-comprovante-implementacao.md`

### RF15 — Prazo do comprovante (parcial, entregue com RF13)

`prazo_comprovante = data_pagamento + 3 dias corridos`; preview no formulário e coluna na tabela.

### RF16 — Identificação de atrasos (MVP)

- Situação na listagem de pagamentos (desde RF13/14).
- **Dashboard:** card + painel de alertas via `GET /api/dashboard/resumo`.
- **Pagamentos:** cards resumo (MO-08a) alinhados à tabela.

→ `docs/rf16-alertas-comprovante-implementacao.md`

### RF20 — Dashboard gerencial (parcial)

- Fila real de validação no dashboard (preview) + métrica **Documentos em análise** via `documentosValidacaoFila` em `GET /api/dashboard/resumo`.
- → `docs/rf20-dashboard-parcial-implementacao.md`

### RF18 — Consulta de pendências

- `GET /api/pendencias` + tela `/pendencias` (irregularidades documentais e comprovantes).
- → `docs/rf18-pendencias-implementacao.md` (**aguarda deploy API + Pages**)

### RF17 / RF19 — pendentes

| RF | Escopo |
|----|--------|
| RF17 | `/auditoria` — histórico de versões de documentos |
| RF19 | Consulta por obra (alocações) |

---

| Item | O que foi feito |
|------|------------------|
| CRUD usuários internos | Admin — `docs/crud-usuarios-internos.md` |
| Responsividade mobile | Drawer + botão **Menu** (≤767px), ajustes em `AppShell`, login, documentos, validação, pagamentos |
| CI frontend | `.github/workflows/deploy-cloudflare.yml` — lint, test, build Node 22 |
| Hexagonal + Postgres | Camadas Domain / Application / Infrastructure / Api |
| E2E RF07–RF10 (19/09) | Upload terceirizado + aprovar/rejeitar analista — ver `docs/resumo-2026-09-19.md` |

---

## Commits marcantes (19–20/09/2026)

| Commit / tema | Descrição |
|---------------|-----------|
| `43d20bc` | RF08–RF10 documentos funcionário + validação |
| `90d60e0` | Fix upload R2 Cloudflare |
| `6708e549` | Fix ModelSnapshot truncado (build Render) |
| `3ba9bb338` | Designer migration `AddDocumentoValidoAte` |
| RF13 (develop) | Pagamentos + migration `AddPagamentosFuncionario` |
| `833ca78` | RF14 comprovante R2 + migration `AddPagamentoComprovanteArquivo` |
| Mobile UX | AppShell drawer, CSS páginas internas (push develop) |

---

## Testes E2E em produção (browser) — 20/09/2026

**Pré-requisito:** API Render live com migrations RF13/RF14; Pages com bundle RF14.

### RF12 (parcial)

- `/validacao`: aprovar → **Válido até**; terceirizado vê coluna em `/documentos`.
- Fluxo **Vencido → Nova versão** não fechado no browser (depende de data/`valido_ate` no banco).

### RF13 + RF14 + RF15 — Terceirizado (`11122233344`)

1. **Registrar pagamento:** Funcionario RF05 E2E · competência 09/2026 · pagamento 20/09/2026.
2. **Prazo:** 23/09/2026 (RF15).
3. **Enviar comprovante PDF:** upload ok → **Visualizar** + **Substituir PDF**.
4. **Situação:** **No prazo** · **Enviado em** 20/09/2026 ~13:30.
5. **Visualizar:** nova aba R2 presignada (`…/pagamentos/…/comprovante-….pdf`).

### Analista (`12345678900`)

- `/pagamentos`: coluna **Empresa** (`Construtora Teste E2E Ltda`); mesma linha; só **Visualizar** (sem registrar/enviar).

### Mobile

- Viewport 375px: menu drawer, tabela com scroll horizontal — OK.

**Não testado nesta rodada:** **Em atraso** / **Enviado em atraso** (datas de pagamento/comprovante fora do prazo).

---

## Lições operacionais

1. **Migrations EF manuais:** sempre incluir **`.Designer.cs`** além do `.cs` e atualizar **ModelSnapshot** — senão EF acha “already up to date” e colunas não aparecem no Postgres.
2. **Render free:** cold start; após deploy manual, aguardar status **live** antes de E2E (404 em `/api/pagamentos` enquanto API antiga).
3. **R2:** mesmo padrão de assinatura S3 que RF07; comprovantes em prefixo `pagamentos/` dentro de `empresas/{id}/`.

---

## Pendências / próximo trabalho

| Prioridade | Item |
|------------|------|
| RF16 | **MVP entregue** — ver `docs/rf16-alertas-comprovante-implementacao.md` |
| RF20 | **Parcial entregue** — fila + métrica documentos — ver `docs/rf20-dashboard-parcial-implementacao.md` |
| RF18 | **Implementado local** — ver `docs/rf18-pendencias-implementacao.md` (deploy pendente) |
| RF17 / RF19 | Auditoria e consulta por obra |
| RF12 E2E | Cenário vencido + nova versão (seed ou SQL em `valido_ate`) |
| RF13/14 E2E | Cenários **Em atraso** e **Enviado em atraso** |
| ClickUp | Atribuir RF11–RF14 concluídos a Matheus — ver `docs/clickup/atribuicao-matheus-2026-09-19.md` e `docs/clickup/kanban-squad81.md` |
| Produto | Aprovação formal de comprovantes — **PENDENTE** (só consulta/rastreio hoje) |

---

## Referências

- Requisitos: `docs/requisitos/requisitos-funcionais.md`
- Mapa de telas: `docs/requisitos/mapa-de-telas.md`
- Kanban: `docs/clickup/kanban-squad81.md`

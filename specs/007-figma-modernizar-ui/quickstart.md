# Quickstart: validar piloto — UI Validação Documental

## Pré-requisitos

- Node.js conforme `frontend/package.json`.
- Backend e banco opcionais para teste manual E2E; testes automatizados do frontend usam mocks.
- Conta interna (Admin/Analista) para validação manual.
- Referência visual: [handoff Make](./contracts/ui-validacao-handoff.md).

Modelo UI: [data-model.md](./data-model.md). Pesquisa: [research.md](./research.md).

## 1. Gates automatizados (obrigatórios)

Na raiz do repositório:

```bash
npm --prefix frontend ci
npm --prefix frontend run lint
npm --prefix frontend test
npm --prefix frontend run build
```

Backend **não** é gate desta feature salvo regressão reportada; se tocar backend por engano, rodar também:

```bash
dotnet test backend/JotaNunesForms.sln --configuration Release
```

## 2. Validação manual — happy path

1. Subir API + frontend (Docker Compose ou processos locais conforme README do projeto).
2. Login como usuário **interno**.
3. Navegar para `/validacao`.
4. Confirmar:
   - breadcrumb, título, data no cabeçalho;
   - três cartões com contagens coerentes;
   - busca reduz linhas da tabela;
   - chip de status filtra;
   - **Validar** (aprovar) em um item e fila atualiza;
   - **Rejeitar** abre modal, exige motivo, confirma rejeição.
5. Comparar com checklist V1–V10 em [ui-validacao-handoff.md](./contracts/ui-validacao-handoff.md).

### 2.1 Roteiro funcional JN (SC-001)

| ID | Verificação | PASS |
|----|-------------|------|
| RF09 | **Validar** (aprovar) atualiza/remove item na fila após sucesso | |
| RF10 | Rejeitar exige motivo; envio sem motivo bloqueado | |
| RF12 | Ao abrir `/validacao`, itens refletem “Em análise” conforme backend após load (`status` / `statusRotulo`) | |

Registrar PASS/FAIL na PR ou abaixo em “Evidência de execução”.

## 3. Validação manual — negação de acesso

1. Login como terceirizado (MO ou Materiais).
2. Tentar acessar `/validacao` diretamente na URL.
3. **Esperado**: redirecionamento ou bloqueio conforme `InternalRoute` (sem dados da fila).

### 3.1 Evidência (SC-005 / FR-004)

| Data | Perfil testado | Resultado esperado | Resultado observado | OK? |
|------|----------------|--------------------|---------------------|-----|
| | Terceirizado MO ou Materiais | Sem fila / redirect / bloqueio UX | | |

## 4. Estados de interface

| Estado | Como provocar | Esperado |
|--------|----------------|----------|
| Loading | throttle rede ou primeira carga | Indicador de carregamento; sem tabela vazia enganosa |
| Erro | API indisponível ou 401 | Mensagem `role="alert"`; sem vazamento de stack |
| Vazio | fila `[]` | Empty state claro (copy existente ou alinhada ao Make) |

## 5. Revisão visual

1. Abrir Make na versão **Modernize page design layout**.
2. Exportar captura da tela **Validação Documental**.
3. Lado a lado com `/validacao` no browser (mesma resolução ~1440px).
4. Preencher checklist V1–V10; registrar desvios na tabela de desvios do contrato.

## 6. Evidência de execução (T020)

| Gate / seção | Comando ou passo | Resultado | Data |
|--------------|------------------|-----------|------|
| lint | `npm --prefix frontend run lint` | PASS (warnings pré-existentes em outras páginas) | 2026-09-27 |
| test | `npm --prefix frontend test` (Node 22) | PASS — 26 files, 50 tests | 2026-09-27 |
| build | `VITE_API_URL=https://api.example.com npm --prefix frontend run build` | PASS | 2026-09-27 |
| Manual §2 + §2.1 | Happy path + RF09/10/12 | Pendente validação manual E2E com API | |
| Manual §3 + §3.1 | Negação terceirizado | Coberto por `InternalRoute.test.tsx` (+ manual opcional) | 2026-09-27 |

## 7. Próximo passo

Implementar: `/speckit-implement` ou tarefas em [tasks.md](./tasks.md).

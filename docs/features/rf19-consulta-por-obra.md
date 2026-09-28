# Feature: RF19 — Consulta por obra (alocações)

> **Status:** Especificação pronta para implementação
> **Slug:** `rf19-consulta-por-obra`
> **Criado em:** 2026-09-28
> **Autor da spec:** Claude (a partir da spec do time + entrevista com Gabriel)

---

## 1. Resumo executivo

A equipe Jotanunes (Administrador/Analista) passa a ter uma página dedicada por obra (`/obras/:obraId`) que mostra quais empresas terceirizadas de mão de obra atuam ali e quais trabalhadores cada uma alocou, com uma leitura rápida de conformidade: situação documental resumida e situação de pagamentos/comprovantes por trabalhador. Documentos e pagamentos aparecem como links para as telas existentes — não há CRUD nesta visão.

---

## 2. Contexto e motivação

### 2.1 Problema
Hoje a consulta de alocações é um modal na lista de Obras ("Ver MO") com lista plana de funcionários: não agrupa por empresa, não tem deep link e não diz nada sobre conformidade.

### 2.2 Quem é afetado
Administradores e Analistas Jotanunes que acompanham a regularidade das parceiras por obra (UC-JN-10).

### 2.3 O que existe hoje
- `GET /api/obras/{id}/funcionarios` (policy `Interno`) → `ObraFuncionarioAlocacaoResponse` com `EmpresaRazaoSocial`.
- `ObrasPage.tsx` com modal "Alocações — {codigo}".
- RF06: terceirizado MO vincula funcionário ↔ obra.

### 2.4 Resultado esperado
Analista abre a obra, vê as terceirizadas agrupadas, identifica trabalhadores com documentação irregular ou comprovante em atraso e navega direto para os documentos/pagamentos do trabalhador.

---

## 3. Escopo

### 3.1 Dentro do escopo (MVP)
- Endpoint `GET /api/obras/{id}/visao-conformidade` (Opção B).
- Página `ObraDetalhePage` em `/obras/:obraId` (substitui o modal).
- Cabeçalho da obra: nome, código, cidade/UF, status ativo.
- Agrupamento por empresa parceira.
- Colunas: nome, CPF, cargo, status do funcionário, situação documental, situação de pagamentos, links Documentos/Pagamentos.
- `PagamentosPage` aceita `?funcionarioId=` e filtra a tabela (com "Limpar filtro").
- Empty state para obra sem alocações; 404 amigável para obra inexistente.

### 3.2 Fora do escopo (não fazer agora)
- Editar vínculos pela visão Jotanunes (continua no portal MO).
- Mapa ou cronograma da obra.
- Exportar CSV.

### 3.3 Roadmap futuro (v2+)
- Cadastro de tipos de documento obrigatórios por funcionário (hoje não existe) para situação "Incompleto".
- Deep link a partir de cards do dashboard.

---

## 4. Personas e usuários

| Persona | Papel/Permissão | Contexto de uso |
|---------|-----------------|-----------------|
| Analista Jotanunes | `Analista` (policy `Interno`) | Acompanhamento de conformidade por obra |
| Administrador Jotanunes | `Administrador` (policy `Interno`) | Idem + gestão de obras |
| Terceirizado MO | `Terceirizado` | **Sem acesso** (403 na API, redirect no front) |

---

## 5. Fluxos UX

### 5.1 Fluxo principal (happy path)
1. Analista acessa **Obras** e clica em **Ver alocações** na linha da obra.
2. Sistema navega para `/obras/:obraId` e carrega a visão de conformidade.
3. Cabeçalho mostra nome, código, local e badge Ativa/Inativa, mais contadores (empresas, trabalhadores, pendências).
4. Para cada empresa, uma seção com a razão social e a tabela de trabalhadores.
5. Analista clica **Documentos** → `/funcionarios/:id/documentos` (link de volta retorna à obra).
6. Analista clica **Pagamentos** → `/pagamentos?funcionarioId=:id` filtrado.

### 5.2 Fluxos alternativos
- Deep link direto em `/obras/:obraId` (ex.: colado no chat ou vindo do dashboard).
- Obra sem vínculos → empty state.

### 5.3 Telas e componentes
- `ObraDetalhePage.tsx` (nova), `ObrasPage.tsx` (botão vira link; modal removido), `PagamentosPage.tsx` (filtro), `FuncionarioDocumentosPage.tsx` (link de volta contextual).
- Componentes existentes: `PageHeader`, `Badge`, `Button`, `MetricCard`.

### 5.4 Estados de cada tela
- **Loading:** "Carregando…"
- **Vazio:** "Nenhum funcionário alocado nesta obra." + texto explicando que o vínculo é feito pela empresa MO.
- **Erro:** mensagem da API + botão "Tentar novamente"; 404 → "Obra não encontrada." com link para Obras.
- **Sucesso:** seções por empresa.

### 5.5 Copy crítico
- Botão na lista: **Ver alocações**.
- Subtitle: "Terceirizadas e trabalhadores alocados nesta obra (RF19)."
- Situação documental: Regular · Em análise · Irregular · Sem documentos.
- Situação de pagamentos: Em dia · Aguardando comprovante · Comprovante em atraso · Enviado em atraso · Sem pagamentos.

### 5.6 Plataformas e acessibilidade
- Web responsivo; tabelas com scroll horizontal (`overflow-x: auto`), padrão do projeto.
- Tabelas com `<th scope="col">`, seção de empresa com heading `h2`.
- i18n: somente pt-BR.

---

## 6. Regras de negócio e modelo de dados

### 6.1 Entidades
Sem entidades nem migrations novas. Usa `Obra`, `FuncionarioObra`, `Funcionario`, `Empresa`, `DocumentoFuncionario`, `PagamentoFuncionario`.

Resposta `ObraVisaoConformidadeResponse`:

| Campo | Tipo |
|-------|------|
| obra | `ObraResponse` |
| totalFuncionarios | int |
| empresas[] | `{ empresaId, razaoSocial, funcionarios[] }` |
| funcionarios[] | `{ funcionarioId, nome, cpf, cargo, ativo, documentos, pagamentos }` |
| documentos | `{ situacao, situacaoRotulo, total, aprovados, emAnalise, irregulares }` |
| pagamentos | `{ situacao, situacaoRotulo, total, emAtraso, pendentes }` |

### 6.2 Regras de validação
- `id` precisa ser GUID (rota `{id:guid}`); obra inexistente → 404 `{ message }`.

### 6.3 Regras de autorização
| Ação | Quem pode |
|------|-----------|
| Ler visão de conformidade | Administrador, Analista (policy `Interno`) |
| Terceirizado (MO ou Materiais) | 403 |

### 6.4 Estados e transições
Somente leitura. A consulta **não persiste** transições de vencimento: um documento `Aprovado` com `ValidoAte <= agora` é contado como `Vencido` apenas no cálculo.

### 6.5 Cálculos e derivações
**Situação documental** (pior status entre todos os documentos do funcionário):
1. Algum `Rejeitado` ou `Vencido` (inclui aprovado expirado) → `irregular` / "Irregular"
2. Senão, algum `Pendente` ou `EmAnalise` → `em_analise` / "Em análise"
3. Senão, todos `Aprovado` → `regular` / "Regular"
4. Sem documentos → `sem_documentos` / "Sem documentos"

**Situação de pagamentos** (pior situação de comprovante, `ObterSituacaoComprovante(hoje UTC)`):
1. Algum `EmAtraso` → `em_atraso` / "Comprovante em atraso"
2. Senão, algum `EnviadoEmAtraso` → `enviado_em_atraso` / "Enviado em atraso"
3. Senão, algum `Pendente` → `pendente` / "Aguardando comprovante"
4. Senão, todos `NoPrazo` → `em_dia` / "Em dia"
5. Sem pagamentos → `sem_pagamentos` / "Sem pagamentos"

Ordenação: empresas por razão social; funcionários por nome.

### 6.6 Limites e quotas
Sem paginação no MVP (volume por obra é baixo). Documentos e pagamentos carregados em lote por `funcionarioIds` (3 queries fixas, sem N+1).

### 6.7 Compliance e privacidade
CPF exibido formatado apenas para usuários internos (já é o caso no baseline). Nenhum dado de outra empresa é exposto a terceirizados.

---

## 7. Aspectos técnicos

### 7.1 Stack e padrões do projeto
.NET 8 hexagonal (Domain/Application/Infrastructure/Api), EF Core + PostgreSQL; React 19 + Vite + TS, react-router 7.

### 7.2 Bibliotecas
**Usar:** as já existentes. **Evitar:** novas dependências.

### 7.3 Integrações externas
_Não se aplica_ — nenhuma integração externa nova.

### 7.4 Autenticação e autorização
JWT existente; `[Authorize(Policy = "Interno")]`. Registrado em `specs/004-harden-access-security/contracts/access-matrix.json`. Front: rota sob `InternalRoute`.

### 7.5 Performance e escala
Consultas: obra, funcionários da obra, empresas, documentos por ids, pagamentos por ids — fixo, independente do número de funcionários.

### 7.6 Background jobs e async
_Não se aplica._

### 7.7 Observabilidade
Logs padrão de request; 403/404 já cobertos pelo `SecurityEventLogger`.

### 7.8 Testes
- Unitários (Application): agrupamento, pior status documental (incl. aprovado expirado), situação de pagamentos, obra inexistente, obra vazia.
- Integração (Api): matriz de papéis — admin/analista 200, MO/Materiais 403; inventário de endpoints.
- Front (Vitest): `ObraDetalhePage` renderiza grupos, links e empty state.
- E2E manual: seed "funcionário RF05 E2E" vinculado a obra → analista vê a linha.

### 7.9 Feature flag e rollout
Sem flag. Deploy: Render (backend, sem migration) + Cloudflare Pages (frontend) após merge em `develop`.

---

## 8. Edge cases e tratamento de erros

| Cenário | Comportamento esperado | Mensagem ao usuário |
|---------|------------------------|---------------------|
| Obra inexistente / GUID desconhecido | API 404 | "Obra não encontrada." + link para Obras |
| `obraId` não-GUID na URL | API 404 (rota não casa) | "Obra não encontrada." |
| Obra sem vínculos | 200 com `empresas: []` | "Nenhum funcionário alocado nesta obra." |
| Obra inativa | Exibe normalmente com badge "Inativa" | — |
| Funcionário inativo alocado | Aparece com badge "Inativo" | — |
| Empresa do funcionário não encontrada | Agrupado como "—" | — |
| Documento aprovado já vencido | Conta como irregular, sem persistir | "Irregular" |
| Terceirizado acessa `/obras/:id` | Front redireciona; API 403 | — |
| Falha de rede | Erro + "Tentar novamente" | Mensagem da API |
| `?funcionarioId` sem pagamentos | Tabela vazia + chip de filtro | "Nenhum pagamento para este funcionário." |

---

## 9. Plano de implementação

### 9.1 Etapas sugeridas (em ordem)
1. Portas: `ListByFuncionarioIdsAsync` em `IDocumentoFuncionarioRepository` e `IPagamentoFuncionarioRepository` + implementações EF.
2. DTOs + `GetObraVisaoConformidadeUseCase` + DI.
3. Endpoint no `ObrasController` + access-matrix + teste de papéis.
4. Testes unitários do use case.
5. Front: service, `ObraDetalhePage`, rota, `ObrasPage`, filtro em `PagamentosPage`, voltar contextual em documentos.
6. Testes Vitest, build, lint.

### 9.2 Arquivos provavelmente afetados
Backend: `Domain/Ports/IDocumentoFuncionarioRepository.cs`, `Domain/Ports/IPagamentoFuncionarioRepository.cs`, repositórios EF correspondentes, `Application/DTOs/ObraVisaoConformidadeResponse.cs` (novo), `Application/UseCases/Obras/GetObraVisaoConformidadeUseCase.cs` (novo), `Application/DependencyInjection.cs`, `Api/Controllers/ObrasController.cs`, `specs/004-harden-access-security/contracts/access-matrix.json`, testes.
Frontend: `services/obrasService.ts`, `pages/ObraDetalhePage.tsx|.css|.test.tsx` (novos), `routes/router.tsx`, `pages/ObrasPage.tsx`, `pages/PagamentosPage.tsx`, `pages/FuncionarioDocumentosPage.tsx`.

### 9.3 Dependências de outras features ou times
RF04, RF06, RF08/RF12, RF13–RF15 — todos já em `develop`.

### 9.4 Estimativa de esforço
_Não solicitada._

---

## 10. Critérios de aceite

- [ ] **CA1:** Analista abre a obra X e vê os funcionários alocados com a empresa MO visível.
- [ ] **CA2:** Os funcionários aparecem agrupados por terceirizada, deixando claro quais atuam na obra.
- [ ] **CA3:** Em cada linha, o link Documentos abre `/funcionarios/:id/documentos` do funcionário.
- [ ] **CA4:** Obra sem vínculos exibe mensagem clara, sem erro 500.
- [ ] **CA5:** Terceirizado recebe 403 em `GET /api/obras/{id}/visao-conformidade`; a rota de front é interna.
- [ ] **CA6:** Link Pagamentos abre `/pagamentos?funcionarioId=:id` com a tabela filtrada.
- [ ] **CA7:** Situação documental segue a regra de pior status (6.5).

---

## 11. Métricas de sucesso e observabilidade pós-launch

### 11.1 KPIs de produto
_Não definidos_ — projeto acadêmico sem analytics.

### 11.2 Eventos a instrumentar
_Não se aplica_ — o projeto não tem instrumentação de eventos de produto.

### 11.3 Plano de rollback
Revert do merge em `develop`. Sem migration, rollback é só redeploy.

---

## 12. Riscos e mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Status "Vencido" divergir do persistido | média | baixo | Regra documentada; listagem de documentos persiste ao abrir |
| Volume alto numa obra | baixa | médio | Consultas em lote; paginação no v2 se necessário |

---

## 13. Questões em aberto

- [ ] Lista oficial de documentos obrigatórios por funcionário (para situação "Incompleto") — fora do MVP.
- [ ] Figma T-32: conferir paridade visual quando o frame estiver disponível.

---

## 14. Decisões registradas (ADR-style)

- **Decisão:** Opção B (endpoint agregado `visao-conformidade`).
  **Razão:** situação documental exige dados de documentos; no front seria N+1.
- **Decisão:** Página dedicada `/obras/:obraId`, removendo o modal.
  **Razão:** deep link e uma única fonte de verdade para a visão.
- **Decisão:** Situação documental = pior status entre todos os documentos.
  **Razão:** não existe cadastro de documentos obrigatórios por funcionário.
- **Decisão:** Pagamentos via `?funcionarioId=` com resumo na linha.
  **Razão:** reaproveita `PagamentosPage` sem novo endpoint.
- **Decisão:** A consulta não persiste vencimento.
  **Razão:** GET sem efeitos colaterais; evita escrita/auditoria em massa numa leitura.

---

## 15. Referências

- `docs/requisitos/requisitos-funcionais.md` § RF19
- `docs/requisitos/casos-de-uso-por-ator.md` UC-JN-10
- `docs/rf04-obras-implementacao.md`, `docs/rf06-funcionario-obras-implementacao.md`
- `docs/rf12-status-documental-implementacao.md`

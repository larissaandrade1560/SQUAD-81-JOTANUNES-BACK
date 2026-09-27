# Research: Modernização UI — Validação Documental (piloto Make)

**Feature**: `007-figma-modernizar-ui`  
**Date**: 2026-09-27

## R1 — Handoff Figma Make sem `node-id`

**Decision**: Tratar o link Share do Make (`qzLAx7CMCI18I4zMpsTvtv`, versão **Modernize page design layout**) + captura de tela como referência canônica do piloto; usar inspeção no Make (`</>`) e, quando útil, Figma MCP com `makeFileKey` para extrair hierarquia visual — sem bloquear entrega por ausência de `node-id`.

**Rationale**: O produto Figma Make não expõe “Copy link to selection” por frame na prática observada pelo time; a spec já assume URL + versão + captura.

**Alternatives considered**:
- Exigir migração prévia de frames para `figma.com/design/...` — rejeitado por atrito operacional no piloto.
- Colar código exportado do Make no repositório — rejeitado (viola fronteiras do frontend e constituição).

## R2 — Escopo de dados e API

**Decision**: **Nenhuma mudança** em contratos HTTP ou backend; métricas, busca e filtros de status são **derivados no cliente** a partir de `listValidacaoFila()` (`ValidacaoDocumentoItem[]`).

**Rationale**: A spec limita a feature a modernização visual com paridade funcional; a fila já expõe `status` / `statusRotulo` e campos textuais para busca.

**Alternatives considered**:
- Novo endpoint de agregação para cards — rejeitado (escopo especulativo, YAGNI).

## R3 — Ação “Validar” vs Aprovar/Rejeitar (RF09/RF10)

**Decision**: Alinhar o **CTA primário da linha** ao Make com label **Validar** (ou **Validar documento**), mantendo **rejeição com motivo obrigatório** acessível na mesma linha (botão secundário **Rejeitar** ou fluxo equivalente já existente). **Aprovar** pode ser fundido no primário “Validar” quando o fluxo atual for aprovação direta, documentando no contrato UI que “Validar” dispara a mesma operação que hoje é **Aprovar**, sem remover o modal de rejeição.

**Rationale**: O Make mostra um botão vermelho “Validar”; o produto exige RF10 (motivo na rejeição). Renomear sem perder as duas ações satisfaz FR-005 e SC-001.

**Alternatives considered**:
- Substituir Aprovar/Rejeitar por só “Validar” abrindo nova rota — rejeitado até existir tela JN de detalhe no app (fora do escopo).
- Manter labels antigas — rejeitado para aceite visual do piloto.

## R4 — Componentes e tokens

**Decision**: Reutilizar `MetricCard`, `PageHeader`, `Button`, `Badge`, `Input`; estender `MetricCard` com tom `info` (cartão “Em análise”) se o CSS atual não cobrir; adicionar blocos de layout em `ValidacaoPage.css` (grid de métricas, toolbar de busca/filtros) em vez de novo framework CSS.

**Rationale**: `MetricCard` já existe em `components/dashboard/`; design system do repo usa CSS variables em `index.css`, não Tailwind.

**Alternatives considered**:
- Novo pacote de UI ou cópia integral do CSS do Make — rejeitado.

## R5 — Breadcrumb e data no cabeçalho

**Decision**: Compor breadcrumb **Operações / Validação documental** e data formatada (`pt-BR`, data local do cliente) acima ou dentro do cabeçalho da página, sem alterar `AppShell` global neste piloto.

**Rationale**: FR-005 exige breadcrumb e data; `PageHeader` pode ganhar props opcionais `breadcrumb` e `meta` ou um wrapper local na página para evitar impacto em todas as telas.

**Alternatives considered**:
- Breadcrumb global no `AppShell` — rejeitado (escopo maior que o piloto).

## R6 — Colunas da tabela vs Make

**Decision**: Priorizar colunas **funcionais e de requisitos** (escopo, empresa, funcionário, tipo, status, ações); **mapear** colunas do Make (CPF/CNPJ, datas) quando os dados já existirem em `ValidacaoDocumentoItem` ou puderem ser exibidos sem novo campo sensível. Não adicionar colunas que a API não autoriza.

**Rationale**: Edge case de privacidade na spec; tipo atual já inclui empresa, funcionário, arquivo, datas.

**Alternatives considered**:
- Redesenhar tabela idêntica ao Make com colunas vazias — rejeitado.

## R7 — Testes

**Decision**: Adicionar testes de componente para `ValidacaoPage` (estados loading/erro/vazio, filtro/busca client-side, presença de métricas e CTAs) com mock de `validacaoService`; executar gates `lint`, `test`, `build` do frontend.

**Rationale**: Constituição exige testes proporcionais a mudanças de comportamento/UI de risco moderado.

**Alternatives considered**:
- Somente validação manual — rejeitado para merge em `develop`.

## R8 — Agregação de cartões e chips

**Decision**: Métricas e filtros seguem as regras fechadas em `data-model.md` (total / `status === 1` / `status !== 1`).

**Rationale**: Evita divergência Make ↔ implementação e estabiliza `validacaoFilaUtils.test.ts`.

**Alternatives considered**:
- Rótulos dinâmicos sem regra — rejeitado (métricas inconsistentes entre revisores).

## Replicação para próxima tela Make

Após o piloto, repetir: atualizar handoff (URL, versão, rota, screenshot), copiar seção “Próxima tela” no contrato UI, rodar `/speckit-plan` leve só se o escopo mudar de módulo; caso contrário clonar fases A–E do `plan.md` com nova rota e arquivo de página.

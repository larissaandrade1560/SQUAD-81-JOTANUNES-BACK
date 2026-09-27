# Feature Specification: Modernização de telas via referência Figma

**Feature Branch**: `007-figma-modernizar-ui`

**Created**: 2026-09-27

**Status**: Draft

**Input**: User description: "Modernizar a interface usando o Figma Make (versão Modernize page design layout) como referência visual, com handoff pelo link Share do Make (sem node-id por frame), começando pela tela Validação Documental no produto. Link: https://www.figma.com/make/qzLAx7CMCI18I4zMpsTvtv/Modernizar-design-de-p%C3%A1ginas--Copy-?t=OfDfRYLtIbP7UOrz-1"

## Referência de handoff (piloto)

| Campo | Valor |
|-------|--------|
| Arquivo Make | [Modernizar design de páginas (Copy)](https://www.figma.com/make/qzLAx7CMCI18I4zMpsTvtv/Modernizar-design-de-p%C3%A1ginas--Copy-?t=OfDfRYLtIbP7UOrz-1) |
| Identificador do arquivo | `qzLAx7CMCI18I4zMpsTvtv` |
| Versão ativa no Make | **Modernize page design layout** (histórico de 2026-09-27) |
| Tela no Make | **Validação Documental** — dashboard com breadcrumb, título e data, três cartões de resumo (totais por status), busca, filtros por status e tabela com ação **Validar** por linha |
| Tela no produto | Fila **Validação documental** — rota `/validacao`, perfil interno Jotanunes (mesmo fluxo já existente nos requisitos JN) |
| Identificação quando não há `node-id` | URL Make + nome da versão + nome da tela + captura de tela exportada (Share do Make não fornece link por seleção) |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Definir referência de design para uma tela existente (Priority: P1)

Um desenvolvedor ou designer responsável pela modernização abre o arquivo Figma Make indicado, confirma a versão **Modernize page design layout**, identifica a tela **Validação Documental** e registra o vínculo explícito com a rota `/validacao` e os estados principais já suportados pelo produto (listagem com dados, vazio, carregando, erro), sem alterar regras de negócio ou permissões.

**Why this priority**: Sem mapeamento claro design ↔ tela do produto, qualquer exportação de código ou inspeção visual gera retrabalho e inconsistência entre módulos.

**Independent Test**: Verificar que o handoff do piloto lista URL Make, versão, nome da tela, rota `/validacao` e estados cobertos, sem necessidade de já ter código alterado.

**Acceptance Scenarios**:

1. **Given** o arquivo Make e o mapa de telas do produto, **When** o responsável confirma o piloto, **Then** o vínculo **Validação Documental (Make)** ↔ **`/validacao`** fica registrado com URL Make, nome da versão e data da referência.
2. **Given** a tela piloto no produto, **When** o design ou os requisitos exigem variações (vazio, loading, erro), **Then** o handoff lista cada variação a refletir na interface ou justifica manter o comportamento já documentado nos requisitos JN.

---

### User Story 2 - Modernizar a interface preservando comportamento e acesso (Priority: P1)

Um usuário interno Jotanunes autenticado acessa `/validacao` após a modernização e consegue realizar as mesmas tarefas principais de antes (consultar fila, filtrar/buscar, abrir validação de um documento), com layout, hierarquia visual, cartões de resumo, tabela e CTAs alinhados à referência **Validação Documental** do Make, sem perda de feedback de erro ou estados bloqueados documentados nos requisitos.

**Why this priority**: O objetivo declarado é modernizar páginas; o valor só é entregue se a experiência visual melhora sem quebrar fluxos críticos nem contornar controles de acesso.

**Independent Test**: Executar o roteiro de aceite do fluxo de validação documental JN (cenários felizes e de negação) na versão modernizada e comparar com o comportamento funcional esperado nos requisitos.

**Acceptance Scenarios**:

1. **Given** um usuário interno com permissão para validação, **When** ele usa a fila em `/validacao`, **Then** consegue localizar documentos, ver status e iniciar validação como antes, com resultados consistentes com os requisitos do módulo.
2. **Given** um usuário sem permissão para o módulo ou recurso, **When** ele tenta acessar `/validacao` ou ações de validação, **Then** o sistema nega o acesso conforme política existente (sem expor dados de outras empresas).
3. **Given** a referência visual da tela **Validação Documental** no Make, **When** um revisor compara a implementação com o handoff, **Then** breadcrumb, título, cartões de métricas, busca, filtros, colunas principais da tabela e botão **Validar** estão alinhados aos critérios de aceite visual registrados.

---

### User Story 3 - Reutilizar padrões visuais do produto em vez de colar código gerado (Priority: P2)

O time implementa a modernização adaptando a referência do Make aos componentes e tokens já adotados no frontend, tratando inspeção no Make (ícone de desenvolvimento) ou qualquer export apenas como referência de layout e conteúdo, não como entrega copiada integralmente.

**Why this priority**: Código sugerido pelo Make costuma ignorar arquitetura do projeto, acessibilidade e reutilização; a constituição do produto exige fronteiras claras entre páginas, serviços e componentes.

**Independent Test**: Revisar o diff de `/validacao` e confirmar que integrações de dados permanecem na camada de serviços e que elementos repetidos usam componentes compartilhados quando já existem equivalentes.

**Acceptance Scenarios**:

1. **Given** medidas ou marcação obtidas no modo de inspeção do Make, **When** o desenvolvedor implementa a tela, **Then** elementos repetidos usam componentes compartilhados existentes ou novos componentes justificados no handoff.
2. **Given** a necessidade de dados da fila de validação, **When** a implementação é inspecionada, **Then** a página não passa a concentrar chamadas de integração que deviam permanecer na camada de serviços do frontend.

---

### User Story 4 - Escalar o processo para demais páginas do arquivo Make (Priority: P3)

Após o piloto em `/validacao` aprovado, o time repete o mesmo fluxo (mapeamento, handoff com URL Make + versão + captura, implementação, revisão visual e funcional) para outras telas do mesmo arquivo Make, priorizadas conforme roadmap.

**Why this priority**: O arquivo Make sugere múltiplas páginas; o processo replicável reduz custo das próximas entregas.

**Independent Test**: Aplicar o checklist de handoff a uma segunda tela apenas em rascunho, verificando que os mesmos artefatos e gates se aplicam sem exigir `node-id`.

**Acceptance Scenarios**:

1. **Given** o piloto concluído, **When** o time seleciona a próxima tela no Make, **Then** utiliza o mesmo modelo de vínculo (URL Make, versão, nome da tela, rota) e o mesmo roteiro de validação.
2. **Given** duas telas modernizadas em sequência, **When** um usuário navega entre elas, **Then** a experiência visual permanece coerente salvo divergências documentadas no design.

---

### Edge Cases

- O layout no Make diverge do arquivo canônico [JotaNunesForms Design System](https://www.figma.com/design/Pu4udm2alIAtdGScbIqLBN): para o piloto, prevalece a tela **Validação Documental** aprovada no Make; divergências com requisitos funcionais escritos devem ser resolvidas antes de concluir.
- Share do Make retorna apenas URL do arquivo (sem `node-id`): handoff MUST incluir versão **Modernize page design layout**, nome da tela e captura exportada; bloquear “concluído” se a referência visual não for identificável.
- Exportação ou sugestão de código do Make gera estrutura inacessível: a implementação MUST corrigir contraste (incluindo CTAs como **Validar**), foco de teclado e mensagens perceptíveis.
- A fila exibe dados sensíveis: a modernização MUST NOT aumentar colunas ou detalhes além do que a API autoriza para o perfil; cenários de negação do módulo JN permanecem obrigatórios.
- Make sem estado vazio/erro para a listagem: manter estados já existentes em `/validacao` ou documentados nos requisitos, sem remover feedback ao usuário.
- Link Make ou versão restaurada para outra entrada do histórico: atualizar handoff antes de marcar entrega concluída.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O escopo MUST documentar handoff do arquivo Make [Modernizar design de páginas (Copy)](https://www.figma.com/make/qzLAx7CMCI18I4zMpsTvtv/Modernizar-design-de-p%C3%A1ginas--Copy-?t=OfDfRYLtIbP7UOrz-1) para rotas do produto, incluindo versão ativa, nome da tela e estados visuais obrigatórios; quando não houver `node-id`, MUST usar URL Make + versão + captura de tela.
- **FR-002**: A primeira entrega MUST modernizar visualmente a tela **Validação Documental** do produto (rota `/validacao`, fila de validação documental Jotanunes) conforme a versão **Modernize page design layout** do Make, mantendo paridade funcional com os requisitos já publicados para esse fluxo.
- **FR-003**: Materiais obtidos via inspeção no Make ou exportação MUST ser traduzidos para a estrutura de componentes e layouts do frontend existente; código sugerido pelo Make MUST NOT ser integrado literalmente se violar convenções de organização ou duplicar componentes equivalentes.
- **FR-004**: A tela modernizada MUST preservar regras de autenticação e autorização já aplicadas em `/validacao` (rota interna); mudança visual MUST NOT substituir validação no servidor.
- **FR-005**: O handoff do piloto MUST registrar critérios de aceite visual verificáveis para: breadcrumb/caminho, título (**Validação Documental**, D maiúsculo em Documental) e data, três cartões de resumo, campo de busca, filtros de status, colunas da tabela e ação **Validar** por linha.
- **FR-006**: Onde o design system canônico definir tokens ou componentes para o mesmo elemento, a implementação MUST priorizar esses padrões; desvios em favor do layout do Make MUST ser listados no handoff com motivo.
- **FR-007**: Após o piloto, o processo MUST ser reutilizável para demais telas do mesmo arquivo Make, com priorização acordada com o roadmap.
- **FR-008**: Regressões funcionais detectadas em testes manuais ou automatizados do módulo de validação MUST ser corrigidas antes de considerar a modernização concluída.

### Key Entities

- **Referência de design (Make)**: Arquivo `qzLAx7CMCI18I4zMpsTvtv`, versão nomeada, tela **Validação Documental**, data de aprovação e capturas por estado quando aplicável.
- **Tela do produto**: `/validacao` — fila de validação documental para usuários internos, tarefas e permissões conforme requisitos JN.
- **Registro de handoff**: Documento que liga referência Make à rota, critérios de aceite visual e checklist de estados.
- **Critério de aceite visual**: Alinhamento verificável de layout e conteúdo entre implementação e referência Make, sem prescrever tecnologia.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Para `/validacao`, 100% dos cenários de aceite funcional já definidos para validação documental JN passam após a modernização (sem regressão conhecida em aberto).
- **SC-002**: Revisão visual estruturada do piloto atinge pelo menos 90% dos itens do handoff (FR-005) na primeira rodada, ou 100% após um ciclo de ajuste documentado.
- **SC-003**: Membros do time completam o roteiro principal da fila (buscar, filtrar, iniciar validação) em tempo igual ou inferior ao observado antes da modernização, em pelo menos 3 execuções consecutivas. Validação de SC-003 é **pós-implementação** (aceite); não bloqueia merge se SC-001, SC-002 e SC-005 estiverem atendidos.
- **SC-004**: O handoff do piloto permite iniciar a segunda tela do Make em até 1 dia útil sem redefinir processo (mesmos artefatos: URL, versão, captura, rota).
- **SC-005**: Nenhum novo vazamento de dados entre empresas ou perfis é introduzido nos cenários de negação testados para validação documental.

## Assumptions

- O link Share do Make (`figma.com/make/...?t=...`) é a referência canônica de compartilhamento do arquivo; links por frame (`node-id`) não são exigidos para esta feature quando o Make não os fornece.
- A versão **Modernize page design layout** no histórico do Make é a baseline visual acordada para o piloto em 2026-09-27.
- O arquivo de design system canônico permanece referência de longo prazo para tokens e biblioteca; o Make pode antecipar visual que depois será refletido no DS.
- Inspeção no Make (modo desenvolvimento) substitui, para este piloto, o Dev Mode do arquivo `.design` quando não houver frame linkável.
- A modernização é **somente frontend**: contratos de API, regras de domínio e matriz de acesso só mudam se requisito separado exigir.
- Acessibilidade mínima: contraste legível em CTAs (incluindo **Validar**), teclado em busca/filtros/tabela e mensagens de erro perceptíveis.
- Idioma da interface permanece português, alinhado aos textos da referência Make salvo ajuste aprovado no handoff.
- Gates de qualidade do frontend (lint, testes, build) permanecem obrigatórios para concluir a entrega.

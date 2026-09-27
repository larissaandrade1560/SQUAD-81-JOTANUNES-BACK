# UI contract: handoff Make → `/validacao`



## Referência de design



| Item | Valor |

|------|--------|

| Make URL | https://www.figma.com/make/qzLAx7CMCI18I4zMpsTvtv/Modernizar-design-de-p%C3%A1ginas--Copy-?t=OfDfRYLtIbP7UOrz-1 |

| Versão | Modernize page design layout (2026-09-27) |

| Tela | Validação Documental |

| Rota produto | `/validacao` (`ValidacaoPage`) |

| DS canônico (secundário) | https://www.figma.com/design/Pu4udm2alIAtdGScbIqLBN — página Jotanunes Flow quando alinhamento de tokens for necessário |

| Copy aprovada do título | **Validação Documental** (D maiúsculo em Documental) |



## Estados cobertos (handoff)



| Estado | Coberto na UI |

|--------|----------------|

| Listagem com dados | Sim |

| Vazio | Sim |

| Carregando | Sim |

| Erro | Sim |



Screenshot/export: referência Make Share (versão **Modernize page design layout**); captura local opcional na PR de implementação 007.



## Checklist de aceite visual (FR-005)



Revisor marca ✅/❌ sem ler código.



| # | Elemento | Critério |

|---|----------|----------|

| V1 | Breadcrumb | Exibe caminho equivalente a **Operações / Validação documental** (ou **Validação Doc** conforme copy aprovada no handoff) |

| V2 | Título | **Validação Documental** (capitalização consistente com Make) |

| V3 | Data | Data contextual visível no cabeçalho (ex.: “15 de março de 2024” → em produção usar data atual formatada `pt-BR`) |

| V4 | Cartões | Três métricas: total, aguardando validação, em análise — valores coerentes com a fila carregada (`data-model.md`) |

| V5 | Busca | Campo “Buscar por nome…” (ou equivalente) filtra linhas visíveis |

| V6 | Filtros | Chips/botões de status (ex.: Todos, Aguardando, Em análise) filtram a tabela |

| V7 | Tabela | Colunas legíveis; status com indicador visual (badge/dot) |

| V8 | Ação linha | CTA primário alinhado ao Make (**Validar**); rejeição ainda acessível |

| V9 | Estados | Loading, erro e vazio tratados sem layout quebrado |

| V10 | Contraste | CTA primário e texto legíveis (WCAG AA alvo em elementos críticos) |



Meta de spec: ≥ 90% dos itens na primeira rodada (SC-002).



## Matriz de colunas (Make ↔ produto)



| Coluna Make (referência) | Campo / origem | Exibir no piloto? |

|--------------------------|----------------|-------------------|

| Tipo de documento | `tipoRotulo` | Sim |

| Solicitante | `empresaRazaoSocial` e/ou `funcionarioNome` (conforme `escopo`) | Sim |

| CPF / CNPJ | *não presente em `ValidacaoDocumentoItem`* | **Não** — ver desvios |

| Data solicitação | `enviadoEm` | Sim |

| Atualização | *sem campo dedicado na fila* | **Não** (ou só `enviadoEm`) — ver desvios |

| Status | `statusRotulo` + Badge | Sim |

| Ações | Validar / Rejeitar | Sim |

| Escopo, Código, Arquivo, Tamanho | campos atuais da página | Manter se úteis para RF09; ajustar densidade na PR |



Preencher coluna “Exibir” final na PR após T023.



## Comportamento funcional invariável



| ID | Requisito |

|----|-----------|

| RF09 | Aprovar documento na fila continua disponível (via CTA alinhado ao Make) |

| RF10 | Rejeição exige motivo no modal/form existente |

| RF12 | Comportamento ao carregar fila (transição para em análise) permanece o do backend — UI não simula mudança de status sem resposta da API |

| AUTH | Rota permanece sob `InternalRoute` — terceirizados não acessam `/validacao` |

| UX-VALIDAR | O botão **Validar** na linha executa a **mesma ação que Aprovar** hoje (`aprovarDocumentoValidacao`); não abre tela JN-03 de análise até existir rota dedicada no app |



API inalterada: ver `frontend/src/services/validacaoService.ts` e endpoints `/api/validacao/*`.



## Desvios documentados



| Desvio | Motivo |

|--------|--------|

| CTA “Validar” na tabela | Make sugere fluxo de análise; MVP mantém aprovação inline + modal Rejeitar (RF10) — `research.md` R3 |

| CPF/CNPJ na tabela | Campo ausente na fila API — privacidade e escopo |

| Coluna “Atualização” | Sem campo na fila; usar `enviadoEm` ou omitir |

| Tabela densa (escopo/arquivo/tamanho) | Metadados em sublinha sob tipo de documento — FR-006 / legibilidade |
| Tokens visuais | `frontend/src/styles/tokens.css` + componentes `ui/*` e `MetricCard` — Make como referência de hierarquia |



## Próxima tela Make (template — US4)



| Campo | Valor |

|-------|--------|

| Make URL | |

| Versão | |

| Nome da tela | |

| Rota produto | |

| Screenshot | |

| Checklist | Reutilizar V1–V10 adaptando copy |



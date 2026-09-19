# Requisitos do sistema

Documentação de requisitos do **JotaNunesForms** (Squad 81).

| Documento | Conteúdo |
|-----------|----------|
| [Casos de uso por ator](./casos-de-uso-por-ator.md) | UC por Jotanunes / Mão de Obra / Materiais |
| [Requisitos funcionais (RF01–RF20)](./requisitos-funcionais.md) | Catálogo de RF |
| [Figma v2 — fluxos](./fluxos-figma-v2.md) | Prompt de arquitetura de navegação |
| [Figma v3 — revisão e correções](./fluxos-figma-v3-e-correcoes.md) | Correções dos shells + Materials Flow v1 |
| [Figma — revisão e próximos passos](./figma-proximos-passos-e-correcoes.md) | Correções da Auditoria + escopo final do Materials Flow |
| [Figma — revisão v4](./figma-revisao-v4.md) | Estabilização MO/JN, documento do funcionário, pagamentos e handoff |
| [Mapa de telas](./mapa-de-telas.md) | Navegação, telas e componentes |

## Figma

https://www.figma.com/design/Pu4udm2alIAtdGScbIqLBN

Concluído: correções da Auditoria; regressão da sidebar de `MAT-01a` (mensagem de aprovação só em `MAT-04b`); **Materials Flow v1** em `08 — Materials Flow` (`MAT-02` a `MAT-07`), com `MAT-01a/b` em `07 — Shell Materiais` e índice entre as páginas.

Concluído também: **Validação documental Jotanunes** em `10 — Jotanunes Flow` (`JN-02`…`JN-04`, `JN-03f`/`JN-03g`), alinhada ao Materials Flow e à Auditoria; correções do checklist em [figma-proximos-passos-e-correcoes.md](./figma-proximos-passos-e-correcoes.md); `T-52`/`T-53` marcados como legado.

Concluído: **Mão de Obra v1** em `09 — Mao de Obra Flow` (`MO-02`…`MO-07`) e **rodada v4** ([figma-revisao-v4.md](./figma-revisao-v4.md)): correções MAT/JN, `MO-02d`, PDF `JN-03f1–f3`, documentos do funcionário (`MO-05a–c`), **pagamentos** (`MO-08`…`MO-11`) e visão Jotanunes (`JN-05`…`JN-06`).

Rodada **v4 concluída no Figma** ([figma-revisao-v4.md](./figma-revisao-v4.md)): tabs/`Voltar` no detalhe, `MO-05d`, vínculos `MO-06e/f`, Select em `MO-06b`, anotações externas, pagamentos MO/JN.

Próximo: implementação React (menus por `UserRole` + `SupplierType` e componentes compartilhados).

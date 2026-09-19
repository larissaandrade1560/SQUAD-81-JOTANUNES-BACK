# Mapa de telas — JotaNunesForms

**Versão:** 3.8  
**Data:** 2026-09-16  
**Revisão/correções:** [gestao-de-usuarios.md](./gestao-de-usuarios.md) · [figma-correcoes-finais.md](./figma-correcoes-finais.md) · [figma-proximos-passos-e-correcoes.md](./figma-proximos-passos-e-correcoes.md)  
**Arquitetura de navegação:** [fluxos-figma-v2.md](./fluxos-figma-v2.md)  
**Casos de uso:** [casos-de-uso-por-ator.md](./casos-de-uso-por-ator.md)  
**RF:** [requisitos-funcionais.md](./requisitos-funcionais.md)  
**Figma:** https://www.figma.com/design/Pu4udm2alIAtdGScbIqLBN  

---

## 1. Entrada e perfis

```text
LOGIN → UserRole (+ SupplierType quando Terceirizado)
  ├─ Jotanunes (Administrador | Analista) → Shell Jotanunes
  ├─ Terceirizado + MaoDeObra             → Shell Mão de Obra
  └─ Terceirizado + Materiais             → Shell Materiais
```

`Mão de Obra` e `Materiais` são **tipo de empresa**, não roles de autenticação.

---

## 2. Navegação por contexto

| Contexto | Menu |
|----------|------|
| Jotanunes (Admin) | Dashboard · Empresas · **Usuários e acessos** · Obras · Funcionários · Validação · Pagamentos · Pendências · Auditoria |
| Jotanunes (Analista) | Igual ao operacional **sem** `Usuários e acessos` (estado `JN-08x` se URL direta) |
| Mão de Obra | Dashboard · Minha Empresa · Funcionários · Obras / Vínculos · Documentos · Pagamentos · Pendências |
| Materiais | Empresa · Documentos · Pendências |

O rodapé da área interna exibe **nome do usuário + perfil** (ex.: `Mariana Souza` / `Analista`), nunca `Admin/Analista`.

Materiais não expõe funcionários, folha, pagamentos, comprovantes salariais nem vínculo de trabalhador a obra — nem desabilitados.

---

## 3. Organização do arquivo Figma

| Página | Conteúdo |
|--------|----------|
| `Foundations` + páginas de componente | Design System v1 |
| `02b — Components v2` | Componentes novos da fase v3 |
| `03 — Flows / IA` | Arquitetura dos três contextos |
| `04 — Auth` | Login |
| `05 — Shell Jotanunes` | Dashboard interno (`JN-01`) |
| `06 — Shell Mão de Obra` | Shell MO |
| `07 — Shell Materiais` | `MAT-01a` / `MAT-01b` Minha Empresa (visualização e edição) |
| `08 — Materials Flow` | Fluxo documental completo (`MAT-02` a `MAT-07`), frames soltos e indexados |
| `09 — Mao de Obra Flow` | **Mão de Obra v1** — funcionários, documentação, pendências, obras/vínculos, pagamentos/comprovantes (`MO-02`…`MO-11`, `MO-09e`) |
| `10 — Jotanunes Flow` | **Validação documental** + visão interna de pagamentos (`JN-02`…`JN-06`, `JN-05a`…`d`) |
| `11 — Usuários e Acessos` | **Gestão de usuários terceirizados** (Admin) — `JN-08`…`JN-10`, `JN-EMP-USR`, `AUTH-02`…`e` |
| `T-70 Auditoria` | Auditoria com conteúdo real (`JN-07`) + eventos administrativos de usuários |

---

## 4. Correções aplicadas

| # | Correção | Status |
|---|----------|--------|
| 1 | `Minha Empresa` com dois estados: visualização `[Editar dados]` / edição `[Cancelar] [Salvar alterações]` | Aplicada — `MAT-01a` / `MAT-01b` |
| 2 | CNPJ e Tipo da empresa somente leitura (+ Nome Fantasia opcional) | Aplicada |
| 3 | Sidebar interna com `Funcionários`, mantendo `Pagamentos` | Aplicada |
| 4 | Rodapé interno com nome + perfil reais | Aplicada |
| 5 | Placeholder `Conteúdo conforme casos de uso` removido (Auditoria com log real) | Aplicada |
| 6 | Cores, spacing e radius vinculados a Figma Variables | Aplicada nas telas v3 |
| 7 | Auditoria: coluna `Justificativa` renomeada para `Detalhes` (conteúdo contextual por ação) | Aplicada — `JN-07` |
| 8 | Auditoria: `Exportar CSV` removido do MVP, registrado como `Backlog — Exportação de auditoria` | Aplicada |
| 9 | Auditoria: escopo obrigatório é envio/reenvio/aprovação/rejeição; `Classificou` e `Validou` marcados como `Auditoria ampliada — validar escopo com requisitos/backend` | Aplicada |
| 10 | `MAT-01a`: topo da sidebar restaurado (`jotanunes FORMS` + `Empresa parceira` / `Materiais`); textos de aprovação removidos da sidebar | Aplicada |
| 11 | Mensagem de aprovação apenas em `MAT-04b` (`Alert/Aprovado` no conteúdo principal) | Aplicada |
| 12 | Sidebars dos frames clonados após a regressão (`MAT-02b/c`, `MAT-04c`, `MAT-05c`, `MAT-06a–d`) alinhadas ao padrão de `MAT-01b` | Aplicada |

A página `07 — Shell Materiais` mantém `MAT-01a` / `MAT-01b` e um índice apontando para o fluxo completo em `08 — Materials Flow`.

---

## 5. Materials Flow v1 — página `08`

Frames soltos (um por tela), indexados no topo da página. `MAT-01a` / `MAT-01b` permanecem em `07 — Shell Materiais`, com anotação cruzada entre as duas páginas.

### Documentos

| ID | Tela | Conteúdo |
|----|------|----------|
| MAT-02 | Documentos | Resumo (5 · 1 aprovado · 1 em análise · 3 pendências), FilterBar, tabela `Documento · Status · Enviado em · Atualizado em · Ações`, paginação |
| MAT-02b | Documentos · loading | Skeleton de header, cards e tabela |
| MAT-02c | Documentos · erro | “Não foi possível carregar os documentos.” + `Tentar novamente` |

Dados da tabela: `Contrato Social` Aprovado · `Cartão CNPJ` Em Análise · `Certidão Negativa` Rejeitado · `Inscrição Estadual` Pendente · `Alvará` Vencido.

Ações por status: `Pendente → Enviar documento` · `Em Análise → Visualizar` · `Aprovado → Visualizar` · `Rejeitado → Ver motivo + Reenviar` · `Vencido → Enviar nova versão`.

### Upload

| ID | Tela |
|----|------|
| MAT-03a | Upload · inicial |
| MAT-03b | Upload · selecionado (`inscricao-estadual.pdf` · 842 KB · `Remover`) |
| MAT-03c | Upload · enviando |
| MAT-03d | Upload · sucesso |
| MAT-03e | Upload · erro (`Tentar novamente`) |

O limite de tamanho aparece como “conforme configuração do sistema”, sem número fixo.

### Detalhe, rejeição e reenvio

| ID | Tela | Conteúdo |
|----|------|----------|
| MAT-04a | Documento · Em Análise | Status, versão, envio, arquivo; `Substituir`/`Remover` desabilitados com aviso |
| MAT-04b | Documento · Aprovado | Status, versão, `Aprovado em`, arquivo (sem vencimento inventado) |
| MAT-04c | Documento · loading | Skeleton de detalhe + histórico |
| MAT-05a | Documento · Rejeitado | Bloco destacado `Motivo da rejeição`, data/analista, `Visualizar arquivo enviado` + `Reenviar documento` |
| MAT-05b | Reenvio (modal) | Aviso `Nova versão`, motivo anterior e UploadArea |
| MAT-05c | Histórico | v2 Em Análise + v1 Rejeitado, toast de nova versão |

Histórico: `Versão · Status · Enviado em · Analisado em · Ações`. O reenvio cria nova versão e nunca sobrescreve a anterior.

### Pendências e feedback

| ID | Tela | Conteúdo |
|----|------|----------|
| MAT-06a | Pendências | Documento não enviado, rejeitado (`Ver motivo`/`Resolver`) e vencido, com o destino de cada ação |
| MAT-06b | Pendências · vazio | “Nenhuma pendência encontrada” / “A documentação da empresa está regular no momento.” |
| MAT-06c | Pendências · loading | `Skeleton/Lista` |
| MAT-06d | Pendências · erro | “Não foi possível carregar as pendências.” + `Tentar novamente` |
| MAT-07 | Feedback global | Toasts de envio, nova versão, dados atualizados e erro, com o gatilho de cada um |

---

## 6. Componentes (`02b — Components v2`)

`DataTable` · `DocumentStatusBadge` (5 status por label + cor + forma) · `PaymentReceiptStatusBadge` · `UserAccessStatusBadge` (Convite pendente · Ativo · Suspenso · Convite expirado · Bloqueado) · `UploadArea` (5 estados) · `Modal` · `Drawer` · `Alert` (info/success/warning/error) · `Toast` (4) · `EmptyState` · `Skeleton/Tabela` · `Skeleton/Lista` · `Skeleton/Detalhe` · `FilterBar` · `Pagination`.

`Drawer` fica disponível como alternativa ao `Modal` para o módulo de funcionários de Mão de Obra.

---

## 7. Tokens

Variables criadas nesta fase, com espelho em `frontend/src/styles/tokens.css`:

| Figma Variable | CSS |
|----------------|-----|
| `color/badge/{neutral,info,success,warning,danger}-{bg,fg}` | `--color-badge-*` |
| `color/bg/sidebar` | `--color-bg-sidebar` |
| `color/text/on-sidebar` · `color/text/muted-on-sidebar` | `--color-text-on-sidebar` · `--color-text-muted-on-sidebar` |
| `color/bg/readonly` | `--color-bg-readonly` |

Semânticas em `Color` fazem alias de `Primitives`; `spacing/*` e `radius/*` alimentam padding, gap e cornerRadius das telas v3.

---

## 8. Validação documental Jotanunes — página `10`

Fecha o ciclo com o Materials Flow (`MAT-02`…`MAT-05`) e registra eventos em `JN-07 Auditoria` (RF09, RF10, RF12 · UC-JN-11…15).

| ID | Tela | Conteúdo |
|----|------|----------|
| JN-02 | Fila de validação | Resumo, FilterBar com **default `Status: Em Análise`**, tabela `Documento · Origem · Enviado em · Status · Ações` (`Analisar` / `Visualizar`), paginação |
| JN-02b | Fila · loading | Skeleton de header + tabela |
| JN-02c | Fila · erro | Falha ao carregar fila + `Tentar novamente` |
| JN-02d | Fila · vazio | Nenhum documento aguardando análise |
| JN-03a | Detalhe · Em Análise | Arquivo, fornecedor, histórico de versões, `Aprovar` / `Rejeitar` |
| JN-03b | Modal · Aprovar | Confirmação + registro na auditoria |
| JN-03c | Modal · Rejeitar | Motivo obrigatório (RF10) visível ao fornecedor |
| JN-03d | Detalhe · Aprovado | Status aprovado, analista, alerta de auditoria |
| JN-03e | Após rejeição | Motivo registrado + referência ao reenvio (`MAT-05b`) |
| JN-03f | Visualizador de documento | Referência principal |
| JN-03f1–f3 | Visualizador · carregado / loading / erro | Carregado: `Aprovar`/`Rejeitar`; loading/erro: ações **disabled**; erro: `Tentar novamente` + `Fechar visualizador` ativo |
| JN-03g | Detalhe · Documento de Funcionário | Contexto ASO / funcionário — **João Silva · Alpha Serviços** (dados fictícios unificados) |
| JN-04 | Feedback | Toasts do analista (aprovação/rejeição/erro/validação) |
| JN-05a–d | Pagamentos e comprovantes | DataTable `Empresa · Funcionário · Competência · Pagamento · Prazo · Envio · Situação · Ações` (`Visualizar`); empty/loading/erro; **anotação PENDENTE DE DECISÃO DE PRODUTO** (sem `Aprovar`/`Rejeitar` de comprovante) |
| JN-06 | Detalhe do comprovante | Consulta (sem aprovar/rejeitar comprovante) |

Dados de exemplo alinhados a **Alpha Materiais** (Certidão Negativa v2 após rejeição da v1).

---

## 9. Mão de Obra v1 — página `09`

Base: shell `06 — Shell Mão de Obra`. Resumo de `MO-02a` coerente com **3** linhas na tabela (não 12/8/3/1 do texto de exemplo do prompt).

| ID | Tela | Conteúdo |
|----|------|----------|
| MO-02a | Funcionários | Header, cards resumo, filtros, tabela (João / Carlos / Pedro) |
| MO-02b | Funcionários · vazio | Empty state |
| MO-02c | Funcionários · loading | Skeleton |
| MO-02d | Funcionários · erro | Falha + `Tentar novamente` |
| MO-03a | Cadastrar Funcionário | Formulário |
| MO-03b | Cadastro · salvando | Estado de envio |
| MO-03c | Cadastro · erro | Falha + retry |
| MO-03d | Cadastro · sucesso | Confirmação + `Ver funcionário` |
| MO-04a | Funcionário · Visão geral | Status documental vs autorização para atuar |
| MO-04b | Funcionário · Documentos | Exemplo: ASO (Em Análise) · Documento de identificação (Rejeitado) · Ficha de registro (Pendente); anotação: tipos definitivos parametrizáveis com produto/backend |
| MO-04c | Funcionário · Pendências | Não enviado / rejeitado |
| MO-04d | Funcionário · Obras / Vínculos | **Tela canônica** de vínculos (tabs do detalhe); exemplos `Vinculado + Bloqueado` vs `Vinculado + Autorizado` |
| MO-05a | Documento · Em Análise | Metadados, arquivo, histórico, bloqueio de edição |
| MO-05b | Documento · Rejeitado | Motivo, analista/data, reenvio único |
| MO-05c | Documento · Aprovado | Confirmação sem CTA de reenvio |
| MO-05d | Documento · Reenvio | **ASO · João Silva** (não documento empresarial); motivo/datas coerentes (ex.: analisado 14/09/2026 · Mariana Souza) |
| MO-06a | Obras / Vínculos · **LEGACY** | Referência antiga — usar `MO-04d`; banner `LEGACY — utilizar MO-04d…` |
| MO-06b1 | Vincular a obra · disponível | Funcionário + Select obra; `Cancelar` / `Vincular` habilitado |
| MO-06b2 | Vincular a obra · já vinculada | Obra já vinculada; mensagem de validação; `Vincular` disabled |
| MO-06g | Vínculo · sucesso | Toast `Funcionário vinculado à obra com sucesso.` |
| MO-06e | Vínculos · vazio | Empty state |
| MO-06f | Vínculos · erro | Falha + retry |
| MO-06c | Vínculo · bloqueado | Alerta documentação pendente |
| MO-06d | Vínculo · autorizado | Card regular + badge autorizado |
| MO-07 | Feedback | Sucesso/erro (cadastro, vínculo) |

### Pagamentos e comprovantes (v4 + correções finais)

| ID | Tela | Conteúdo |
|----|------|----------|
| MO-08a | Pagamentos | Header, cards (`3` pagamentos · pendentes/atraso/comprovante enviado alinhados à tabela), DataTable `Funcionário · Competência · Pagamento · Prazo · Envio · Situação · Ações`; anotação **PENDENTE DE DECISÃO DE PRODUTO** |
| MO-08b–d | Pagamentos · vazio / loading / erro | Empty copy fornecedor; `Skeleton/Tabela`; erro + `Tentar novamente` |
| MO-09a | Registrar pagamento | Form real (Select/Date/competência); prazo readonly (+3 dias **corridos**); comprovante **opcional** no registro |
| MO-09b–d | Registrar · enviando / sucesso / erro | `Salvando pagamento…`; CTAs disabled no envio |
| MO-09e | Enviar comprovante | Fluxo separado pós-registro (metadados + UploadArea PDF/imagem) |
| MO-10 | Detalhe pagamento/comprovante | Layout por campos (empresa, competência, obra, datas, situação, arquivo) |
| MO-11 | Pendências de comprovantes | Cards/lista; Pedro → `Enviar comprovante`; João enviado **sem** CTA de reenvio automático |

Prazo do comprovante: **data do pagamento + 3 dias corridos** (`MO-09a`, `MO-09e`, tabela). Exemplo **Pedro Lima**: `Pendente em atraso` (prazo 15/09/2026, referência após essa data).

**Personagem fictício unificado:** João Silva · Eletricista · CPF `***.***.***-12` · **Alpha Serviços** (nunca Beta Serviços como empregador dele).

Telas **LEGACY** (banner acima do frame): `T-52`, `T-53`, `T-60`, `T-61`, **`MO-06a`** → fluxos canônicos em `MO-04d`, `10 — Jotanunes Flow` e `09 — Mao de Obra Flow`.

---

## 10. Critérios de aceite

### Revisão 2026-09-15

Conforme [figma-proximos-passos-e-correcoes.md](./figma-proximos-passos-e-correcoes.md) §28:

| Área | Status |
|------|--------|
| Correções Materiais + JN (métricas MAT-02, upload global, MAT-03c/04a/05a, fila JN-02, copy aprovação, JN-03f/g, legacy T-52/53) | **Atendido no Figma** |
| Mão de Obra v1 (lista, cadastro, detalhe, docs, pendências, obras, bloqueio/autorizado, estados, feedback) | **Atendido no Figma** |

### Correções finais 2026-09-16

Conforme [figma-correcoes-finais.md](./figma-correcoes-finais.md) (checklist § Critérios de aceite — **34/34 verificados no arquivo Figma**):

| Área | Status |
|------|--------|
| Validação Jotanunes (filtro fila, PDF loading/erro, João Silva / Alpha Serviços) | **Atendido no Figma** |
| Funcionários (docs exemplo, reenvio ASO, datas, vínculo ≠ autorização) | **Atendido no Figma** |
| Obras (`MO-04d` canônico, `MO-06a` legacy, modais b1/b2, feedback sucesso) | **Atendido no Figma** |
| Pagamentos (DataTable, competência, prazo +3d, MO-09e, estados, badges) | **Atendido no Figma** |
| Decisão pendente (aprovação de comprovante) | **Explicitamente marcada** — não implementada no fluxo |

---

## 11. Usuários e Acessos — página `11` (Admin Jotanunes)

Spec: [gestao-de-usuarios.md](./gestao-de-usuarios.md). Termo: **Usuário da terceirizada** (≠ funcionário operacional MO).

| Seção | IDs | Conteúdo |
|-------|-----|----------|
| 01 — Listagem | `JN-08`, `JN-08b`–`d`, `JN-08x` | DataTable, filtros, cards 24/18/4/2; loading/empty/erro; **acesso negado** Analista |
| 02 — Cadastro | `JN-09a`–`e` | Form DS, validações, enviando, sucesso, erro convite |
| 03 — Detalhe | `JN-10a`, `JN-10b`–`e`, `JN-10d`, `JN-10f`, `JN-10g` | Ativo, editar, suspender, suspenso, reativar, convite pendente/expirado |
| 04 — Empresa | `JN-EMP-USR`, `JN-EMP-USR-EMPTY` | Aba administrativa no detalhe da empresa |
| 05 — Ativação | `AUTH-02`…`e` | Token, senha pelo usuário, expirado/inválido |

Nav **Usuários e acessos** inserida no shell Admin (após Empresas). Anotações de **regras backend** e auditoria (`JN-07`) no arquivo.

---

## 12. Próximo incremento

1. **Produto:** comprovantes (aprovação?) · evolução de perfis externos (`PENDENTE DE EVOLUÇÃO` em `JN-09a`).
2. **Implementação React + TypeScript:** módulo Usuários e Acessos (RBAC Admin vs Analista) + demais fluxos MO/JN.

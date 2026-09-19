# Requisitos funcionais — JotaNunesForms

**Versão:** 1.2  
**Data:** 2026-09-15  
**Fonte:** tabela de requisitos funcionais do produto (RF01–RF20)  
**Status:** documentado — telas Figma v1 em andamento; implementação em código pendente

## 1. Objetivo

O sistema deve permitir que a construtora gerencie **empresas terceirizadas**, **obras**, **trabalhadores**, **documentação obrigatória** (empresarial e de funcionários), **validação documental**, **comprovantes de pagamento** (com prazos e atrasos) e **consultas/dashboard** gerenciais.

Este documento é a base para:

1. Desenho das telas no Figma (design system JotaNunesForms).
2. Implementação no frontend (React) e backend (ASP.NET Core).

## 2. Atores e perfis

| Perfil | Papel no sistema |
|--------|------------------|
| **Administrador** | Gestão ampla (cadastros, obras, visão gerencial). |
| **Analista** | Validação documental (aprovar/rejeitar), consultas e acompanhamento de pendências. |
| **Terceirizado** | Empresa ou usuário da empresa: cadastros operacionais, uploads, reenvios e registros de pagamento/comprovantes. |

Perfis concretos e matriz de permissões por tela serão detalhados na especificação de autenticação (RF01) e nas specs de feature.

## 3. Glossário

| Termo | Significado |
|-------|-------------|
| Empresa terceirizada | Pessoa jurídica cadastrada para prestar serviço/fornecer material à construtora. |
| Fornecedora de Mão de Obra | Classificação de empresa que aloca trabalhadores. |
| Fornecedora de Materiais | Classificação de empresa focada em fornecimento de materiais. |
| Obra | Empreendimento/canteiro ativo da construtora. |
| Documento empresarial | Arquivo (PDF) obrigatório da empresa. |
| Documento de funcionário | Arquivo obrigatório individual do trabalhador. |
| Comprovante | Recibo/comprovante bancário do pagamento de salário. |

## 4. Status documentais (RF12)

O sistema deve apresentar e persistir, para documentos (e fluxos correlatos), pelo menos os status:

| Status | Uso típico |
|--------|------------|
| **Pendente** | Aguardando envio ou início de análise. |
| **Em Análise** | Em avaliação pelo analista. |
| **Aprovado** | Validado positivamente. |
| **Rejeitado** | Recusado; exige motivo (RF10) e permite reenvio (RF11). |
| **Vencido** | Fora da validade / prazo expirado. |

## 5. Agrupamento por domínio (orientação a telas)

Agrupamento sugerido para backlog de Figma e de implementação:

| Módulo | Requisitos | Telas / fluxos esperados (alto nível) |
|--------|------------|----------------------------------------|
| Acesso | RF01 | Login, sessão, redirecionamento por perfil |
| Empresas | RF02, RF03 | Listagem, formulário, classificação |
| Obras | RF04, RF06, RF19 | CRUD de obras; consulta de alocações (visão Jotanunes) |
| Funcionários (cadastro) | RF05, RF06 | **Somente** empresa de mão de obra cadastra/vincula seus trabalhadores |
| Conformidade (visão Jotanunes) | RF08, RF12–RF16, RF18, RF20 | Dashboard e consultas: trabalhadores enviados pelas parceiras, validade documental, pagamentos e comprovantes |
| Documentos | RF07–RF12, RF17 | Upload (terceirizado); validação (analista Jotanunes) |
| Pagamentos | RF13–RF16 | Registro/comprovante (terceirizado); acompanhamento e validade (Jotanunes) |
| Consultas / BI | RF18, RF19, RF20 | Pendências, consulta por obra, dashboard |

## 6. Catálogo de requisitos funcionais

| ID | Requisito funcional | Descrição detalhada do comportamento do sistema |
|----|---------------------|--------------------------------------------------|
| **RF01** | Autenticação | Permite login seguro para Administradores, Analistas e Terceirizados com perfis de acesso. Credenciais provisionadas pela Jotanunes (sem cadastro público). |
| **RF02** | Cadastro de Empresas | Permite o registro completo de empresas terceirizadas com Razão Social, CNPJ e Contatos. |
| **RF03** | Classificação de Empresa | Classifica a empresa entre Fornecedora de Mão de Obra ou Fornecedora de Materiais. |
| **RF04** | Cadastro de Obras | Permite a criação e gerenciamento das obras ativas da construtora. |
| **RF05** | Cadastro de Funcionários | Disponibiliza **às empresas de mão de obra** o cadastro dos trabalhadores que elas irão prover (Nome, CPF, Cargo). A equipe Jotanunes **não** cadastra esses funcionários; apenas consulta/acompanha. |
| **RF06** | Vinculação a Obras | Permite à empresa de MO vincular seus funcionários a uma ou mais obras. |
| **RF07** | Upload Doc. Empresariais | Permite o envio de arquivos digitais (PDF) dos documentos da empresa. |
| **RF08** | Upload Doc. Funcionários | Permite o upload de documentos individuais obrigatórios de cada trabalhador. |
| **RF09** | Validação Documental | Permite aos analistas da construtora aprovar ou rejeitar cada arquivo enviado. |
| **RF10** | Motivo de Rejeição | Exige a digitação do motivo explicativo ao rejeitar um documento. |
| **RF11** | Reenvio de Documentos | Permite ao terceirizado substituir e reenviar arquivos rejeitados. |
| **RF12** | Controle de Status | Apresenta status: Pendente, Em Análise, Aprovado, Rejeitado, Vencido. |
| **RF13** | Registro de Pagamentos | Permite registrar a data de pagamento de salários dos funcionários. |
| **RF14** | Upload de Comprovantes | Permite anexar o comprovante bancário/recibo do pagamento efetuado. |
| **RF15** | Cálculo Automatizado de Prazos | Calcula automaticamente a data limite de 3 dias pós-pagamento. |
| **RF16** | Identificação de Atrasos | Sinaliza automaticamente comprovantes não enviados dentro do prazo de 3 dias. |
| **RF17** | Histórico de Auditoria | Registra e mantém o histórico de todas as versões e alterações de documentos. |
| **RF18** | Consulta de Pendências | Permite à construtora listar empresas e trabalhadores em situação irregular. |
| **RF19** | Consulta por Obra | Permite visualizar a lista de terceirizados e trabalhadores alocados por obra. |
| **RF20** | Dashboard Gerencial | Painel da equipe Jotanunes: panorama de empresas regulares, pendências, alertas de atraso; visão dos trabalhadores enviados pelas parceiras, validade dos documentos e situação de pagamentos/comprovantes. |

## 7. Detalhamento por requisito

### RF01 — Autenticação

- **Atores:** Administrador, Analista, Terceirizado (empresas parceiras).
- **Comportamento:** login seguro com identificação de perfil e acesso conforme o perfil.
- **Provisionamento:** credenciais são **fornecidas pela Jotanunes** às empresas parceiras. **Não há** cadastro público, “Cadastre-se”, “Primeiro acesso” nem autoatendimento de recuperação de senha no portal.
- **Telas sugeridas:** Login (usuário/documento + senha + CTA Acessar).
- **Dependências:** base para todos os demais RF.

### RF02 — Cadastro de Empresas

- **Dados mínimos citados:** Razão Social, CNPJ, Contatos.
- **Telas sugeridas:** lista de empresas; formulário criar/editar; detalhe da empresa.

### RF03 — Classificação de Empresa

- **Valores:** Fornecedora de Mão de Obra **ou** Fornecedora de Materiais.
- **Telas sugeridas:** campo/etapa no cadastro ou edição de empresa; filtros por classificação.
- **Nota:** cadastro de funcionários (RF05) aplica-se às empresas de mão de obra.

### RF04 — Cadastro de Obras

- **Comportamento:** criar e gerenciar obras ativas da construtora.
- **Telas sugeridas:** lista de obras; formulário; detalhe / status ativo.

### RF05 — Cadastro de Funcionários

- **Atores principais:** empresas terceirizadas de **mão de obra** (Terceirizado).
- **Dados mínimos citados:** Nome, CPF, Cargo.
- **Regra de ownership:** quem cadastra é a empresa parceira que providencia a mão de obra — **não** há tela/menu de cadastro de funcionários para o funcionário/analista Jotanunes.
- **Visão Jotanunes:** consulta dos trabalhadores enviados (via dashboard, obra, empresa, pagamentos), com validade documental e conformidade de pagamento — sem CRUD de cadastro.
- **Telas sugeridas:** lista/formulário **apenas** no perfil Terceirizado (MO).

### RF06 — Vinculação a Obras

- **Atores:** empresa de mão de obra (vínculo); Jotanunes visualiza alocações na consulta por obra (RF19).
- **Comportamento:** vincular funcionário cadastrado pela parceira a uma ou mais obras.
- **Telas sugeridas:** seleção de obras no cadastro do funcionário (Terceirizado); listas no detalhe da obra (Jotanunes).

### RF07 — Upload Doc. Empresariais

- **Formato citado:** PDF.
- **Telas sugeridas:** área de documentos da empresa; upload; lista com status (RF12).

### RF08 — Upload Doc. Funcionários

- **Comportamento:** upload de documentos individuais obrigatórios por trabalhador.
- **Telas sugeridas:** documentos no detalhe do funcionário; checklist de obrigatórios.

### RF09 — Validação Documental

- **Atores:** Analistas da construtora.
- **Comportamento:** aprovar ou rejeitar cada arquivo enviado.
- **Telas sugeridas:** fila / inbox de análise; detalhe do documento com ações.

### RF10 — Motivo de Rejeição

- **Regra:** rejeição **exige** texto explicativo.
- **Telas sugeridas:** modal/formulário de rejeição com campo obrigatório de motivo.

### RF11 — Reenvio de Documentos

- **Atores:** Terceirizado.
- **Comportamento:** substituir e reenviar arquivos rejeitados.
- **Telas sugeridas:** ação de reenvio no documento rejeitado; nova versão (ver RF17).

### RF12 — Controle de Status

- **Status:** Pendente, Em Análise, Aprovado, Rejeitado, Vencido.
- **Telas sugeridas:** badges/filtros em listagens; regras de transição nas specs de documentos.

### RF13 — Registro de Pagamentos

- **Atores (registro):** Terceirizado (empresa de MO).
- **Comportamento:** registrar a data de pagamento de salários dos funcionários que a empresa providencia.
- **Visão Jotanunes:** acompanhar se a parceira pagou na data prevista e se enviou comprovantes válidos.
- **Telas sugeridas:** formulário de registro (Terceirizado); listagens/dashboard de conformidade (Jotanunes).

### RF14 — Upload de Comprovantes

- **Atores (upload):** Terceirizado.
- **Comportamento:** anexar comprovante bancário/recibo do pagamento para envio à Jotanunes.
- **Visão Jotanunes:** conferir se o comprovante foi enviado e se é válido (validação documental / status).
- **Telas sugeridas:** upload vinculado ao pagamento (Terceirizado); status na fila/validação e no dashboard (Jotanunes).

### RF15 — Cálculo Automatizado de Prazos

- **Regra:** data limite = **3 dias** após a data de pagamento.
- **Comportamento:** cálculo automático (sem digitação manual do prazo).

### RF16 — Identificação de Atrasos

- **Comportamento:** sinalizar automaticamente comprovantes não enviados dentro do prazo de 3 dias.
- **Telas sugeridas:** indicadores/alertas em listas, pendências (RF18) e dashboard (RF20).

### RF17 — Histórico de Auditoria

- **Comportamento:** manter histórico de versões e alterações de documentos.
- **Telas sugeridas:** timeline / histórico no detalhe do documento.

### RF18 — Consulta de Pendências

- **Atores:** construtora (Administrador / Analista).
- **Comportamento:** listar empresas e trabalhadores em situação irregular.
- **Telas sugeridas:** consulta de pendências com filtros.

### RF19 — Consulta por Obra

- **Comportamento:** visualizar terceirizados e trabalhadores alocados por obra.
- **Telas sugeridas:** detalhe da obra com abas/listas de empresas e trabalhadores.

### RF20 — Dashboard Gerencial

- **Atores:** Administrador / Analista (equipe Jotanunes).
- **Indicadores:** empresas regulares, pendências, alertas de atraso.
- **Conteúdo obrigatório da visão Jotanunes (não é um menu “Funcionários”):**
  1. Trabalhadores enviados / cadastrados pelas empresas de mão de obra parceiras.
  2. Validade / situação dos documentos desses trabalhadores.
  3. Se a terceirizada pagou na data prevista, enviou os comprovantes à Jotanunes e se esses comprovantes são válidos.
- **Telas sugeridas:** dashboard home pós-login (T-10); aprofundamento em Pendências (T-11), Validação (T-52), Pagamentos (T-60) e Obra (T-32).

## 8. Regras transversais destacadas

| ID | Regra |
|----|--------|
| R-AUTH | Credenciais de login são provisionadas pela Jotanunes; sem auto-cadastro no portal (RF01). |
| R-PDF | Uploads empresariais citados como PDF (RF07); demais formatos de comprovante a confirmar em spec. |
| R-3DIAS | Prazo de comprovante = 3 dias após pagamento (RF15/RF16). |
| R-REJ | Rejeição documental exige motivo (RF10). |
| R-MO | Cadastro de funcionários é exclusivo das empresas de mão de obra (RF03 + RF05). |
| R-JOTA-VIEW | Equipe Jotanunes **não** tem menu/tela de cadastro de funcionários; consulta trabalhadores das parceiras + validade documental + conformidade de pagamentos/comprovantes (RF20). |
| R-MULTI-OBRA | Funcionário pode vincular-se a uma ou mais obras (RF06). |

## 9. Rastreabilidade (próximas entregas)

| Entrega | Artefato esperado |
|---------|-------------------|
| UX/UI Figma | Fluxos e telas por módulo (§5), componentes do [Design System](https://www.figma.com/design/Pu4udm2alIAtdGScbIqLBN) |
| Specs Speckit | Uma feature (ou epic) por módulo, referenciando IDs RF |
| Código | Rotas/páginas React + endpoints API mapeados aos RF |

## 10. Fora de escopo deste documento

- Requisitos não funcionais (desempenho, segurança detalhada, LGPD, disponibilidade).
- Wireframes e protótipos (serão produzidos no Figma).
- Modelo de dados definitivo e contratos de API (virão nas specs de feature).

## 11. Histórico

| Data | Versão | Alteração |
|------|--------|-----------|
| 2026-09-15 | 1.0 | Inclusão do catálogo RF01–RF20 e detalhamento para Figma/código |
| 2026-09-15 | 1.2 | Visão Jotanunes: sem cadastro de funcionários; dashboard foca trabalhadores das parceiras, docs e pagamentos/comprovantes |

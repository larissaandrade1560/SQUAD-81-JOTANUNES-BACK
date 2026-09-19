# JotaNunesForms — Revisão do Figma + Correções + Próximas Etapas

## Revisão realizada

Foi realizada uma nova revisão do arquivo Figma do **JotaNunesForms**, incluindo:

- `07 — Shell Materiais`
- `08 — Materials Flow`
- `10 — Jotanunes Flow`
- telas legadas de validação (`T-52` / `T-53`)
- estrutura atual de `09 — Mao de Obra Flow`

## Situação atual

O avanço desde a última revisão foi significativo.

### Materials Flow

O fluxo de Materiais agora está praticamente completo e possui:

```text
MAT-01a Minha Empresa · visualização
MAT-01b Minha Empresa · edição

MAT-02 Documentos
MAT-02b Documentos · loading
MAT-02c Documentos · erro

MAT-03a Upload · inicial
MAT-03b Upload · selecionado
MAT-03c Upload · enviando
MAT-03d Upload · sucesso
MAT-03e Upload · erro

MAT-04a Documento · Em Análise
MAT-04b Documento · Aprovado
MAT-04c Documento · loading

MAT-05a Documento · Rejeitado
MAT-05b Reenvio
MAT-05c Histórico

MAT-06a Pendências
MAT-06b Pendências · vazio
MAT-06c Pendências · loading
MAT-06d Pendências · erro

MAT-07 Feedback global
```

A regressão anterior da sidebar de `MAT-01a` foi corrigida.

A mensagem:

```text
Documento aprovado pela Jotanunes.
Nenhuma ação é necessária para este documento.
```

também foi movida corretamente para `MAT-04b Documento · Aprovado`.

### Jotanunes Flow

O fluxo de validação documental da Jotanunes também já foi iniciado e está bem mais avançado do que na rodada anterior.

Existem atualmente:

```text
JN-02 Fila de validação
JN-02b Fila · loading
JN-02c Fila · erro
JN-02d Fila · vazio

JN-03a Detalhe · Em Análise
JN-03b Modal · Aprovar
JN-03c Modal · Rejeitar
JN-03d Detalhe · Aprovado
JN-03e Após rejeição

JN-04 Feedback
```

Portanto, a próxima rodada NÃO deve recriar o fluxo de Materiais nem a fila de validação da Jotanunes.

O foco agora deve ser:

1. corrigir inconsistências identificadas;
2. fechar alguns estados ainda ausentes na validação;
3. iniciar o fluxo funcional de **Mão de Obra — Funcionários + Documentação + Obras**.

---

# Correções necessárias

# 1. Corrigir métricas da tela `MAT-02 Documentos`

Na tela atual existem 5 documentos:

```text
Contrato Social      → Aprovado
Cartão CNPJ          → Em Análise
Certidão Negativa    → Rejeitado
Inscrição Estadual   → Pendente
Alvará               → Vencido
```

Portanto, os cards atuais:

```text
5 Documentos
2 Aprovados
1 Em análise
2 Pendências
```

não correspondem à tabela.

A própria tela de Pendências apresenta 3 itens:

```text
Certidão Negativa
Inscrição Estadual
Alvará
```

## Corrigir para

```text
5
Documentos

1
Aprovado

1
Em análise

3
Pendências
```

Considerar como `Pendências` neste resumo os documentos que exigem ação do fornecedor:

```text
Pendente
Rejeitado
Vencido
```

Manter a mesma definição entre:

- card de resumo;
- tabela;
- página Pendências.

---

# 2. Remover ambiguidade do botão global `Enviar documento`

Na tela `MAT-02 Documentos` existe:

```text
[Enviar documento]
```

no PageHeader.

Porém, a própria tabela já possui ações contextualizadas:

```text
Inscrição Estadual → Enviar documento
Alvará → Enviar nova versão
Certidão Negativa → Reenviar
```

Além disso, o modal atual de upload abre com:

```text
Documento
Inscrição Estadual
```

já pré-selecionado.

Isso torna o botão global ambíguo: não está definido qual documento será enviado.

## Para o MVP

REMOVER o botão global `Enviar documento` do header.

O upload deve partir da linha do documento correspondente.

Exemplo:

```text
Inscrição Estadual
[Pendente]
[Enviar documento]
```

Isso evita a necessidade de inventar um seletor de tipo documental.

Se futuramente existir upload de documentos adicionais não cadastrados previamente, tratar como outra funcionalidade.

---

# 3. Ajustar estado `MAT-03c Upload · enviando`

Durante o estado:

```text
Enviando documento...
```

o modal ainda apresenta:

- botão `Cancelar`;
- botão de fechar `✕`.

Isso pode sugerir que o upload pode ser interrompido, mesmo que nenhum comportamento de cancelamento tenha sido especificado.

## Corrigir

Durante o envio:

- desabilitar `Cancelar`;
- desabilitar `✕`;
- manter o CTA em estado loading;
- impedir envio duplicado.

Exemplo:

```text
Enviando documento...

[Cancelar — disabled]
[Enviando documento... — loading]
```

Não implementar "cancelamento de upload" no design sem que exista uma regra correspondente.

---

# 4. Simplificar `MAT-04a Documento · Em Análise`

Atualmente aparecem ações bloqueadas:

```text
Substituir arquivo
Remover versão
```

com a indicação:

```text
Indisponível durante a análise
```

A regra já está suficientemente explicada pelo Alert:

```text
Enquanto estiver em análise não é possível editar, substituir ou apagar a versão atual.
```

## Ajuste recomendado

REMOVER os dois botões bloqueados.

O estado Em Análise deve mostrar apenas ações possíveis:

```text
Visualizar arquivo
Voltar
```

Isso reduz ruído e evita apresentar ações que o usuário nunca poderá executar naquele estado.

---

# 5. Remover CTA duplicado em `MAT-05a Documento · Rejeitado`

A tela possui `Reenviar documento`:

- no PageHeader;
- dentro do card de motivo da rejeição.

Manter apenas um CTA principal.

## Manter

Dentro do card:

```text
[Visualizar arquivo enviado]
[Reenviar documento]
```

## Remover

O `Reenviar documento` do PageHeader.

No PageHeader manter somente:

```text
[Voltar]
```

A ação fica próxima da justificativa, que é o contexto necessário antes do reenvio.

---

# 6. Corrigir comportamento inicial da `JN-02 Fila de validação`

A página é descrita como:

```text
Documentos enviados por fornecedores aguardando análise da Jotanunes.
```

Porém, o filtro inicial está:

```text
Status: todos
```

e a tabela mostra inclusive:

```text
Contrato Social → Aprovado
```

Isso conflita com o conceito de fila de documentos aguardando análise.

## Corrigir o estado inicial

Filtro default:

```text
Status: Em Análise
```

Mostrar inicialmente somente documentos:

```text
Em Análise
```

Exemplo inicial:

```text
Cartão CNPJ
Certidão Negativa v2
ASO
```

Documentos aprovados ou rejeitados podem continuar acessíveis quando o usuário alterar explicitamente o filtro.

Exemplo:

```text
Status:
- Em Análise
- Aprovado
- Rejeitado
- Todos
```

---

# 7. Ajustar copy do modal `JN-03b Modal · Aprovar`

O modal atualmente informa algo equivalente a:

```text
A aprovação será registrada na auditoria e o fornecedor será notificado.
```

Para o MVP, não afirmar notificação automática de aprovação se esse comportamento não estiver formalmente definido.

## Alterar para

```text
A aprovação será registrada na auditoria e o status do documento será atualizado para Aprovado.
```

O fornecedor perceberá a mudança de estado no próprio fluxo de Documentos.

Se notificação ativa por e-mail/in-app for implementada futuramente, tratá-la em feature própria.

---

# 8. Criar visualização real do arquivo para o Analista

O fluxo de validação possui:

```text
[Visualizar arquivo]
```

mas ainda não existe uma especificação visual clara de como o analista realmente lê o PDF.

Isso é necessário para que a análise possa ser implementada sem inventar UX durante o desenvolvimento.

Criar:

`JN-03f Visualizador de documento`

## Estrutura recomendada

Desktop.

Layout dividido:

```text
┌───────────────────────────┬────────────────────┐
│                           │ Documento           │
│                           │ Status              │
│       PDF PREVIEW         │ Empresa/Funcionário │
│                           │ Versão              │
│                           │ Enviado em          │
│                           │                    │
│                           │ [Rejeitar]          │
│                           │ [Aprovar]           │
└───────────────────────────┴────────────────────┘
```

## Área PDF

Representar:

- página atual;
- total de páginas;
- zoom;
- próxima/anterior;
- download, se a aplicação permitir;
- loading do arquivo;
- erro de carregamento.

Não desenhar um editor de PDF.

O analista precisa apenas consultar o documento.

---

# 9. Criar variante de detalhe para documento de Funcionário

A fila já contém:

```text
ASO
João Silva · Beta Serviços · Funcionário
```

Porém, o detalhe atual foi desenhado somente para documento empresarial.

Criar:

`JN-03g Detalhe · Documento de Funcionário`

## Exibir

```text
ASO

Funcionário
João Silva

Empresa
Beta Serviços

Cargo
[valor do cadastro]

Documento
ASO

Status
Em Análise

Enviado em
14/09/2026 às 16:40
```

Se o funcionário estiver vinculado a obras, mostrar:

```text
Obras vinculadas
```

mas NÃO inventar vínculo quando os dados não existirem.

## Ações

```text
Visualizar arquivo
Rejeitar
Aprovar documento
```

Utilizar os mesmos modais de aprovação e rejeição.

---

# 10. Marcar telas antigas de validação como legado

Ainda existem páginas antigas:

```text
T-52 Validação
T-53 Rejeitar Modal
```

Essas telas possuem uma estrutura anterior e podem gerar ambiguidade para quem implementar o frontend.

## Não excluir necessariamente

Adicionar uma anotação clara:

```text
LEGACY / NÃO IMPLEMENTAR

Fluxo substituído pelos frames da página:
10 — Jotanunes Flow
```

Se possível, mover para uma seção visual:

```text
Legacy
```

O desenvolvimento deve utilizar:

```text
JN-02...
JN-03...
JN-04...
```

como referência atual.

---

# Próxima etapa principal

Depois das correções acima, iniciar:

# Mão de Obra Flow v1 — Funcionários + Documentação + Obras

A página já existe:

`09 — Mao de Obra Flow`

mas atualmente contém apenas um stub.

NÃO implementar Pagamentos nesta rodada.

O objetivo primeiro é fechar:

```text
Empresa de Mão de Obra
        ↓
Funcionários
        ↓
Cadastrar funcionário
        ↓
Detalhes do funcionário
        ↓
Documentação
        ↓
Vínculo com obra
        ↓
Situação de autorização
```

Somente depois desse ciclo estar concluído, fazer:

```text
Pagamentos
→ Comprovantes
→ Prazo de 3 dias
→ Atrasos
```

---

# PROMPT — Mão de Obra Flow v1

## Contexto

Continuar o JotaNunesForms utilizando:

- Design System existente;
- AppShell de Mão de Obra existente;
- componentes de documentos criados no Materials Flow;
- mesmos tokens;
- mesmos badges;
- DataTable;
- UploadArea;
- EmptyState;
- ErrorState;
- Skeleton;
- Toast.

NÃO recriar esses componentes.

O fornecedor de mão de obra possui navegação:

```text
Dashboard
Minha Empresa
Funcionários
Obras / Vínculos
Documentos
Pagamentos
Pendências
```

Nesta etapa, focar apenas em:

```text
Funcionários
Documentação do funcionário
Obras / Vínculos
Pendências relacionadas ao funcionário
```

---

# 11. Criar `MO-02 Funcionários`

Usar o AppShell existente de Mão de Obra.

Sidebar ativa:

`Funcionários`

## Header

```text
Funcionários

Cadastre e acompanhe os trabalhadores vinculados à empresa.
```

CTA:

```text
[Cadastrar funcionário]
```

---

## Resumo

Cards compactos:

```text
12
Funcionários

8
Documentação regular

3
Com pendências

1
Aguardando análise
```

Os números devem ser coerentes com a tabela criada.

---

## Filtros

Criar:

```text
Buscar por nome ou CPF

Status documental:
Todos
Regular
Com pendências
Em análise
```

Não adicionar filtros que não possuam utilidade clara.

---

## Tabela

| Funcionário | CPF | Cargo | Documentação | Obras | Ações |
|---|---|---|---|---|---|

Exemplos fictícios:

```text
João Silva
***.***.***-12
Eletricista
Em análise
1 obra
[Visualizar]
```

```text
Carlos Santos
***.***.***-34
Pedreiro
Regular
2 obras
[Visualizar]
```

```text
Pedro Almeida
***.***.***-56
Servente
Com pendências
Nenhuma obra
[Visualizar]
```

Utilizar CPF mascarado na listagem.

---

# 12. Estados da lista de Funcionários

Criar:

```text
MO-02a Funcionários
MO-02b Funcionários · vazio
MO-02c Funcionários · loading
MO-02d Funcionários · erro
```

## Empty State

```text
Nenhum funcionário cadastrado

Cadastre o primeiro funcionário para iniciar a gestão documental e os vínculos com obras.

[Cadastrar funcionário]
```

---

# 13. Criar `MO-03 Cadastrar Funcionário`

Campos obrigatórios do modelo atual:

```text
Nome
CPF
Cargo
```

## Formulário

```text
Nome completo *
CPF *
Cargo
```

Ações:

```text
Cancelar
Cadastrar funcionário
```

Estados:

- inicial;
- validação de campos;
- salvando;
- sucesso;
- erro.

Não adicionar informações pessoais que ainda não estejam previstas nos requisitos.

---

# 14. Criar `MO-04 Detalhe do Funcionário`

Exemplo:

```text
João Silva

CPF
***.***.***-12

Cargo
Eletricista

Empresa
Beta Serviços
```

Criar navegação interna por abas ou seções:

```text
Visão geral
Documentos
Obras / Vínculos
```

Manter uma única página de funcionário como referência principal.

---

# 15. Status do Funcionário

Separar duas informações diferentes:

## Status documental

Exemplos:

```text
Regular
Com pendências
Em análise
```

## Autorização para atuar

Exemplos:

```text
Autorizado
Bloqueado por pendências documentais
```

NÃO tratar esses conceitos como a mesma coisa.

Um funcionário pode estar vinculado a uma obra, mas a liberação para atuação deve refletir sua situação documental.

---

# 16. Criar seção `Documentos do Funcionário`

Reutilizar:

`DocumentStatusBadge`

Tabela:

| Documento | Status | Enviado em | Ações |
|---|---|---|---|

Exemplo visual:

```text
ASO
Em Análise
14/09/2026
[Visualizar]
```

```text
Documento obrigatório
Pendente
—
[Enviar documento]
```

```text
Documento obrigatório
Rejeitado
13/09/2026
[Ver motivo] [Reenviar]
```

## Importante

Não definir uma lista oficial de tipos documentais que ainda não tenha sido acordada.

O Figma pode utilizar `ASO` como exemplo já presente no projeto, mas outros itens devem ser tratados como exemplos ou tipos parametrizáveis.

---

# 17. Upload de documento de Funcionário

REUTILIZAR o fluxo de upload criado em Materiais.

Não criar outro componente UploadArea.

A diferença é apenas o contexto.

Exemplo:

```text
Enviar documento

Funcionário
João Silva

Documento
ASO
```

Demais estados:

```text
Inicial
Selecionado
Enviando
Sucesso
Erro
```

devem usar o mesmo padrão do Materials Flow.

---

# 18. Criar `MO-05 Pendências do Funcionário`

Dentro do detalhe do funcionário, apresentar:

```text
Pendências documentais
```

Exemplo:

```text
Documento obrigatório não enviado
[Enviar]
```

```text
Documento rejeitado
[Ver motivo] [Reenviar]
```

Caso não existam:

```text
Nenhuma pendência documental.
```

---

# 19. Criar `MO-06 Obras / Vínculos`

O funcionário pode estar vinculado a uma ou mais obras.

Dentro do detalhe, mostrar:

| Obra | Situação do vínculo | Autorização |
|---|---|---|

Exemplo:

```text
Residencial Jardins
Vinculado
Bloqueado — documentação pendente
```

ou:

```text
Edifício Central
Vinculado
Autorizado
```

---

# 20. Criar vínculo com obra

CTA:

```text
[Vincular a obra]
```

Abrir Modal ou Drawer:

```text
Vincular funcionário a obra

Funcionário
João Silva

Obra
[Selecionar obra]
```

Ações:

```text
Cancelar
Vincular
```

---

# 21. Regra visual de autorização

O vínculo com obra e a autorização para atuar são conceitos diferentes.

Fluxo:

```text
Funcionário vinculado à obra
        ↓
Documentação obrigatória aprovada?
        ↓
 ┌──────┴──────┐
 Não           Sim
 ↓              ↓
Bloqueado    Autorizado
```

Se existir documentação pendente:

```text
Bloqueado para atuação

Existem documentos obrigatórios que ainda não foram aprovados.
```

CTA:

```text
[Ver pendências]
```

Não representar o funcionário como autorizado enquanto existirem pendências obrigatórias.

---

# 22. Estado regular

Quando todos os documentos necessários estiverem aprovados:

```text
Documentação regular

O funcionário está apto para autorização nas obras vinculadas.
```

Badge:

```text
Regular
```

Nas obras aplicáveis:

```text
Autorizado
```

---

# 23. Pendências gerais do fornecedor de Mão de Obra

A tela principal `Pendências` deverá futuramente agregar:

- pendências empresariais;
- pendências de funcionários;
- comprovantes.

Nesta etapa, criar apenas a parte documental.

Exemplo:

```text
João Silva
ASO aguardando análise

[Ver funcionário]
```

```text
Pedro Almeida
Documento obrigatório pendente

[Resolver]
```

---

# 24. Componentes adicionais

Criar apenas se necessário:

```text
EmployeeStatusBadge
AuthorizationBadge
EmployeeSummaryCard
WorkLinkRow
DocumentProgress
```

Antes de criar qualquer novo componente, verificar se um componente atual pode ser reutilizado.

---

# 25. Estados obrigatórios

Para Funcionários e Vínculos criar:

- loading;
- empty;
- error;
- success.

Não é necessário criar telas separadas para cada pequena ação quando um componente/variant resolver o estado.

---

# 26. Organização de frames

Organizar `09 — Mao de Obra Flow` aproximadamente assim:

```text
MO-02a Funcionários
MO-02b Funcionários · vazio
MO-02c Funcionários · loading
MO-02d Funcionários · erro

MO-03a Cadastrar Funcionário
MO-03b Cadastro · salvando
MO-03c Cadastro · erro

MO-04a Funcionário · Visão geral
MO-04b Funcionário · Documentos
MO-04c Funcionário · Pendências

MO-05a Documento · Em Análise
MO-05b Documento · Rejeitado

MO-06a Obras / Vínculos
MO-06b Vincular a obra
MO-06c Vínculo · bloqueado
MO-06d Vínculo · autorizado

MO-07 Feedback
```

---

# 27. O que NÃO fazer nesta etapa

NÃO detalhar ainda:

```text
Registro de pagamento
Upload de comprovante salarial
Contagem regressiva de 3 dias
Comprovante enviado no prazo
Comprovante enviado com atraso
```

Esses itens pertencem ao próximo incremento:

# Mão de Obra Flow v2 — Pagamentos e Comprovantes

---

# 28. Critérios de aceite

## Correções

- [x] métricas de `MAT-02` corrigidas para refletir a tabela;
- [x] resumo de Pendências coerente com `MAT-06`;
- [x] botão global ambíguo de upload removido;
- [x] estado enviando bloqueia ações incompatíveis;
- [x] ações impossíveis removidas de `MAT-04a`;
- [x] CTA duplicado de reenvio removido;
- [x] fila Jotanunes inicia com `Em Análise`;
- [x] aprovados/rejeitados acessíveis apenas por mudança de filtro;
- [x] copy de aprovação não promete notificação não definida;
- [x] visualizador de PDF especificado;
- [x] detalhe de documento de funcionário criado;
- [x] telas T-52/T-53 marcadas como legado.

## Mão de Obra

- [x] lista de funcionários criada;
- [x] cadastro de funcionário criado;
- [x] detalhe do funcionário criado;
- [x] status documental separado de autorização;
- [x] documentos do funcionário utilizam componentes existentes;
- [x] upload reutiliza Materials Flow;
- [x] pendências documentais representadas;
- [x] vínculo com uma ou mais obras representado;
- [x] bloqueio por documentação pendente representado;
- [x] estado autorizado representado;
- [x] loading/empty/error criados;
- [x] tokens e componentes existentes preservados;
- [x] fluxo pronto para implementação em React + TypeScript.

---

# Etapa posterior

Depois de concluir este incremento, revisar novamente o Figma.

Se Funcionários + Documentação + Obras estiverem consistentes, o próximo prompt deverá detalhar:

# Mão de Obra Flow v2 — Pagamentos e Comprovantes

Fluxo futuro:

```text
Funcionário
    ↓
Registrar data de pagamento
    ↓
Sistema calcula data limite
    ↓
Prazo = pagamento + 3 dias corridos
    ↓
Enviar comprovante
    ↓
 ┌───────────────┴───────────────┐
Dentro do prazo              Após o prazo
↓                              ↓
Enviado no prazo           Enviado com atraso
```

Também deverá contemplar:

```text
Pendente em atraso
Validação pela Jotanunes
Histórico
Pendências
```

# JotaNunesForms — Correções finais de consistência + refinamento de Pagamentos e Comprovantes

## Contexto

Continuar trabalhando no arquivo Figma existente do **JotaNunesForms**.

O projeto já possui fluxos avançados para:

- Fornecedor de Materiais;
- Validação documental da Jotanunes;
- Funcionários de empresas de Mão de Obra;
- Documentação de funcionários;
- Vínculos com obras;
- início do fluxo de Pagamentos e Comprovantes.

Esta rodada NÃO deve recriar o Design System nem redesenhar fluxos já consolidados.

O objetivo agora é:

1. corrigir inconsistências encontradas entre os fluxos;
2. consolidar telas redundantes;
3. padronizar exemplos e dados fictícios;
4. refinar Pagamentos e Comprovantes;
5. deixar explícitas as decisões de negócio ainda pendentes;
6. deixar os fluxos prontos para implementação em React + TypeScript.

---

# Regras gerais

Preservar:

- identidade visual atual;
- sidebar;
- AppShells;
- tokens;
- Figma Variables;
- componentes existentes;
- Materials Flow;
- estrutura da Validação Jotanunes;
- estrutura de Funcionários;
- componentes compartilhados já criados.

Não criar um segundo componente quando já existir equivalente.

Priorizar reutilização de:

```text
Button
Input
Select
FormField
DataTable
DocumentStatusBadge
EmployeeStatusBadge
AuthorizationBadge
UploadArea
Alert
Toast
EmptyState
Skeleton
Pagination
FilterBar
Modal
Drawer
```

---

# PARTE 1 — Corrigir Fila de Validação da Jotanunes

## Problema atual

A tela:

`JN-02 Fila de validação`

é descrita como:

> Documentos enviados por fornecedores aguardando análise da Jotanunes.

A tabela apresentada contém documentos em:

`Em Análise`

Porém o filtro inicial aparece como:

```text
Status: todos
```

Isso cria inconsistência entre conceito e estado inicial da tela.

---

## Correção

Alterar o estado default para:

```text
Status: Em Análise
```

Opções do filtro:

```text
Em Análise
Aprovado
Rejeitado
Todos
```

No estado inicial mostrar apenas documentos aguardando análise.

Exemplo:

```text
Cartão CNPJ
Em Análise

Certidão Negativa · v2
Em Análise

ASO · João Silva
Em Análise
```

Documentos aprovados e rejeitados só devem aparecer quando o usuário mudar explicitamente o filtro.

---

# PARTE 2 — Corrigir estados do Visualizador de PDF

Existem:

```text
JN-03f1 Visualizador · carregado
JN-03f2 Visualizador · loading
JN-03f3 Visualizador · erro
```

## Estado carregado

Manter:

```text
Rejeitar
Aprovar documento
```

---

## Estado loading

Enquanto o documento estiver carregando:

```text
Carregando documento...
```

as ações:

```text
Rejeitar
Aprovar documento
```

NÃO devem estar habilitadas.

Podem:

- ficar disabled;
- ou desaparecer temporariamente.

Preferir disabled para preservar o layout.

Não permitir decisão antes que o arquivo possa ser visualizado.

---

## Estado de erro

Quando ocorrer:

```text
Não foi possível carregar o documento.
```

mostrar ação:

```text
[Tentar novamente]
```

As ações:

```text
Rejeitar
Aprovar documento
```

devem permanecer disabled.

Manter:

```text
Fechar visualizador
```

funcional.

---

# PARTE 3 — Padronizar dados fictícios

Existem inconsistências entre telas usando o mesmo personagem.

## João Silva

Padronizar em todo o projeto:

```text
Funcionário:
João Silva

Empresa:
Alpha Serviços

Cargo:
Eletricista

CPF:
***.***.***-12
```

NÃO utilizar João Silva como funcionário de `Beta Serviços` em outras telas.

Atualizar especialmente:

`JN-03g Detalhe · Documento de Funcionário`

onde atualmente aparece:

```text
Beta Serviços
```

Alterar para:

```text
Alpha Serviços
```

---

# PARTE 4 — Corrigir documentos duplicados do funcionário

Na tela:

`MO-04b Funcionário · Documentos`

existem duas linhas chamadas:

```text
ASO
```

com estados diferentes.

Isso gera ambiguidade.

## Ajustar exemplos para

```text
ASO
Em Análise
```

```text
Documento de identificação
Rejeitado
```

```text
Ficha de registro
Pendente
```

Importante:

Os nomes adicionais são apenas exemplos visuais.

Adicionar anotação:

```text
Tipos documentais definitivos devem ser parametrizados/validados com produto e backend.
```

Não transformar essa lista fictícia em regra fixa do sistema.

---

# PARTE 5 — Corrigir fluxo de reenvio do funcionário

O frame:

`MO-05d Documento · Reenvio`

atualmente utiliza conteúdo de documento empresarial:

```text
Certidão Negativa
```

Isso está incorreto no contexto de documento do funcionário.

## Alterar para

```text
Reenviar documento

ASO · João Silva

Versão atual
v1 · Rejeitado
```

Motivo anterior:

```text
Documento ilegível.
Envie uma nova versão legível do arquivo.
```

Aviso:

```text
Nova versão

Você está enviando uma nova versão do documento de João Silva.
A versão anterior permanecerá disponível no histórico.
```

Reutilizar:

`UploadArea`

---

# PARTE 6 — Corrigir datas do documento rejeitado

No fluxo atual existem datas conflitantes para a análise do documento.

Padronizar o exemplo.

Utilizar:

```text
Enviado em:
12/09/2026

Analisado em:
14/09/2026

Analista:
Mariana Souza
```

Se for mostrar histórico:

```text
v2
Rejeitado
Enviado: 12/09/2026
Analisado: 14/09/2026
```

Evitar textos internos com datas diferentes para o mesmo evento.

---

# PARTE 7 — Consolidar Obras / Vínculos

Existem telas muito semelhantes:

```text
MO-06a Obras / Vínculos
MO-04d Funcionário · Obras / Vínculos
```

Utilizar como tela canônica:

```text
MO-04d Funcionário · Obras / Vínculos
```

porque ela faz parte naturalmente das abas do detalhe do funcionário:

```text
Visão geral
Documentos
Obras / Vínculos
Pendências
```

---

## MO-06a

Não utilizar como página paralela independente.

Pode:

- ser removida;
- ou marcada como `LEGACY / referência`.

Adicionar anotação:

```text
LEGACY — utilizar MO-04d como tela principal de vínculos do funcionário.
```

---

# PARTE 8 — Refinar modal de vinculação com obra

O frame:

`MO-06b Vincular a obra`

atualmente demonstra apenas uma obra já vinculada:

```text
Residencial Jardins
```

com ação desabilitada.

Isso é útil como estado de validação, mas falta o fluxo normal.

Criar duas variantes.

---

## MO-06b1 Vincular a obra · disponível

```text
Vincular funcionário a obra

Funcionário
João Silva

Obra
[Selecionar obra]
```

Exemplo selecionado:

```text
Edifício Central
```

Ações:

```text
Cancelar
Vincular
```

Botão `Vincular` habilitado.

---

## MO-06b2 Vincular a obra · já vinculada

Exemplo:

```text
Residencial Jardins
```

Mensagem:

```text
Este funcionário já está vinculado a esta obra.
```

Botão:

```text
Vincular
```

disabled.

---

## Sucesso

Criar feedback:

```text
Funcionário vinculado à obra com sucesso.
```

---

# PARTE 9 — Manter separação entre vínculo e autorização

Preservar a regra visual:

```text
Vinculado
≠
Autorizado
```

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

Exemplos:

```text
Residencial Jardins
Vinculado
Bloqueado — documentação pendente
```

e:

```text
Edifício Central
Vinculado
Autorizado
```

---

# PARTE 10 — Refinar tela MO-08 Pagamentos

A tela atual está funcionalmente correta, mas visualmente ainda está abaixo do padrão do restante do sistema.

Refazer utilizando os mesmos componentes de alta fidelidade.

## Header

```text
Pagamentos e comprovantes

Registre pagamentos e acompanhe o prazo de envio dos comprovantes.
```

CTA:

```text
[Registrar pagamento]
```

---

## Resumo

Criar cards consistentes:

```text
3
Pagamentos

1
Pendente no prazo

1
Pendente em atraso

1
Comprovante enviado
```

Os valores devem corresponder à tabela.

Não utilizar métricas contraditórias com os dados exibidos.

---

# PARTE 11 — Corrigir regra temporal do exemplo

Existe atualmente:

```text
Pedro Lima

Pagamento:
12/09/2026

Prazo:
15/09/2026

Comprovante:
não enviado

Status:
Pendente
```

Considerando a data de referência após 15/09, isso deve ser:

```text
Pendente em atraso
```

Corrigir a tabela e os cards correspondentes.

---

# PARTE 12 — Estruturar DataTable de Pagamentos

Substituir linhas corridas de texto por DataTable real.

Colunas:

| Funcionário | Competência | Pagamento | Prazo | Envio | Situação | Ações |
|---|---|---|---|---|---|---|

Exemplos:

### Maria Souza

```text
Competência:
09/2026

Pagamento:
10/09/2026

Prazo:
13/09/2026

Enviado:
12/09/2026

Situação:
Enviado no prazo

Ação:
Visualizar
```

### João Silva

```text
Competência:
09/2026

Pagamento:
08/09/2026

Prazo:
11/09/2026

Enviado:
13/09/2026

Situação:
Enviado com atraso

Ação:
Visualizar
```

### Pedro Lima

```text
Competência:
09/2026

Pagamento:
12/09/2026

Prazo:
15/09/2026

Enviado:
—

Situação:
Pendente em atraso

Ação:
Enviar comprovante
```

---

# PARTE 13 — Adicionar Competência / Período

A regra de negócio prevê comprovantes vinculados ao funcionário e período.

Adicionar ao fluxo:

```text
Competência
```

Exemplo:

```text
09/2026
```

---

# PARTE 14 — Refinar MO-09 Registrar pagamento

Transformar placeholders em campos reais do Design System.

Não utilizar textos como:

```text
Funcionário: [Selecionar]
Data do pagamento: [date]
```

Utilizar componentes reais.

## Formulário

### Funcionário

```text
Funcionário *

[Select]
```

Exemplo:

```text
João Silva
```

---

### Competência

```text
Competência *

[MM/AAAA]
```

Exemplo:

```text
09/2026
```

---

### Data do pagamento

```text
Data do pagamento *

[Date Input]
```

Exemplo:

```text
08/09/2026
```

---

### Prazo do comprovante

Somente leitura:

```text
Prazo do comprovante

11/09/2026
```

Helper text:

```text
Calculado automaticamente: data do pagamento + 3 dias corridos.
```

---

# PARTE 15 — Separar registro do pagamento e envio do comprovante

Não exigir necessariamente o comprovante no mesmo instante do registro do pagamento.

O fluxo conceitual deve permitir:

```text
Registrar pagamento
       ↓
Pagamento criado
       ↓
Prazo calculado
       ↓
Comprovante pendente
       ↓
Enviar comprovante posteriormente
```

Portanto, o formulário deve permitir:

```text
Salvar pagamento
```

sem obrigar upload imediato.

Se houver upload opcional no mesmo fluxo, marcar:

```text
Comprovante — opcional neste momento
```

---

# PARTE 16 — Criar envio de comprovante separado

Criar:

`MO-09e Enviar comprovante`

Conteúdo:

```text
Enviar comprovante

Funcionário
João Silva

Competência
09/2026

Pagamento
08/09/2026

Prazo
11/09/2026
```

Upload:

```text
Arraste o comprovante aqui
ou
[Selecionar arquivo]
```

Formatos:

```text
PDF ou imagem
```

Ações:

```text
Cancelar
Enviar comprovante
```

---

# PARTE 17 — Estados de comprovante

Representar:

```text
Pendente no prazo
Pendente em atraso
Enviado no prazo
Enviado com atraso
```

Criar:

`PaymentReceiptStatusBadge`

ou reutilizar componente equivalente se já existir.

Não depender somente da cor.

---

# PARTE 18 — Detalhe do pagamento / comprovante

Refinar:

`MO-10 Detalhe do pagamento / comprovante`

Utilizar layout real.

Exibir:

```text
Pagamento — João Silva

Empresa
Alpha Serviços

Competência
09/2026

Obra
Residencial Jardins

Data do pagamento
08/09/2026

Prazo
11/09/2026

Data de envio
13/09/2026

Situação
Enviado com atraso
```

Arquivo:

```text
comprovante-joao.pdf
```

Ação:

```text
[Visualizar comprovante]
```

---

# PARTE 19 — Pendências de comprovantes

Refinar:

`MO-11 Pendências de comprovantes`

Não utilizar apenas texto corrido.

Criar lista/cards ou tabela.

Exemplo:

## Pedro Lima

```text
Competência
09/2026

Prazo
15/09/2026

Status
Pendente em atraso

[Enviar comprovante]
```

---

## João Silva

Se já houve envio:

```text
Enviado com atraso
```

não apresentar:

```text
Reenviar comprovante
```

automaticamente.

Só permitir reenvio se existir alguma regra de rejeição ou substituição definida.

Não inventar esse comportamento.

---

# PARTE 20 — Decisão de negócio pendente: validação de comprovantes

Existe uma questão ainda não definida:

> Os comprovantes de pagamento precisam ser formalmente aprovados/rejeitados pela Jotanunes ou a Jotanunes apenas consulta e acompanha o prazo?

NÃO escolher uma das opções sem definição de produto.

Adicionar anotação visível no Figma:

```text
PENDENTE DE DECISÃO DE PRODUTO

Definir se comprovantes possuem workflow de:

Aprovar / Rejeitar

ou apenas:

Recebido / prazo / auditoria.
```

---

# PARTE 21 — Jotanunes Pagamentos

A página atual:

`JN-05a Pagamentos e comprovantes`

deve ser refinada apenas até o ponto que não dependa da decisão anterior.

Criar tabela:

| Empresa | Funcionário | Competência | Pagamento | Prazo | Envio | Situação | Ações |
|---|---|---|---|---|---|---|---|

Ação segura:

```text
Visualizar
```

NÃO adicionar:

```text
Aprovar
Rejeitar
```

até a decisão de produto ser confirmada.

---

# PARTE 22 — Loading / Empty / Error de Pagamentos

Substituir estados simplificados atuais por componentes consistentes.

## Empty

```text
Nenhum pagamento registrado.

Os pagamentos cadastrados pelos fornecedores aparecerão aqui.
```

## Loading

Utilizar:

`Skeleton/Tabela`

## Error

```text
Não foi possível carregar os pagamentos.

[Tentar novamente]
```

---

# PARTE 23 — Corrigir estados de salvamento

No estado:

`MO-03b Cadastro · salvando`

o botão continua visualmente semelhante a um botão normal.

Representar:

```text
Cadastrando...
```

disabled/loading.

O mesmo vale para:

```text
MO-09b Registrar pagamento · enviando
```

Durante submissão:

- desabilitar cancelar;
- desabilitar fechamento;
- evitar dupla submissão;
- mostrar CTA loading.

---

# PARTE 24 — Organização

Organizar as seções atuais claramente:

```text
09 — Mao de Obra Flow

01 — Funcionários
02 — Cadastro
03 — Detalhe
04 — Documentação
05 — Obras / Vínculos
06 — Pagamentos
07 — Comprovantes
08 — Pendências
09 — Feedback
```

Telas redundantes devem receber label:

```text
LEGACY
```

ou serem removidas quando seguro.

---

# Critérios de aceite

## Validação Jotanunes

- [ ] filtro default é `Em Análise`;
- [ ] loading do PDF bloqueia decisão;
- [ ] erro do PDF possui `Tentar novamente`;
- [ ] Aprovar/Rejeitar ficam disabled quando arquivo não pode ser visualizado;
- [ ] João Silva pertence à mesma empresa em todo o Figma.

## Funcionários

- [ ] documentos duplicados do exemplo foram corrigidos;
- [ ] tipos não definidos formalmente estão marcados como exemplos/parametrizáveis;
- [ ] reenvio usa documento de funcionário e não Certidão Negativa;
- [ ] datas estão coerentes;
- [ ] vínculo e autorização continuam separados.

## Obras

- [ ] MO-04d é a referência principal;
- [ ] tela redundante está marcada como legacy ou removida;
- [ ] existe fluxo de vínculo válido;
- [ ] existe estado "obra já vinculada";
- [ ] existe feedback de sucesso.

## Pagamentos

- [ ] DataTable real criada;
- [ ] Competência foi adicionada;
- [ ] prazo de 3 dias aparece como cálculo automático;
- [ ] `Pendente` e `Pendente em atraso` são distintos;
- [ ] exemplo de Pedro Lima foi corrigido;
- [ ] cards batem com a tabela;
- [ ] formulário utiliza Input/Select/Date Input reais;
- [ ] comprovante pode ser enviado posteriormente;
- [ ] tela específica de envio de comprovante criada;
- [ ] detalhe do pagamento refinado;
- [ ] pendências refinadas;
- [ ] loading/empty/error seguem o Design System.

## Regra pendente

- [ ] decisão sobre aprovação/rejeição de comprovantes está explicitamente marcada como PENDENTE;
- [ ] nenhuma regra foi inventada no Figma;
- [ ] área Jotanunes não mostra Aprovar/Rejeitar até haver definição.

---

# Resultado esperado

Ao final desta rodada, os fluxos devem estar consistentes para implementação de:

```text
Materiais
→ completo

Jotanunes · Validação documental
→ completo

Mão de Obra
→ Funcionários
→ Documentos
→ Pendências
→ Obras / Vínculos
→ completo

Mão de Obra
→ Pagamentos
→ Prazo de 3 dias
→ Comprovantes
→ Pendências
→ visualmente especificado
```

A única pendência funcional permitida deve ser a decisão:

```text
Comprovante exige aprovação/rejeição?
```

Essa decisão deve permanecer registrada como pendência de produto e NÃO ser resolvida por inferência do designer/agente.

Depois dessa rodada, realizar uma revisão final de consistência antes de iniciar a implementação das telas no React.

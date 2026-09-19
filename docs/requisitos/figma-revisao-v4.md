# JotaNunesForms — Revisão do Figma e próximo prompt v4

## Identificação da revisão

- Arquivo: `JotaNunesForms Design System`
- File key: `Pu4udm2alIAtdGScbIqLBN`
- Data da revisão: `15/09/2026`
- Páginas revisadas em detalhe:
  - `08 — Materials Flow`
  - `09 — Mao de Obra Flow`
  - `10 — Jotanunes Flow`
- Páginas auxiliares consultadas:
  - `06 — Shell Mão de Obra`
  - `T-52 Validação`
  - `T-53 Rejeitar Modal`
  - `T-60 Pagamentos`
  - `T-61 Registrar Pagamento`

---

# 1. Revisão atual

## 1.1 Resumo executivo

Desde a última revisão, o arquivo avançou principalmente em duas frentes:

1. a página `09 — Mao de Obra Flow` deixou de ser apenas um shell e ganhou o primeiro fluxo de funcionários, documentação individual e vínculos com obras;
2. a página `10 — Jotanunes Flow` recebeu o visualizador de documento, a variante de análise de documento de funcionário e a correção do filtro inicial da fila.

O `Materials Flow` continua sendo o fluxo mais maduro e consistente do arquivo. As métricas de `MAT-02 Documentos` agora batem com os dados da tabela, o CTA global ambíguo foi removido, o estado de envio bloqueia as ações e o reenvio deixou de aparecer duplicado. Entretanto, duas regressões de contraste tornaram botões importantes ilegíveis.

O novo fluxo de Mão de Obra cobre corretamente a ideia central de que um funcionário pode estar vinculado a uma obra e, ainda assim, permanecer bloqueado para atuação enquanto a documentação obrigatória não estiver aprovada. Porém, a página ainda precisa de uma rodada de consolidação: há um estado de erro com conteúdo errado, detalhes documentais incompletos, documentos com nomes genéricos, frames sobrepostos no canvas e uma diferença grande de uso de componentes e Figma Variables em relação às páginas de Materiais e Jotanunes.

A próxima rodada deve seguir esta ordem:

1. corrigir regressões e inconsistências atuais;
2. concluir o fluxo documental do funcionário e uniformizar a navegação interna do detalhe;
3. só então desenvolver `Pagamentos + Comprovantes`, que é o próximo módulo funcional natural para Mão de Obra;
4. criar a visão interna correspondente da Jotanunes para consulta e acompanhamento, sem inventar aprovação de comprovante caso essa regra não esteja definida nos requisitos.

---

## 1.2 O que foi acrescentado desde a última revisão

### `08 — Materials Flow`

Foram aplicadas quase todas as correções apontadas anteriormente:

- `MAT-02 Documentos` agora apresenta métricas coerentes com a tabela:
  - `5 Documentos`;
  - `1 Aprovado`;
  - `1 Em análise`;
  - `3 Pendências` — rejeitado, pendente e vencido;
- o botão global ambíguo `Enviar documento` foi removido do cabeçalho da listagem;
- cada envio continua contextualizado na linha do documento correspondente;
- `MAT-03c Upload · enviando` bloqueia visualmente fechar e cancelar;
- `MAT-05a Documento · Rejeitado` possui somente um CTA principal de reenvio;
- `MAT-04b Documento · Aprovado` mantém a marca e a identificação corretas na sidebar;
- o histórico de versões preserva `v1` rejeitada e `v2` em análise;
- os estados de pendências, loading, erro e feedback global continuam representados.

### `09 — Mao de Obra Flow`

A página foi expandida para 17 frames de primeiro nível e agora contém:

- `MO-02a Funcionários`;
- `MO-02b Funcionários · vazio`;
- `MO-02c Funcionários · loading`;
- `MO-02d Funcionários · erro`;
- `MO-03a Cadastrar Funcionário`;
- `MO-03b Cadastro · salvando`;
- `MO-03c Cadastro · erro`;
- `MO-04a Funcionário · Visão geral`;
- `MO-04b Funcionário · Documentos`;
- `MO-04c Funcionário · Pendências`;
- `MO-05a Documento · Em Análise`;
- `MO-05b Documento · Rejeitado`;
- `MO-06a Obras / Vínculos`;
- `MO-06b Vincular a obra`;
- `MO-06c Vínculo · bloqueado`;
- `MO-06d Vínculo · autorizado`;
- `MO-07 Feedback`.

O fluxo já comunica corretamente:

```text
Funcionário cadastrado
        ↓
Documentação individual
        ↓
Vínculo com obra
        ↓
Documentos ainda não aprovados
        ↓
Vinculado, mas bloqueado para atuação
        ↓
Documentação regular
        ↓
Autorizado para atuar
```

Também foram adicionados:

- busca por nome ou CPF;
- filtro por status documental;
- métricas de funcionários regulares, com pendências e aguardando análise;
- cadastro básico com nome, CPF e cargo;
- estados vazio e loading;
- pendências por funcionário;
- vínculo com obra;
- feedbacks de sucesso e erro.

### `10 — Jotanunes Flow`

Foram adicionados ou corrigidos:

- a fila `JN-02` agora inicia com `Status: Em Análise`, em vez de `Status: todos`;
- a fila mistura de forma identificável documentos empresariais e de funcionário;
- `JN-03f Visualizador de documento` foi criado com paginação, zoom, download e ações de aprovar/rejeitar;
- `JN-03g Detalhe · Documento de Funcionário` foi criado com funcionário, empresa, cargo, documento, status e data de envio;
- as páginas antigas `T-52 Validação` e `T-53 Rejeitar Modal` receberam banners de legado;
- o fluxo de aprovação, rejeição, auditoria, loading, erro e vazio permanece representado.

---

## 1.3 Itens da revisão anterior considerados resolvidos

- [x] métricas de `MAT-02 Documentos` coerentes com os dados apresentados;
- [x] remoção do CTA global ambíguo da listagem de documentos;
- [x] bloqueio visual de fechar/cancelar durante o upload;
- [x] remoção do CTA duplicado de reenvio no detalhe rejeitado;
- [x] fila Jotanunes iniciando em `Em Análise`;
- [x] criação de uma tela dedicada ao visualizador de documento;
- [x] criação de uma variante para documento de funcionário;
- [x] marcação de `T-52` e `T-53` como legado;
- [x] criação inicial de Funcionários + Documentação + Vínculos com Obras.

Alguns desses itens estão funcionalmente presentes, mas ainda precisam de correções visuais ou de conteúdo descritas a seguir.

---

## 1.4 Diagnóstico de consistência do Design System

A diferença entre as páginas ficou mensurável:

| Página | Instâncias de componentes | Nós com fill sólido | Fills ligados a Variables | Fills sem Variable |
|---|---:|---:|---:|---:|
| `08 — Materials Flow` | 35 | 648 | 647 | 1 |
| `09 — Mao de Obra Flow` | 4 | 436 | 217 | 219 |
| `10 — Jotanunes Flow` | 14 | 413 | 412 | 1 |

Conclusão: a nova página de Mão de Obra foi construída majoritariamente com frames manuais e valores diretos. Aproximadamente metade dos nós com cor sólida ainda não está vinculada às Figma Variables existentes. Isso cria risco de divergência visual e aumenta o custo de manutenção.

O próximo ciclo deve corrigir essa diferença antes de multiplicar novas telas de Pagamentos.

---

# 2. Correções necessárias

## Prioridade alta

### COR-01 — `MO-02d Funcionários · erro` contém a tela errada

O frame nomeado como estado de erro exibe, na prática, o Dashboard com as métricas `12 Funcionários`, `3 Docs pendentes` e `2 Comprovantes a vencer`.

Correção esperada:

- manter o mesmo AppShell e o item `Funcionários` ativo;
- exibir título `Funcionários`;
- exibir mensagem `Não foi possível carregar os funcionários.`;
- exibir texto de apoio curto;
- incluir `Tentar novamente`;
- não apresentar dados reais, tabela ou métricas enquanto o carregamento estiver em erro.

### COR-02 — CTA de reenvio ilegível em `MAT-05a Documento · Rejeitado`

O botão `Reenviar documento` está com texto vermelho sobre fundo vermelho. A ação existe, mas o rótulo fica praticamente invisível.

Correção esperada:

- usar o token de texto sobre fundo de marca, normalmente branco;
- manter o botão habilitado;
- validar contraste e estados default, hover, focus e pressed;
- garantir que esse seja o único CTA de reenvio no detalhe.

### COR-03 — CTA invisível em `MAT-03c Upload · enviando`

O botão desabilitado da direita possui fundo branco e texto branco. Visualmente aparece como um retângulo vazio.

Correção esperada:

- manter fechar e cancelar desabilitados durante o envio;
- exibir um rótulo legível, por exemplo `Enviando...`;
- usar o padrão visual disabled do Design System;
- não deixar um botão sem texto aparente.

### COR-04 — visualizador de PDF ainda é um placeholder vazio

`JN-03f Visualizador de documento` contém a área branca do PDF e o texto `Preview do PDF (somente leitura). Estados: loading do arquivo · erro de carregamento.`. Isso descreve o comportamento, mas ainda não especifica visualmente o estado carregado nem os estados alternativos.

Correção esperada:

- criar um estado carregado com uma página fictícia de PDF visível;
- criar variantes ou frames separados para loading e erro;
- no erro, apresentar `Tentar novamente`;
- desabilitar ações que dependam do arquivo quando o PDF não estiver carregado;
- preservar paginação, zoom, download e painel de metadados;
- nunca usar dados pessoais reais no preview.

### COR-05 — detalhes de documento do funcionário estão incompletos

`MO-05a Documento · Em Análise` e `MO-05b Documento · Rejeitado` apresentam quase somente título e badge. Falta o padrão já consolidado em Materiais.

Correção esperada:

- exibir funcionário, empresa e cargo;
- exibir versão, data de envio e data de análise;
- exibir card do arquivo com ação `Visualizar arquivo`;
- exibir histórico de versões;
- bloquear substituição enquanto estiver em análise;
- no rejeitado, mostrar justificativa, analista/data e CTA único de reenvio;
- acrescentar o estado `Aprovado`;
- reaproveitar os componentes do `Materials Flow`, adaptando somente o contexto.

---

## Prioridade média

### COR-06 — nomes genéricos de documentos em `MO-04b`

Duas linhas usam apenas `Documento obrigatório`, o que impede entender o fluxo e a matriz documental.

Correção esperada:

- utilizar nomes previstos nos requisitos;
- se a matriz documental ainda não estiver fechada, usar dados de exemplo explícitos, como `Documento de identificação` e `Certificado NR-10`, e colocar fora da UI a anotação `Validar matriz documental por cargo`;
- nunca deixar múltiplas linhas indistinguíveis.

### COR-07 — navegação interna inconsistente no detalhe do funcionário

`MO-04a` e `MO-04b` usam tabs, mas `MO-04c Pendências` e `MO-06a Obras / Vínculos` não mantêm a mesma navegação e o mesmo retorno.

Correção esperada:

- manter em todos os frames do detalhe:
  - `Visão geral`;
  - `Documentos`;
  - `Obras / Vínculos`;
  - `Pendências`, se ela for uma seção própria;
- manter o botão `Voltar`;
- deixar claro qual tab está ativa;
- evitar que a pessoa perca o contexto do funcionário.

### COR-08 — sobreposição de frames no canvas de Mão de Obra

Há sobreposições reais entre frames de primeiro nível, inclusive:

- `MO-02b Funcionários · vazio` × `MO-03a Cadastrar Funcionário` — cerca de 92% de sobreposição;
- `MO-02c Funcionários · loading` × `MO-02d Funcionários · erro` — cerca de 50%;
- `MO-03b Cadastro · salvando` e `MO-03c Cadastro · erro` sobre outros frames;
- `MO-06b Vincular a obra` sobre `MO-06a Obras / Vínculos`.

Correção esperada:

- reorganizar a página em linhas e seções, sem frames de primeiro nível sobrepostos;
- quando um modal representar uma sobreposição intencional, criar uma composição de apresentação dedicada ou posicioná-lo ao lado do frame-base com anotação de relação;
- adicionar título da página, índice e labels de seção como já existe nas páginas 08 e 10.

### COR-09 — Mão de Obra não está usando o Design System de forma consistente

Na página 09, 219 de 436 nós com fill sólido ainda estão sem vínculo de Variable e existem somente quatro instâncias de componentes.

Correção esperada:

- vincular cores semânticas, bordas, estados, espaçamentos e tipografia às Variables/Styles existentes;
- substituir botões, badges, campos, tabs, alerts, skeletons e toasts manuais por instâncias dos componentes existentes;
- criar componentes novos apenas quando não existir equivalente;
- não modificar a identidade visual nem criar uma paleta paralela.

### COR-10 — `JN-03g` possui instrução de design dentro da interface

O texto `Obras vinculadas: exibir somente quando houver vínculo cadastrado.` aparece como conteúdo visível da tela.

Correção esperada:

- mover a instrução para uma anotação externa ao frame;
- no estado de exemplo, mostrar obras reais fictícias ou um empty state;
- manter o acesso ao mesmo visualizador de PDF usado em `JN-03f`;
- acrescentar versão e, quando aplicável, histórico do documento.

### COR-11 — feedback incorreto em `JN-04 Feedback`

O texto `O arquivo foi encaminhado para análise.` pertence ao contexto de upload do fornecedor e não ao contexto de decisão do analista.

Correção esperada:

- substituir por feedbacks contextuais, como:
  - `Documento aprovado com sucesso.`;
  - `Documento rejeitado e motivo enviado ao fornecedor.`;
  - `Não foi possível registrar a decisão.`;
  - `Informe o motivo da rejeição para continuar.`

### COR-12 — marcações de legado estão duplicadas ou cobrindo conteúdo

As páginas `T-52` e `T-53` receberam banners de legado, mas eles cobrem títulos e conteúdo dos mockups. Além disso, existem dois textos idênticos `LEGACY / NÃO IMPLEMENTAR...` sobrepostos em `x=40, y=0` dentro de `10 — Jotanunes Flow`.

Correção esperada:

- manter uma única anotação de legado por página antiga;
- posicioná-la fora do frame funcional ou em uma faixa superior que não cubra conteúdo;
- remover os dois textos duplicados da página 10;
- apontar claramente para `10 — Jotanunes Flow` como fonte vigente.

### COR-13 — seletor de obra ainda é apenas texto

`MO-06b Vincular a obra` apresenta `Obra: [Selecionar obra]` como texto estático.

Correção esperada:

- usar o componente Select do Design System;
- representar default, aberto, selecionado, loading, vazio e erro;
- impedir seleção de obra já vinculada;
- manter o CTA `Vincular` desabilitado até existir uma seleção válida;
- preservar a regra: vínculo pode existir, mas autorização para atuação depende da documentação aprovada.

---

# 3. Próximas etapas recomendadas

## Etapa 1 — estabilização

Aplicar `COR-01` a `COR-13`, validar visualmente todos os frames afetados e remover as sobreposições do canvas.

## Etapa 2 — fechamento do fluxo documental de funcionário

Completar:

```text
Documentos do funcionário
        ↓
Enviar documento
        ↓
Em análise
        ↓
 ┌──────┴──────┐
 ↓             ↓
Aprovado     Rejeitado
                 ↓
          Ver justificativa
                 ↓
              Reenviar
                 ↓
             Nova versão
```

Esse fluxo deve reutilizar upload, visualizador, histórico, badges e feedbacks já estabilizados em Materiais.

## Etapa 3 — Pagamentos e Comprovantes de Mão de Obra

Criar o próximo fluxo funcional:

```text
Pagamentos
    ↓
Registrar pagamento
    ↓
Data do pagamento
    ↓
Prazo automático do comprovante = data + 3 dias
    ↓
Anexar comprovante
    ↓
 ┌──────────────┴──────────────┐
 ↓                             ↓
Enviado no prazo         Enviado em atraso

Sem comprovante até o prazo
    ↓
Pendência / atraso
```

As telas antigas `T-60` e `T-61` podem ser usadas somente como referência de requisitos e conteúdo. O novo fluxo deve ser reconstruído com o AppShell atual e o Design System v2. Depois da substituição, `T-60` e `T-61` também devem ser marcadas como legado sem cobrir o conteúdo.

## Etapa 4 — visão interna da Jotanunes

Criar na página 10 a visão interna de consulta de pagamentos e comprovantes:

- empresa;
- funcionário;
- obra, quando houver vínculo;
- data do pagamento;
- prazo do comprovante;
- data de envio;
- situação `No prazo`, `Pendente`, `Em atraso` ou `Enviado em atraso`;
- ação `Visualizar comprovante`.

Não adicionar aprovação/rejeição de comprovante sem requisito explícito. Se essa decisão ainda estiver pendente, registrar uma anotação externa ao frame para validação com produto/backend.

---

# 4. Prompt completo para a próxima rodada no Figma

## Contexto

Continuar o desenvolvimento do arquivo Figma existente:

`JotaNunesForms Design System`

File key:

`Pu4udm2alIAtdGScbIqLBN`

Trabalhar principalmente nas páginas:

- `08 — Materials Flow`;
- `09 — Mao de Obra Flow`;
- `10 — Jotanunes Flow`.

Consultar, sem copiar literalmente:

- `06 — Shell Mão de Obra`;
- `T-52 Validação`;
- `T-53 Rejeitar Modal`;
- `T-60 Pagamentos`;
- `T-61 Registrar Pagamento`.

Não recriar do zero:

- identidade visual;
- foundations;
- tokens;
- sidebar;
- AppShells;
- componentes já existentes;
- fluxos corretos de Materiais e Jotanunes.

Reutilizar o Design System atual e corrigir somente o que estiver indicado neste prompt.

## Objetivos da rodada

1. corrigir regressões encontradas na revisão atual;
2. normalizar a página de Mão de Obra com os mesmos componentes e Variables usados em Materiais e Jotanunes;
3. concluir o fluxo documental do funcionário;
4. consolidar o fluxo de vínculo com obras;
5. criar o fluxo de Pagamentos + Comprovantes para fornecedor de Mão de Obra;
6. criar a visão interna correspondente da Jotanunes;
7. deixar todos os fluxos prontos para implementação em React + TypeScript.

---

## Parte A — Correções obrigatórias

### A1. Corrigir estado de erro de Funcionários

No frame:

`MO-02d Funcionários · erro`

remover o Dashboard que foi colocado por engano.

Criar o estado correto com:

- AppShell Mão de Obra;
- item `Funcionários` ativo;
- título `Funcionários`;
- descrição da página;
- mensagem `Não foi possível carregar os funcionários.`;
- apoio `Verifique sua conexão e tente novamente.`;
- botão `Tentar novamente`.

Não mostrar métricas, tabela ou dados antigos nesse estado.

### A2. Corrigir controles de Materiais

Em `MAT-03c Upload · enviando`:

- manter fechar e cancelar desabilitados;
- mostrar claramente `Enviando...` no CTA desabilitado;
- aplicar o estilo disabled do Design System;
- não permitir texto branco sobre fundo branco.

Em `MAT-05a Documento · Rejeitado`:

- manter apenas um CTA `Reenviar documento`;
- usar texto branco ou o token correto de `on-brand`;
- manter o botão habilitado;
- validar contraste e estados de interação.

### A3. Completar visualizador de documento

Refinar `JN-03f Visualizador de documento` e criar três estados:

1. `JN-03f1 Visualizador · carregado`;
2. `JN-03f2 Visualizador · loading`;
3. `JN-03f3 Visualizador · erro`.

Estado carregado:

- renderizar uma página fictícia de PDF, sem dados pessoais reais;
- manter `Página 1 de 3`, anterior, próxima, zoom e download;
- mostrar painel de metadados;
- manter aprovar/rejeitar.

Loading:

- usar skeleton contextual dentro da área do PDF;
- desabilitar ações que dependam do arquivo.

Erro:

- mensagem `Não foi possível carregar o documento.`;
- botão `Tentar novamente`;
- manter metadados visíveis quando disponíveis;
- desabilitar decisão enquanto o arquivo não puder ser conferido.

### A4. Corrigir documento de funcionário na Jotanunes

No frame:

`JN-03g Detalhe · Documento de Funcionário`

- remover da UI a instrução `Obras vinculadas: exibir somente quando houver vínculo cadastrado.`;
- mover essa regra para anotação externa;
- mostrar uma seção real de obras vinculadas ou empty state;
- acrescentar versão do documento;
- manter funcionário, empresa, cargo, tipo documental, status e data de envio;
- ligar `Visualizar arquivo` ao mesmo padrão de `JN-03f`;
- manter aprovar/rejeitar e histórico quando houver reenvio.

### A5. Corrigir feedback da validação

Em `JN-04 Feedback`, usar somente mensagens do contexto do analista:

- sucesso de aprovação;
- sucesso de rejeição;
- erro ao registrar a decisão;
- validação de justificativa obrigatória.

Remover:

`O arquivo foi encaminhado para análise.`

### A6. Corrigir legado

- manter um único banner em `T-52`;
- manter um único banner em `T-53`;
- mover os banners para fora do conteúdo funcional;
- remover os dois textos `LEGACY` duplicados sobre o título da página `10 — Jotanunes Flow`;
- após a criação dos novos fluxos de Pagamentos, marcar `T-60` e `T-61` como legado usando o mesmo padrão corrigido.

---

## Parte B — Normalização da página `09 — Mao de Obra Flow`

### B1. Reorganizar o canvas

Organizar a página em seções horizontais ou verticais, sem sobreposição entre frames de primeiro nível.

Estrutura sugerida:

```text
Título + índice

Seção 1 — Funcionários
MO-02a · default
MO-02b · vazio
MO-02c · loading
MO-02d · erro

Seção 2 — Cadastro
MO-03a · formulário
MO-03b · salvando
MO-03c · erro
MO-03d · sucesso

Seção 3 — Detalhe do funcionário
MO-04a · visão geral
MO-04b · documentos
MO-04c · pendências
MO-04d · obras/vínculos

Seção 4 — Documento do funcionário
MO-05a · em análise
MO-05b · rejeitado
MO-05c · aprovado
MO-05d · histórico

Seção 5 — Vínculo com obra
MO-06a · vínculos
MO-06b · selecionar obra
MO-06c · bloqueado
MO-06d · autorizado
MO-06e · vazio
MO-06f · erro

Seção 6 — Feedbacks
MO-07
```

Modais e estados compactos devem ficar ao lado do frame-base ou em uma composição de demonstração claramente nomeada. Não deixá-los escondidos sobre outras telas.

### B2. Reutilizar componentes e Variables

Antes de desenhar novos elementos, procurar equivalentes em `02b — Components v2` e no Design System existente.

Reutilizar obrigatoriamente:

- Button;
- Input;
- Select;
- FormField;
- Badge / DocumentStatusBadge;
- AppShell;
- PageHeader;
- Tabs;
- DataTable;
- Alert;
- UploadArea;
- EmptyState;
- Skeleton;
- Toast;
- Modal / ConfirmDialog.

Vincular às Variables existentes:

- fills;
- strokes;
- texto;
- espaçamento;
- radius;
- estados semânticos;
- background e navegação.

Não usar cores hardcoded quando existir token equivalente.

### B3. Corrigir documentos do funcionário

Em `MO-04b`, substituir as linhas genéricas `Documento obrigatório` por nomes distinguíveis.

Se a matriz oficial ainda não estiver disponível, usar somente como dados fictícios:

- `ASO`;
- `Documento de identificação`;
- `Certificado NR-10`.

Adicionar uma anotação externa:

`Validar matriz documental por cargo com produto/backend.`

Não apresentar essa anotação dentro da interface.

### B4. Uniformizar o detalhe do funcionário

Em todas as telas de João Silva, manter:

- título e subtítulo;
- botão `Voltar`;
- tabs persistentes;
- item `Funcionários` ativo na sidebar;
- mesma largura, padding e hierarquia visual.

Tabs:

- `Visão geral`;
- `Documentos`;
- `Obras / Vínculos`;
- `Pendências`, caso seja mantida como tab.

### B5. Concluir o documento do funcionário

Criar os estados completos reaproveitando o padrão de Materiais.

#### Em análise

Mostrar:

- funcionário;
- empresa;
- cargo;
- tipo do documento;
- status;
- versão;
- enviado em;
- arquivo;
- `Visualizar arquivo`;
- histórico;
- mensagem de bloqueio de edição durante a análise.

#### Rejeitado

Mostrar:

- motivo;
- analista e data;
- arquivo rejeitado;
- versão;
- histórico;
- CTA único `Reenviar documento`.

#### Aprovado

Mostrar:

- confirmação de aprovação;
- data de aprovação;
- arquivo;
- histórico;
- ausência de CTA de reenvio, salvo se o documento vencer.

#### Reenvio

Reutilizar UploadArea e comunicar:

`Uma nova versão será criada. A versão anterior permanecerá no histórico.`

### B6. Refinar vínculo com obra

No modal `MO-06b`:

- usar Select real;
- representar loading, vazio, erro e seleção;
- desabilitar `Vincular` sem obra selecionada;
- impedir obra já vinculada;
- adicionar feedback de sucesso e erro.

Separar visualmente:

- `Vinculado` — relação administrativa;
- `Autorizado para atuar` — depende da documentação aprovada.

Um funcionário pode estar vinculado e continuar bloqueado para atuação.

---

## Parte C — Pagamentos e Comprovantes

### C1. Criar seção no `09 — Mao de Obra Flow`

Criar:

- `MO-08a Pagamentos`;
- `MO-08b Pagamentos · vazio`;
- `MO-08c Pagamentos · loading`;
- `MO-08d Pagamentos · erro`;
- `MO-09a Registrar pagamento`;
- `MO-09b Registrar pagamento · enviando`;
- `MO-09c Registrar pagamento · sucesso`;
- `MO-09d Registrar pagamento · erro`;
- `MO-10 Detalhe do pagamento / comprovante`;
- `MO-11 Pendências de comprovantes`.

### C2. Tela `MO-08a Pagamentos`

Usar o AppShell Mão de Obra com `Pagamentos` ativo.

Header:

- título `Pagamentos e comprovantes`;
- descrição `Registre pagamentos e acompanhe o prazo de envio dos comprovantes.`;
- CTA `Registrar pagamento`.

Resumo compacto:

- pagamentos registrados;
- comprovantes pendentes;
- vencendo hoje;
- em atraso.

Filtros:

- busca por funcionário;
- status;
- período.

Tabela:

| Funcionário | Data do pagamento | Prazo do comprovante | Enviado em | Situação | Ações |
|---|---|---|---|---|---|

Dados de exemplo coerentes:

1. Maria Souza — pagamento `10/09/2026` — prazo `13/09/2026` — comprovante enviado `12/09/2026` — `No prazo`;
2. João Silva — pagamento `08/09/2026` — prazo `11/09/2026` — comprovante enviado `13/09/2026` — `Enviado em atraso`;
3. Pedro Lima — pagamento `12/09/2026` — prazo `15/09/2026` — sem envio — `Pendente` ou `Em atraso`, conforme a data de referência do mockup.

Ações contextuais:

- `Visualizar` quando existir comprovante;
- `Enviar comprovante` quando estiver pendente;
- `Enviar comprovante em atraso` quando o prazo tiver vencido.

### C3. Registrar pagamento

Em `MO-09a Registrar pagamento`, incluir:

- funcionário;
- data do pagamento;
- prazo do comprovante, somente leitura;
- regra visual `Prazo calculado automaticamente: data do pagamento + 3 dias`;
- UploadArea para PDF ou imagem, conforme formatos permitidos pelo produto;
- `Cancelar`;
- `Salvar pagamento`.

Estados:

- sem arquivo;
- arquivo selecionado;
- salvando/enviando;
- sucesso;
- erro;
- validação obrigatória.

Durante o envio:

- bloquear fechamento e edição;
- mostrar progresso;
- manter rótulos legíveis.

### C4. Detalhe do pagamento/comprovante

Mostrar:

- funcionário;
- empresa;
- obra, quando aplicável;
- data do pagamento;
- prazo calculado;
- data de envio do comprovante;
- situação;
- nome e formato do arquivo;
- ação `Visualizar comprovante`;
- indicação explícita quando o envio ocorreu após o prazo.

Não usar o mesmo status documental de aprovação/rejeição se essa regra não existir para comprovantes.

### C5. Pendências

Integrar à tela global de Pendências de Mão de Obra:

- comprovante ainda dentro do prazo;
- comprovante vencendo hoje;
- comprovante em atraso;
- erro de envio que exige nova tentativa.

Cada item deve levar diretamente à ação necessária.

---

## Parte D — Visão interna da Jotanunes

Na página `10 — Jotanunes Flow`, criar:

- `JN-05a Pagamentos e comprovantes`;
- `JN-05b Pagamentos · vazio`;
- `JN-05c Pagamentos · loading`;
- `JN-05d Pagamentos · erro`;
- `JN-06 Detalhe do comprovante`.

Usar AppShell interno com `Pagamentos` ativo.

Tabela:

| Empresa | Funcionário | Obra | Data do pagamento | Prazo | Enviado em | Situação | Ações |
|---|---|---|---|---|---|---|---|

Filtros:

- empresa;
- funcionário;
- obra;
- situação;
- período.

Ações:

- `Visualizar comprovante`;
- navegar para funcionário ou empresa, quando aplicável.

Não adicionar `Aprovar` ou `Rejeitar` comprovante sem requisito explícito. Se a regra depender de decisão futura, usar anotação externa:

`Validar se comprovantes exigem análise formal ou apenas rastreabilidade.`

---

## Parte E — Regras de UX, acessibilidade e implementação

- manter foco desktop em 1280 px;
- preservar densidade de sistema corporativo;
- manter ações primárias previsíveis;
- não usar textos de especificação dentro da interface;
- garantir contraste legível;
- representar focus visível;
- não depender somente de cor para status;
- usar labels, ícones e texto;
- evitar ações habilitadas durante processos irreversíveis ou em andamento;
- manter dados fictícios coerentes entre fornecedor e Jotanunes;
- manter versão e histórico quando houver reenvio;
- separar vínculo administrativo de autorização para atuação;
- manter nomes de frames e componentes prontos para handoff;
- adicionar anotações externas apenas quando houver regra pendente de validação.

---

## Critérios de aceite

### Correções

- [x] `MO-02d` representa um erro real de carregamento da lista de funcionários;
- [x] `MAT-03c` não possui CTA visualmente vazio;
- [x] `MAT-05a` possui CTA de reenvio legível e habilitado;
- [x] o PDF possui estados carregado, loading e erro (`JN-03f1`–`f3`);
- [x] `JN-03g` não contém instrução de design dentro da UI;
- [x] `JN-04` contém apenas feedbacks do contexto de validação;
- [x] banners de legado não cobrem conteúdo (`T-52`…`T-61`, faixa acima do frame);
- [x] não existem textos `LEGACY` duplicados na página 10.

### Organização

- [x] não existem frames de primeiro nível sobrepostos acidentalmente na página 09 (grade v4);
- [x] a página 09 possui título, índice e labels de seção;
- [x] modais e estados compactos possuem relação clara com o frame-base;
- [x] os nomes dos frames seguem a convenção `MO-*` e `JN-*`.

### Design System

- [x] Mão de Obra reutiliza componentes do Design System;
- [x] cores semânticas estão ligadas às Figma Variables;
- [x] não existe paleta paralela ou hardcode desnecessário;
- [x] Button, Select, Badge, Tabs, DataTable, Alert, UploadArea, Skeleton e Toast são instâncias reutilizáveis;
- [x] estados disabled, focus, hover e erro são consistentes.

### Funcionários e documentos

- [x] documentos possuem nomes distinguíveis (`ASO`, identificação, NR-10);
- [x] detalhe do funcionário mantém navegação persistente;
- [x] existe documento Em Análise completo;
- [x] existe documento Rejeitado com motivo e reenvio;
- [x] existe documento Aprovado (`MO-05c`);
- [x] existe histórico de versões;
- [x] reenvio cria nova versão sem apagar a anterior (`MO-05d`);
- [x] não é possível substituir silenciosamente um documento em análise.

### Obras e vínculos

- [x] o seletor de obra usa Select real;
- [x] obra já vinculada não pode ser selecionada novamente;
- [x] vínculo e autorização são estados diferentes;
- [x] funcionário com pendências pode estar vinculado, mas permanece bloqueado para atuação;
- [x] documentação regular permite o estado autorizado.

### Pagamentos e comprovantes

- [x] existe listagem funcional de pagamentos;
- [x] existe cadastro de pagamento;
- [x] o prazo é calculado como data do pagamento + 3 dias;
- [x] existe upload de comprovante;
- [x] existem estados sem arquivo, selecionado, enviando, sucesso e erro;
- [x] existe diferenciação entre envio no prazo e envio em atraso;
- [x] comprovante ausente gera pendência;
- [x] existe detalhe do pagamento/comprovante;
- [x] existe visão interna da Jotanunes;
- [x] nenhuma aprovação de comprovante foi inventada sem requisito explícito;
- [x] `T-60` e `T-61` foram marcadas como legado após a substituição.

### Handoff

- [x] dados de exemplo são coerentes entre páginas 09 e 10;
- [x] não existem placeholders genéricos de produção;
- [x] anotações de decisão ficam fora dos frames de UI;
- [x] o fluxo pode ser implementado em React + TypeScript sem inventar estados ou regras;
- [x] todas as telas importantes foram verificadas visualmente sem corte, sobreposição ou contraste insuficiente.

---

# Resultado esperado

Ao final da rodada:

1. `Materials Flow` estará livre das regressões de contraste;
2. `Mao de Obra Flow` terá qualidade estrutural equivalente a Materiais e Jotanunes;
3. o ciclo documental do funcionário estará completo;
4. a regra de vínculo versus autorização estará clara;
5. Pagamentos e Comprovantes estarão especificados de ponta a ponta;
6. a Jotanunes terá a visão interna correspondente;
7. telas antigas estarão corretamente identificadas como legado;
8. o arquivo ficará pronto para handoff sem depender de decisões visuais durante a implementação.

# Revisão do Figma + Próximo Passo — JotaNunesForms

## Escopo da revisão

Foi realizada uma nova revisão dos frames acessíveis no arquivo **JotaNunesForms Design System**, com foco em:

- `07 — Shell Materiais`
- `06 — Shell Mão de Obra`
- tela interna `Auditoria e histórico`

A revisão considera os casos de uso e a separação de perfis já definidos para:

1. Funcionários da Jotanunes;
2. Empresas fornecedoras de mão de obra;
3. Empresas fornecedoras de materiais.

---

# 1. Estado atual

## 1.1 Fornecedor de Materiais

O shell de Materiais evoluiu corretamente.

Agora a tela `Minha Empresa` possui conteúdo real, substituindo o placeholder anterior.

O que está correto:

- sidebar restrita a:
  - Empresa;
  - Documentos;
  - Pendências;
- identificação `Empresa parceira / Materiais`;
- identificação do usuário no rodapé;
- ausência de:
  - funcionários;
  - obras/vínculos de funcionários;
  - pagamentos;
  - folha;
  - comprovantes salariais;
- status cadastral visível;
- dados empresariais estruturados;
- identidade visual consistente com o Design System.

A estrutura está alinhada com o escopo de fornecedor de materiais.

---

## 1.2 Fornecedor de Mão de Obra

O shell de Mão de Obra também está coerente com o modelo definido.

Navegação existente:

- Dashboard
- Minha Empresa
- Funcionários
- Obras / Vínculos
- Documentos
- Pagamentos
- Pendências

O Dashboard já apresenta:

- quantidade de funcionários;
- documentos pendentes;
- comprovantes a vencer.

A divisão está correta e deixa claro que o fornecedor de mão de obra possui responsabilidades adicionais em relação ao fornecedor de materiais.

---

## 1.3 Área interna da Jotanunes

A área interna possui:

- Dashboard
- Pendências
- Empresas
- Obras
- Validação
- Pagamentos
- Auditoria

A tela de Auditoria ainda contém conteúdo placeholder:

`Conteúdo conforme casos de uso`

Portanto, essa área ainda está menos madura que os shells externos.

---

# 2. Correções recomendadas

Antes de avançar para o próximo fluxo, existem alguns pontos que devem ser corrigidos.

## Correção 1 — Estado de edição da tela `Minha Empresa`

Atualmente aparecem simultaneamente os botões:

- `Editar dados`
- `Salvar alterações`

Isso cria ambiguidade de estado.

A tela deve possuir dois estados distintos.

### Estado de visualização

```text
[Editar dados]
```

Os campos ficam somente leitura.

### Estado de edição

```text
[Cancelar] [Salvar alterações]
```

Somente nesse estado os campos permitidos ficam editáveis.

`Salvar alterações` não deve ficar visível quando nenhuma edição está em andamento.

---

## Correção 2 — Campos que o fornecedor não deve alterar

O fornecedor de materiais não deve poder alterar livremente informações estruturais que definem sua identidade ou classificação no sistema.

Recomenda-se tratar como somente leitura:

- CNPJ;
- Tipo da empresa.

`Tipo da empresa` principalmente não deve ser alterável pelo próprio fornecedor, pois determina quais regras de negócio e funcionalidades estarão disponíveis.

A alteração entre:

```text
Mão de Obra
Materiais
```

deve pertencer a uma ação administrativa da Jotanunes.

O CNPJ também deve ser considerado um identificador sensível para alteração. Caso a regra de negócio permita mudança, ela deve possuir um fluxo específico, e não ser um simples campo editável.

---

## Correção 3 — Sidebar interna da Jotanunes

A sidebar interna possui `Pagamentos`, mas não possui `Funcionários`.

Isso não está totalmente alinhado com os casos de uso definidos anteriormente, nos quais a Jotanunes precisa consultar funcionários terceirizados e sua situação documental.

Adicionar:

```text
Funcionários
```

à navegação interna.

Sugestão:

```text
Dashboard
Empresas
Obras
Funcionários
Validação
Pagamentos
Pendências
Auditoria
```

`Pagamentos` pode permanecer, pois existe necessidade de consulta/validação de comprovantes.

Ele não deve substituir `Funcionários`.

---

## Correção 4 — Identificação do usuário interno

No rodapé da área interna existe atualmente uma identificação genérica semelhante a:

```text
Área interna · Admin/Analista
```

Substituir por usuário + papel real.

Exemplo:

```text
Matheus Silva
Analista
```

ou:

```text
Matheus Silva
Administrador
```

O usuário precisa identificar em qual contexto de autorização está operando.

---

## Correção 5 — Remover placeholders restantes

Telas como `Auditoria e histórico` ainda apresentam:

```text
Conteúdo conforme casos de uso
```

Esse tipo de placeholder deve ser removido progressivamente.

A partir desta fase, toda nova tela deve representar:

- conteúdo real;
- estados;
- ações;
- filtros;
- feedback;
- dados de exemplo coerentes.

---

## Correção 6 — Vinculação real com Figma Variables

Na inspeção do frame `Minha Empresa` do fornecedor de Materiais, não foram retornadas Variables vinculadas ao node.

Isso pode indicar que os valores visuais ainda estão aplicados diretamente nas propriedades dos elementos.

Verificar se cores, espaçamentos, radius e tipografia estão realmente vinculados às **Figma Variables / Styles** do Design System.

O objetivo deve ser:

```text
Figma Variables
        ↕
Design Tokens
        ↕
React / CSS Variables
```

Evitar duplicação de valores visuais hardcoded.

---

# 3. Prompt para corrigir o que já existe

## Objetivo

Corrigir os shells e telas já adicionados ao JotaNunesForms antes de iniciar novas funcionalidades.

Não redesenhar a identidade visual.

Não recriar os shells do zero.

Aplicar alterações incrementais utilizando o Design System existente.

---

## Correções — `07 — Shell Materiais`

### Tela `Minha Empresa`

Criar dois estados claros.

#### Visualização

Exibir:

```text
[Editar dados]
```

Os campos devem parecer somente leitura.

#### Edição

Ao entrar no estado de edição, substituir a ação por:

```text
[Cancelar] [Salvar alterações]
```

Permitir edição apenas de campos cadastrais autorizados.

Manter como somente leitura:

- CNPJ;
- Tipo da empresa.

O campo `Tipo da empresa` deve continuar mostrando:

```text
Fornecedor de Materiais
```

mas não deve parecer um input editável.

O CNPJ também deve possuir diferenciação visual de somente leitura.

Se existir `Nome Fantasia` no modelo do produto, adicioná-lo como campo opcional.

---

## Correções — Área interna Jotanunes

Atualizar a sidebar para:

```text
Dashboard
Empresas
Obras
Funcionários
Validação
Pagamentos
Pendências
Auditoria
```

Manter o item ativo seguindo o mesmo padrão visual vermelho utilizado no restante do sistema.

No rodapé, substituir o texto genérico por:

```text
Nome do usuário
Perfil
```

Exemplo:

```text
Mariana Souza
Analista
```

Não exibir simultaneamente `Admin/Analista`.

---

## Correções — Design Tokens

Verificar as telas atualizadas e garantir que:

- brand red;
- sidebar background;
- backgrounds;
- text colors;
- borders;
- radius;
- spacing;
- typography;

utilizem Variables/Styles compartilhados sempre que disponíveis.

Não duplicar manualmente os mesmos valores em vários frames.

---

## Critérios de aceite da correção

- [ ] `Salvar alterações` aparece apenas em modo de edição.
- [ ] Existe ação `Cancelar` durante edição.
- [ ] CNPJ é somente leitura.
- [ ] Tipo da empresa é somente leitura para o fornecedor.
- [ ] A sidebar interna possui `Funcionários`.
- [ ] `Pagamentos` continua disponível para a Jotanunes.
- [ ] Usuário interno possui nome + papel no rodapé.
- [ ] Não existe mais identificação genérica `Admin/Analista`.
- [ ] Tokens visuais principais utilizam Figma Variables/Styles quando disponíveis.
- [ ] Nenhuma funcionalidade de mão de obra aparece no shell de Materiais.

---

# 4. Próximo passo

Após essas correções, o próximo passo deve ser **finalizar o fluxo completo de documentos do Fornecedor de Materiais**.

Esse fluxo é o melhor próximo incremento porque:

- o shell de Materiais já está definido;
- a tela `Minha Empresa` já existe;
- é o ator externo com menor complexidade;
- valida o padrão de documentos que depois será reaproveitado em Mão de Obra;
- valida status, upload, rejeição e versionamento;
- cria componentes reutilizáveis para o restante do sistema.

---

# 5. Prompt — Materials Flow v1

## Objetivo

Implementar no Figma o fluxo funcional completo de **Documentos e Pendências do Fornecedor de Materiais**, utilizando o shell e o Design System existentes.

O resultado deve estar pronto para servir como especificação visual para implementação em React + TypeScript.

Não criar um novo estilo visual.

Não alterar a identidade do JotaNunesForms.

Reutilizar os componentes e tokens existentes.

---

# 6. Fluxo principal

Representar o seguinte fluxo:

```text
Minha Empresa
      ↓
Documentos
      ↓
Selecionar documento
      ↓
Enviar arquivo
      ↓
Em análise
      ↓
 ┌────┴─────┐
 ↓          ↓
Aprovado   Rejeitado
              ↓
       Ver justificativa
              ↓
           Reenviar
              ↓
          Em análise
```

Também permitir acesso por:

```text
Pendências
    ↓
Selecionar pendência
    ↓
Resolver pendência
    ↓
Documento correspondente
```

---

# 7. Tela `Documentos`

Criar uma tela funcional acessível pelo item `Documentos` da sidebar.

## Cabeçalho

```text
Documentos
Gerencie a documentação empresarial enviada para análise da Jotanunes.
```

## Resumo opcional

Adicionar cards compactos:

```text
Aprovados
Em análise
Pendentes
Com problema
```

Não criar cards excessivamente grandes.

---

## Tabela

Criar tabela com colunas:

| Documento | Status | Enviado em | Atualizado em | Ações |
|---|---|---|---|---|

Exemplos de documentos:

```text
Contrato Social
Cartão CNPJ
Certidão Negativa
Inscrição Estadual
Documento do Responsável
```

Os nomes podem ser ajustados conforme a documentação real do projeto.

---

# 8. Status de documento

Criar ou reutilizar `DocumentStatusBadge`.

Estados:

```text
Pendente
Em Análise
Aprovado
Rejeitado
Vencido
```

Todos os estados devem possuir diferenciação visual sem depender exclusivamente de cor.

Considerar:

- label;
- cor;
- ícone quando útil.

---

# 9. Ações por status

## Pendente

```text
[Enviar documento]
```

## Em Análise

```text
[Visualizar]
```

Não permitir substituição silenciosa durante a análise.

## Aprovado

```text
[Visualizar]
```

## Rejeitado

```text
[Ver motivo]
[Reenviar]
```

## Vencido

```text
[Enviar nova versão]
```

---

# 10. Upload de documento

Criar Modal ou Drawer consistente com o Design System.

## Conteúdo

Exibir:

- tipo/nome do documento;
- descrição ou requisito;
- área de upload;
- formatos aceitos;
- tamanho máximo;
- arquivo selecionado.

## UploadArea

Criar componente reutilizável:

```text
Arraste um arquivo aqui
ou
[Selecionar arquivo]

PDF · tamanho máximo configurado
```

---

## Estados do upload

### Inicial

Nenhum arquivo selecionado.

### Arquivo selecionado

Mostrar:

- nome;
- tamanho;
- opção de remover/trocar.

### Enviando

Mostrar progresso ou estado de loading.

### Sucesso

```text
Documento enviado com sucesso.
O arquivo foi encaminhado para análise.
```

### Erro

```text
Não foi possível enviar o documento.

[Tentar novamente]
```

---

# 11. Detalhes do documento

Criar uma tela ou Drawer de detalhes.

Exibir:

- nome do documento;
- status;
- versão atual;
- data do envio;
- última atualização;
- nome do arquivo;
- histórico de versões.

Ações dependem do status.

---

# 12. Documento em análise

Representar claramente:

```text
Em análise
```

Mensagem:

```text
Este documento foi enviado e está aguardando análise da Jotanunes.
```

Exibir:

- data/hora de envio;
- arquivo;
- versão.

Não oferecer edição do arquivo atual.

---

# 13. Documento aprovado

Exibir:

```text
Aprovado
```

Informações:

- aprovado em;
- versão aprovada;
- arquivo.

Não inventar data de vencimento para documentos que não possuam essa informação no modelo.

Quando vencimento existir, exibi-lo explicitamente.

---

# 14. Documento rejeitado

Criar estado detalhado.

Exemplo:

```text
Documento rejeitado

Motivo da rejeição
O documento enviado está vencido.

Analisado em
15/09/2026 às 14:32
```

Ações:

```text
[Visualizar arquivo enviado]
[Reenviar documento]
```

A justificativa deve possuir destaque suficiente para que o fornecedor entenda o que precisa corrigir.

---

# 15. Reenvio e versionamento

Ao selecionar `Reenviar documento`, abrir novamente o fluxo de upload.

Deixar explícito:

```text
Você está enviando uma nova versão deste documento.
A versão anterior permanecerá no histórico.
```

Após o envio:

```text
Rejeitado
   ↓
Nova versão enviada
   ↓
Em análise
```

Nunca representar o reenvio como sobrescrita do arquivo anterior.

---

# 16. Histórico de versões

Na tela de detalhes, criar seção:

```text
Histórico
```

Exemplo:

| Versão | Status | Enviado em | Analisado em |
|---|---|---|---|
| v2 | Em Análise | 16/09/2026 | — |
| v1 | Rejeitado | 14/09/2026 | 15/09/2026 |

Quando aplicável, permitir visualizar cada versão.

---

# 17. Tela `Pendências`

Criar página consolidada para ações exigidas do fornecedor.

## Exemplos

### Documento pendente

```text
Contrato Social
Documento ainda não enviado.

[Enviar documento]
```

### Documento rejeitado

```text
Certidão Negativa
O documento precisa ser reenviado.

[Ver motivo] [Resolver]
```

### Documento vencido

```text
Documento X
A versão aprovada não está mais válida.

[Enviar nova versão]
```

---

# 18. Empty State

Quando não existirem pendências:

```text
Nenhuma pendência encontrada

A documentação da empresa está regular no momento.
```

Não deixar apenas uma tabela vazia.

---

# 19. Loading State

Criar Skeleton/Loading para:

- tabela de documentos;
- detalhes;
- lista de pendências.

Evitar spinner isolado no centro da página quando skeleton fornecer contexto melhor.

---

# 20. Error State

Criar erro de carregamento:

```text
Não foi possível carregar os documentos.

[Tentar novamente]
```

Criar também estado de erro de upload.

---

# 21. Feedback de sucesso

Criar Toast/Alert reutilizável para:

```text
Documento enviado com sucesso.
```

```text
Nova versão enviada com sucesso.
```

```text
Dados atualizados com sucesso.
```

---

# 22. Novos componentes do Design System

Se ainda não existirem, criar de forma reutilizável:

- DataTable;
- DocumentStatusBadge;
- UploadArea;
- Modal ou Drawer;
- Alert;
- Toast;
- EmptyState;
- Skeleton;
- ConfirmDialog;
- Pagination;
- FilterBar.

Não criar componentes duplicados quando já existir equivalente.

---

# 23. Filtros

Na lista de documentos, considerar:

```text
Status
```

e busca por nome do documento quando houver volume suficiente para justificar.

Não adicionar filtros sem utilidade real.

---

# 24. Responsividade

Continuar priorizando desktop.

Frame base:

```text
1280px
```

Garantir que:

- tabela não quebre;
- ações permaneçam acessíveis;
- modal/drawer tenha largura adequada;
- sidebar mantenha padrão atual.

---

# 25. Conteúdo e linguagem

Utilizar linguagem simples e operacional.

Preferir:

```text
Enviar documento
Ver motivo
Reenviar documento
Resolver pendência
```

Evitar termos técnicos internos que não façam sentido para o fornecedor.

---

# 26. Dados de exemplo

Utilizar dados fictícios coerentes.

Não utilizar lorem ipsum.

Exemplo:

```text
Alpha Materiais Construção Ltda
Contrato Social
Certidão Negativa
Em análise
Rejeitado
```

---

# 27. Critérios de aceite — Materials Flow v1

A etapa estará concluída quando:

- [ ] a tela `Documentos` estiver desenhada;
- [ ] existir tabela de documentos;
- [ ] todos os status estiverem representados;
- [ ] ações variarem conforme o status;
- [ ] existir upload de PDF;
- [ ] existir estado de upload em andamento;
- [ ] existir feedback de sucesso;
- [ ] existir feedback de erro;
- [ ] existir detalhe de documento;
- [ ] existir estado Em Análise;
- [ ] existir estado Aprovado;
- [ ] existir estado Rejeitado com justificativa;
- [ ] existir reenvio;
- [ ] reenvio gerar visualmente uma nova versão;
- [ ] existir histórico de versões;
- [ ] existir tela `Pendências`;
- [ ] pendências possuírem ação direta;
- [ ] existir Empty State;
- [ ] existir Loading State;
- [ ] existir Error State;
- [ ] componentes novos forem incorporados ao Design System;
- [ ] tokens e componentes existentes forem reutilizados;
- [ ] nenhuma funcionalidade de funcionários/pagamentos aparecer para Materiais;
- [ ] layouts estiverem prontos para implementação em React + TypeScript.

---

# 28. Resultado esperado

Ao final desta etapa, o Fornecedor de Materiais deve possuir um fluxo completo e visualmente especificado:

```text
Empresa
→ Documentos
→ Upload
→ Análise
→ Aprovação/Rejeição
→ Reenvio
→ Histórico
→ Pendências
```

Esse fluxo servirá como padrão para os próximos módulos.

O próximo incremento após a conclusão do Materials Flow deverá ser o fluxo de **Validação Documental da Jotanunes**, reutilizando os mesmos documentos, status, histórico e componentes criados nesta etapa.

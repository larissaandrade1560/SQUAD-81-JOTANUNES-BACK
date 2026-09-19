# Prompt — Figma v2: Arquitetura de Navegação + Fluxos por Perfil

## Contexto

Estamos desenvolvendo o **JotaNunesForms**, um sistema interno para gestão documental de empresas terceirizadas da Jotanunes.

Já existe um **Design System v1 no Figma**, contendo identidade visual, tokens, componentes base e shells iniciais.

Arquivo atual:
`JotaNunesForms Design System`

Não recriar a identidade visual do zero.

Utilizar o Design System existente como fonte visual para:

- cores;
- tipografia;
- espaçamentos;
- bordas;
- componentes;
- estados;
- padrões de formulário;
- navegação;
- cards;
- badges;
- botões.

O objetivo desta etapa é transformar o Design System em uma **arquitetura funcional de produto**.

---

# Objetivo

Criar no Figma a **arquitetura de navegação e os principais fluxos do sistema**, considerando três contextos de utilização:

1. Funcionário da Jotanunes;
2. Empresa fornecedora de mão de obra;
3. Empresa fornecedora de materiais.

Também deve ser produzido um primeiro fluxo completo em alta fidelidade para o perfil **Fornecedor de Materiais**.

---

# 1. Criar página `Flows / IA`

Criar uma página no arquivo Figma chamada:

`Flows / IA`

Nessa página, representar visualmente a arquitetura geral de navegação.

## Estrutura

```text
Login
 │
 ├── Jotanunes
 │    ├── Dashboard
 │    ├── Empresas
 │    ├── Obras
 │    ├── Funcionários
 │    ├── Validação
 │    ├── Pendências
 │    └── Auditoria
 │
 ├── Mão de Obra
 │    ├── Dashboard
 │    ├── Minha Empresa
 │    ├── Funcionários
 │    ├── Obras / Vínculos
 │    ├── Documentos
 │    ├── Pagamentos
 │    └── Pendências
 │
 └── Materiais
      ├── Minha Empresa
      ├── Documentos
      └── Pendências
```

Apresentar os três contextos de forma visualmente separada.

---

# 2. Estrutura de perfis

O sistema possui dois conceitos diferentes:

## Perfil de usuário

```text
Jotanunes
├── Administrador
└── Analista

Terceirizado
```

## Tipo da empresa terceirizada

```text
Empresa Terceirizada
├── Mão de Obra
└── Materiais
```

Não tratar `Mão de Obra` e `Materiais` como roles de autenticação.

Eles representam o tipo da empresa vinculada ao usuário terceirizado.

---

# 3. AppShell — Jotanunes

Criar ou refinar um shell destinado aos usuários internos.

## Sidebar

Itens:

- Dashboard
- Empresas
- Obras
- Funcionários
- Validação
- Pendências
- Auditoria

Exibir no rodapé o usuário autenticado e seu perfil:

```text
Matheus Silva
Analista
```

ou

```text
Matheus Silva
Administrador
```

---

# 4. AppShell — Mão de Obra

Criar shell específico para empresas fornecedoras de mão de obra.

## Sidebar

Itens:

- Dashboard
- Minha Empresa
- Funcionários
- Obras / Vínculos
- Documentos
- Pagamentos
- Pendências

Identificação do contexto:

```text
Empresa parceira
Mão de Obra
```

No rodapé:

```text
Fornecedor · Mão de Obra
```

---

# 5. AppShell — Materiais

Utilizar como base o shell existente:

`Shell MAT — Empresa`

Manter a navegação restrita a:

- Empresa
- Documentos
- Pendências

Identificação:

```text
Empresa parceira
Materiais
```

Rodapé:

```text
Fornecedor · Materiais
```

Não mostrar funcionalidades relacionadas a:

- funcionários;
- folha;
- salários;
- pagamentos;
- comprovantes salariais;
- vinculação de trabalhadores a obras.

Essas funcionalidades não devem aparecer nem desabilitadas.

Elas simplesmente não pertencem ao contexto desse usuário.

---

# 6. Fluxo completo — Fornecedor de Materiais

Este deve ser o primeiro fluxo funcional completo desenhado no Figma.

Fluxo:

```text
Minha Empresa
      ↓
Documentos
      ↓
Enviar documento
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
```

---

# 7. Tela — Minha Empresa

Substituir placeholders como:

`Conteúdo conforme casos de uso`

por conteúdo funcional real.

Criar uma página de dados cadastrais contendo, no mínimo:

- Razão Social
- Nome Fantasia, se aplicável
- CNPJ
- E-mail
- Telefone
- Responsável
- Tipo da empresa
- Status cadastral

Exemplo de status:

```text
Regular
Pendente
Irregular
```

Utilizar badges do Design System.

## Ações

Dependendo das permissões:

- Editar dados
- Salvar alterações

---

# 8. Tela — Documentos

Criar uma página contendo uma listagem dos documentos empresariais.

Sugestão de colunas:

| Documento | Status | Enviado em | Última atualização | Ações |
|---|---|---|---|---|

Estados possíveis:

- Pendente
- Em Análise
- Aprovado
- Rejeitado
- Vencido

Usar badges visuais coerentes para cada estado.

## Ações

Dependendo do status:

```text
Pendente
→ Enviar documento

Em análise
→ Visualizar

Aprovado
→ Visualizar

Rejeitado
→ Ver motivo
→ Reenviar

Vencido
→ Enviar nova versão
```

---

# 9. Upload de documento

Criar fluxo de upload utilizando os componentes existentes.

O usuário deverá:

1. selecionar o tipo do documento;
2. selecionar ou arrastar o arquivo;
3. visualizar o arquivo selecionado;
4. confirmar o envio.

Considerar inicialmente arquivos PDF.

## Estados

Representar:

- nenhum arquivo;
- arquivo selecionado;
- enviando;
- sucesso;
- erro.

---

# 10. Documento em análise

Após o envio, representar claramente:

```text
Status: Em análise
```

Exibir:

- nome do documento;
- versão;
- data de envio;
- arquivo enviado;
- status atual.

Evitar criar ações que permitam alterar silenciosamente um documento que já esteja sendo analisado.

---

# 11. Documento rejeitado

Quando um documento for rejeitado, a interface deve mostrar de forma clara:

- status `Rejeitado`;
- motivo informado pelo analista;
- data da análise;
- opção para reenviar.

Exemplo:

```text
Documento rejeitado

Motivo:
O documento enviado está vencido.

Analisado em:
15/09/2026 às 14:32

[Reenviar documento]
```

Não apagar ou esconder a versão anterior.

---

# 12. Reenvio

Ao reenviar um documento rejeitado:

```text
Rejeitado
   ↓
Reenviar
   ↓
Selecionar novo arquivo
   ↓
Confirmar
   ↓
Em análise
```

A interface deve transmitir que se trata de uma **nova versão** e não da edição da versão anterior.

---

# 13. Tela — Pendências

Criar uma página consolidando problemas que exigem ação do fornecedor.

Exemplos:

```text
Contrato Social
Documento pendente de envio

Certidão X
Documento rejeitado

Certidão Y
Documento vencido
```

Cada item deve permitir navegar diretamente para a ação necessária.

Exemplo:

```text
[Resolver pendência]
```

---

# 14. Estados obrigatórios das telas

Para as telas de listagem ou consulta, considerar os estados:

### Loading

Utilizar skeleton ou componente de loading do Design System.

### Empty State

Exemplo:

```text
Nenhuma pendência encontrada.

Sua empresa está com a documentação regular.
```

### Error

Exemplo:

```text
Não foi possível carregar os documentos.

[Tentar novamente]
```

### Success

Dar feedback após ações como:

- documento enviado;
- documento reenviado;
- dados atualizados.

---

# 15. Fluxo futuro — Empresa de Mão de Obra

Não é necessário detalhar todas as telas nesta etapa, mas registrar o fluxo na arquitetura.

```text
Minha Empresa
      ↓
Funcionários
      ↓
Detalhes do Funcionário
      ↓
Documentação
      ↓
Vínculo com Obra
      ↓
Pagamento
      ↓
Comprovante
```

O sistema deve considerar posteriormente:

- cadastro de funcionários;
- documentação individual;
- vinculação com obras;
- pendências documentais;
- data de pagamento;
- prazo de três dias;
- comprovantes;
- atraso de comprovantes.

---

# 16. Fluxo futuro — Funcionário Jotanunes

Também registrar no mapa de fluxos:

```text
Fila de Validação
        ↓
Selecionar Documento
        ↓
Visualizar Arquivo
        ↓
Visualizar Empresa / Funcionário
        ↓
      Analisar
        ↓
 ┌──────┴──────┐
 ↓             ↓
Aprovar      Rejeitar
                ↓
         Justificativa
                ↓
             Salvar
                ↓
            Auditoria
```

A aprovação e rejeição devem possuir estados visuais claros.

---

# 17. Regras de UX

Priorizar uma interface de sistema corporativo interno.

O produto deve ser:

- limpo;
- objetivo;
- rápido de navegar;
- orientado a tarefas;
- adequado a formulários densos;
- adequado a grandes quantidades de documentos;
- consistente entre os três tipos de usuário.

Não transformar a aplicação em um site institucional/marketing.

Evitar:

- banners decorativos;
- grandes hero sections;
- excesso de ilustrações;
- animações desnecessárias;
- excesso de espaços vazios.

---

# 18. Reutilização de componentes

Antes de criar um elemento novo, verificar se existe equivalente no Design System.

Priorizar reutilização de:

- Button
- Input
- Select
- FormField
- Link
- Badge
- Logo
- LoginCard
- AuthLayout
- AppShell
- NavItem
- PageHeader

Criar novos componentes somente quando necessário.

Possíveis novos componentes:

- DocumentTable
- DocumentStatusBadge
- UploadArea
- EmptyState
- Alert
- Modal
- ConfirmDialog
- Pagination
- FilterBar

---

# 19. Tokens

Não utilizar valores visuais arbitrários quando existir token equivalente.

Garantir consistência entre:

```text
Figma Variables
        ↕
Design Tokens
        ↕
React / CSS Variables
```

Verificar se:

- cores;
- tipografia;
- border radius;
- spacing;
- estados de interação;

estão vinculados aos tokens definidos no Design System.

Evitar valores hardcoded desnecessários.

---

# 20. Responsividade

O foco principal é desktop.

Frame principal sugerido:

```text
1280px
```

Também considerar comportamento para resoluções menores.

A aplicação deve manter:

- sidebar utilizável;
- tabelas legíveis;
- formulários acessíveis;
- ações principais visíveis.

Não é necessário criar versão mobile completa nesta etapa.

---

# 21. Organização do Figma

Organizar o arquivo em páginas semelhantes a:

```text
00 — Cover
01 — Foundations
02 — Components
03 — Flows / IA
04 — Auth
05 — Shell Jotanunes
06 — Shell Mão de Obra
07 — Shell Materiais
08 — Materials Flow
09 — Mao de Obra Flow
10 — Jotanunes Flow
```

Para esta etapa, priorizar:

```text
03 — Flows / IA
06 — Shell Mão de Obra
07 — Shell Materiais
08 — Materials Flow
```

---

# 22. Critérios de aceite

A tarefa será considerada concluída quando:

- [ ] existir arquitetura de navegação dos três contextos;
- [ ] Jotanunes possuir navegação definida;
- [ ] Mão de Obra possuir navegação definida;
- [ ] Materiais possuir navegação definida;
- [ ] Mão de Obra e Materiais não forem tratados como roles de usuário;
- [ ] o shell de Materiais não apresentar funcionalidades de funcionários ou folha;
- [ ] placeholders do shell de Materiais forem substituídos por conteúdo real;
- [ ] existir tela funcional de Minha Empresa;
- [ ] existir listagem funcional de Documentos;
- [ ] existir fluxo de upload;
- [ ] existir estado Em Análise;
- [ ] existir estado Aprovado;
- [ ] existir estado Rejeitado com justificativa;
- [ ] existir fluxo de reenvio;
- [ ] existir tela de Pendências;
- [ ] loading, error e empty states estiverem representados;
- [ ] componentes do Design System forem reutilizados;
- [ ] identidade visual permanecer consistente com JotaNunesForms;
- [ ] layout estiver pronto para implementação em React + TypeScript.

---

# Resultado esperado

Ao final desta etapa, o arquivo Figma deve deixar de representar apenas um Design System e passar a funcionar também como referência de produto.

Um desenvolvedor deve conseguir olhar o arquivo e entender:

1. quem utiliza o sistema;
2. o que cada usuário pode acessar;
3. como cada usuário navega;
4. quais telas existem;
5. como funciona o fluxo documental;
6. quais estados cada tela possui;
7. quais componentes devem ser reutilizados;
8. como implementar o primeiro fluxo funcional no frontend.

O **Fornecedor de Materiais** deve ser o primeiro fluxo completamente especificado visualmente antes da expansão para Mão de Obra e Jotanunes.

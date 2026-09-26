# Próximos passos do sistema

Este documento consolida as recomendações para os próximos ciclos do **JotaNunesForms**, considerando o estado atual do código, as specs existentes e as pendências registradas no repositório.

> Atualização de 26/09/2026: a implementação da feature 004 foi aplicada. `npm run lint` passou com avisos; Vitest/Vite estão bloqueados pelo Node 18 local (requisito: Node 22), e a validação .NET/PostgreSQL e o deploy permanecem pendentes. As mudanças e evidências disponíveis estão resumidas em [`docs/resumo-entregas.md`](./resumo-entregas.md#feature-004--endurecimento-de-acesso-26092026).

## Resumo executivo

Antes de iniciar RF17 e RF19, a recomendação é:

1. corrigir segurança, autorização e isolamento entre empresas;
2. concluir o fluxo de liberação de trabalhadores da US4;
3. fechar a decisão de produto sobre validação de comprovantes;
4. ampliar testes de integração e E2E;
5. concluir RF17, RF19 e RF20;
6. amadurecer deploy, migrations, observabilidade e documentação operacional.

O próximo marco objetivo deve ser:

> Segurança corrigida, pipeline verde em ambiente limpo e US4 completa e demonstrável de ponta a ponta.

---

## 1. Segurança e isolamento entre empresas — prioridade crítica

A feature 004 aplicou a matriz canônica de acesso, policies com fallback autenticado, identidade corrente revalidada no banco, escopos explícitos `Internal/Company`, ownership antes de leitura/download/mutação, resposta tenant genérica `404`, bloqueio dos módulos de mão de obra para Materiais e limpeza de sessão frontend em `401`. O dashboard e as consultas agregadas permanecem restritos à equipe interna. Os controles de produção também foram aplicados: validação fail-closed de JWT/CORS, Swagger somente em Development, seed demo limitado a Development e bootstrap Admin create-only.

Foram adicionados testes de matriz/inventário, isolamento A×B, efeitos colaterais, revogação de identidade, exposição de produção, logs sanitizados, consultas de autorização e frontend. A execução completa ainda não foi validada: esta máquina não tem .NET SDK nem Docker, e Node 18 não satisfaz o requisito fixado em Node 22. O lint frontend passou com avisos; TypeScript também passou, mas Vite/Vitest não carregaram neste runtime.

### Próximos passos antes de liberar o próximo deploy

- Executar em CI ou host compatível `dotnet test backend/JotaNunesForms.sln --configuration Release` com .NET 8 e Docker, estabilizando os cenários de isolamento.
- Executar `npm ci`, `npm test` e `npm run build` com Node 22.
- Revisar/rotacionar credenciais antigas no provedor, publicar em ambiente controlado e executar smoke tests de Admin, Analista, MO e Materiais.
- Confirmar `Jwt__SigningKey` aleatória de pelo menos 32 caracteres, `Cors__Origins` com origens HTTPS exatas e `BootstrapAdmin__Enabled=false` após o bootstrap inicial.

### Arquivos de referência

- `backend/src/JotaNunesForms.Api/Program.cs`
- `backend/src/JotaNunesForms.Infrastructure/Auth/AuthUserSeeder.cs`
- `backend/src/JotaNunesForms.Api/Controllers/DashboardController.cs`
- `backend/src/JotaNunesForms.Api/Controllers/ObrasController.cs`
- `backend/src/JotaNunesForms.Api/Controllers/EmpresasController.cs`
- `backend/src/JotaNunesForms.Api/Controllers/ProcessosContratacaoController.cs`

---

## 2. Concluir a liberação de trabalhadores — US4

O sistema já possui processos, checklist dinâmico, versões documentais e mobilizações. Entretanto, ainda falta concluir a regra operacional que determina se um trabalhador está efetivamente liberado para entrar na obra.

### Escopo recomendado

- Implementar movimentos de EPI:
  - entrega;
  - substituição;
  - devolução.
- Implementar o registro de integração da obra pela equipe Jotanunes.
- Validar os metadados estruturados de:
  - documento oficial com foto;
  - vínculo eSocial;
  - ASO;
  - NR-18;
  - ordem de serviço.
- Implementar `RegraLiberacaoTrabalhador`.
- Recalcular automaticamente a situação da mobilização após:
  - aprovação ou rejeição documental;
  - movimento de EPI;
  - registro ou vencimento da integração.
- Expor os itens que ainda impedem a liberação.
- Apresentar esses bloqueios claramente na interface da mobilização.

### Regra esperada

Um trabalhador somente deve ser marcado como liberado quando possuir:

- cadastro completo;
- identidade aprovada;
- vínculo comprovado;
- ASO apto e vigente;
- NR-18 aprovada e vigente;
- ordem de serviço assinada;
- ao menos uma entrega de EPI válida;
- integração da obra concluída e vigente.

### Referência

Tarefas T055–T069 em `specs/002-fluxo-documentos/tasks.md`.

---

## 3. Decisão de produto sobre comprovantes de pagamento

Existe uma divergência entre os documentos atuais:

- documentos anteriores tratam a aprovação formal de comprovantes como uma decisão de produto pendente;
- a spec do fluxo documental prevê aprovação e rejeição pelo analista na tarefa T076.

Essa decisão deve ser fechada antes da implementação da US5.

### Perguntas que precisam ser respondidas

1. O comprovante será apenas enviado e acompanhado ou deverá ser aprovado/rejeitado?
2. Se houver rejeição, o motivo será obrigatório?
3. O comprovante rejeitado poderá ser reenviado mantendo histórico de versões?
4. Quais eventos produzem cada um dos seis estados da competência?
5. Como funcionará um comprovante em lote?
6. Quais dados identificam individualmente os trabalhadores em um lote?
7. O mesmo arquivo poderá ser reutilizado em quais circunstâncias?

### Após a decisão

Executar as tarefas T070–T080 de `specs/002-fluxo-documentos/tasks.md`, incluindo:

- seis estados da competência;
- vínculo com obra e contrato;
- detecção de reuso por hash;
- tratamento de comprovantes em lote;
- pendências com contexto completo;
- bloqueio do fluxo para fornecedores de materiais.

---

## 4. Executar testes automatizados e validação E2E

A feature 004 adicionou cobertura HTTP extensa para segurança e isolamento, mas ela ainda precisa ser executada em ambiente compatível. Os cenários abaixo são os gates prioritários de validação, não trabalho de implementação pendente.

### Cenários a validar

#### Autorização e isolamento

- Administrador, Analista e Terceirizado em cada endpoint relevante.
- Terceirizado acessando recurso da própria empresa.
- Terceirizado tentando acessar recurso de outra empresa.
- Fornecedor de materiais tentando acessar funcionários, mobilizações ou pagamentos.
- Usuário não autenticado acessando endpoints protegidos.

#### Fluxos de negócio

- Processo → checklist → upload → análise → habilitação.
- Mobilização → documentos → EPI → integração → liberação.
- Rejeição → nova versão → nova análise.
- Documento vencido → bloqueio → reenvio.
- Pagamento → prazo → comprovante → situação.
- Convite → definição de senha → login → reenvio/expiração.

#### Infraestrutura

- Aplicação das migrations em PostgreSQL real.
- Upload e download utilizando um adapter fake ou ambiente dedicado de R2.
- Falha de envio de e-mail sem deixar convite utilizável.
- Inicialização sem secrets obrigatórios em produção.

### Ambiente reproduzível

- As versões já estão fixadas em `.nvmrc`, `frontend/package.json#engines` (Node 22) e `global.json` (.NET 8).
- Validar lint, testes e build a partir de uma instalação limpa usando exatamente essas versões; a CI usa Node 22 e .NET 8.
- Tratar gradualmente os avisos `react(set-state-in-effect)` existentes.

---

## 5. Concluir RF17, RF19 e RF20

Esses requisitos devem ser retomados depois que segurança e o núcleo operacional estiverem estáveis.

### RF17 — Histórico de auditoria

Implementar uma trilha append-only para eventos relevantes:

- envio inicial;
- reenvio;
- aprovação;
- rejeição;
- vencimento;
- alterações de estado relevantes.

O histórico global não deve duplicar desnecessariamente o histórico de versões e análises já existente. A auditoria deve acrescentar rastreabilidade transversal: quem fez, quando fez, qual recurso foi afetado e quais detalhes justificam a operação.

Referência: `docs/rf17-auditoria-implementacao.md`.

### RF19 — Consulta por obra

Evoluir a consulta atual para uma visão dedicada de conformidade:

- deep link para a obra;
- agrupamento por empresa terceirizada;
- trabalhadores alocados;
- situação documental resumida;
- situação de pagamentos/comprovantes;
- links para documentos e demais detalhes;
- empty state para obra sem alocações.

Essa visão deve ser interna. Terceirizados não podem consultar trabalhadores de outras empresas.

Referência: `docs/rf19-consulta-obra-implementacao.md`.

### RF20 — Dashboard gerencial

Completar o dashboard com:

- documentos rejeitados e vencidos;
- empresas e trabalhadores irregulares;
- comprovantes pendentes e em atraso;
- atalhos para Pendências, Validação, Pagamentos e Consulta por obra;
- indicadores obtidos integralmente da API.

Também deve ser removido o efeito colateral em que o dashboard consulta a fila de validação e pode mover documentos para **Em análise**. O preview precisa usar uma consulta read-only.

Referência: `docs/rf20-dashboard-parcial-implementacao.md`.

---

## 6. Operação, deploy e observabilidade

### Segurança entregue no repositório — feature 004

- Tokens novos recebem `usuario_id` e `tipo_empresa`; perfil, vínculo e tipo assinados devem corresponder ao banco em cada requisição protegida. Alteração cadastral invalida a sessão anterior e requer novo login.
- Policies declarativas e fallback protegem as rotas; o tipo Materiais não acessa módulos de mão de obra. Escopos fechados `Internal`/`Company` e ownership genérico 404 são aplicados nos casos de uso de empresas, contratos, processos, sócios, funcionários, documentos, mobilizações e pagamentos.
- Negações por tenant publicam eventos sanitizados; testes adicionados cobrem listas A×B, recursos inexistentes/alheios, ausência de efeitos colaterais no banco/storage, perfil/tipo, revogação, inventário e contagem de consulta de identidade.
- `global.json` fixa .NET 8, `.nvmrc` e `frontend/package.json` fixam Node 22. JWT/CORS de produção falham fechados, Swagger fica restrito a Development, não há seed demo produtivo e bootstrap Admin é opt-in/create-only.
- Os testes de contrato e integração PostgreSQL estão escritos, mas **não foram executados neste ambiente**: `dotnet` e Docker não estão instalados. O teste frontend também não iniciou porque o ambiente tem Node 18, enquanto o projeto agora exige Node 22. Não há evidência de build/teste verde nem de deploy.

### Ações ainda necessárias antes de produção

1. Configurar no Render chave JWT aleatória e origens CORS HTTPS exatas; confirmar bootstrap desabilitado.
2. Rotacionar as credenciais demo anteriormente documentadas e revisar acessos/logins; garantir que senhas antigas não continuem válidas.
3. Executar todos os testes/builds em CI e corrigir falhas descobertas.
4. Fazer deploy controlado, confirmar swagger indisponível, testar perfis/isolamento e avisar que sessões antigas exigirão login novamente.
5. Executar a matriz integral de `specs/004-harden-access-security/tasks.md` em CI/ambiente com .NET 8, Node 22 e Docker; corrigir eventuais falhas de compilação, migração, ownership ou contrato antes de liberar.

### Deploy e migrations

- Automatizar o deploy da API ou documentar um release checklist obrigatório.
- Evitar depender de deploy manual sem rastreamento.
- Separar a aplicação de migrations do startup normal da API.
- Validar migration e rollback antes da produção.
- Garantir que toda migration possua arquivo principal, `Designer.cs` e snapshot atualizado.
- Definir backup e restauração testada do PostgreSQL.

### Health checks e observabilidade

- Criar `/health/live` para indicar que o processo está ativo.
- Criar `/health/ready` verificando PostgreSQL e dependências essenciais.
- Monitorar falhas de R2 e e-mail sem expor secrets ou tokens.
- Adotar logs estruturados com correlation ID.
- Registrar duração e falha dos principais casos de uso.
- Configurar alertas para erros HTTP 5xx e indisponibilidade.

### Arquivos e segurança

- Validar assinatura real do PDF, não apenas extensão e `Content-Type`.
- Manter limites de tamanho por tipo de upload.
- Considerar varredura antimalware antes de disponibilizar o arquivo.
- Garantir nomes e metadados seguros ao gerar downloads.

---

## 7. Organização do backlog e documentação

O Kanban mais antigo ainda apresenta RF17 e RF19 como próximos itens, enquanto a nova spec do fluxo documental possui tarefas abertas nas US4, US5 e no polish.

### Ações recomendadas

- Escolher uma fonte de verdade para o status: `specs/`, ClickUp ou ambos com sincronização definida.
- Atualizar `docs/resumo-entregas.md` com o estado das US1–US5.
- Marcar claramente o que está:
  - implementado;
  - validado localmente;
  - validado em produção;
  - parcialmente implementado;
  - aguardando decisão de produto.
- Remover ou atualizar afirmações antigas que tratam toda a implementação como pendente.
- Não considerar uma tarefa concluída apenas porque a interface existe; exigir API, autorização, testes e cenário de aceite.

---

## Roadmap sugerido

| Etapa | Entrega | Critério de conclusão |
| --- | --- | --- |
| 0 | Segurança e autorização | Matriz de acesso testada; sem fallback/seed inseguro em produção |
| 1 | QA das US1–US3 | Quickstart executado e principais regressões automatizadas |
| 2 | US4 — liberação | Trabalhador só é liberado com todos os requisitos cumpridos |
| 3 | Decisão de comprovantes | Regra aprovada e documentada pelo produto |
| 4 | US5 — pagamentos | Seis estados, vínculo completo, hash e lote validados |
| 5 | RF17 | Auditoria append-only consultável pela equipe interna |
| 6 | RF19 | Visão de conformidade por obra, com isolamento correto |
| 7 | RF20 | Dashboard gerencial completo e sem efeitos colaterais |
| 8 | Operação | Deploy, migrations, health checks, backup e alertas consolidados |

## Definição de pronto recomendada

Uma entrega deve ser considerada concluída somente quando:

- regra de negócio implementada no domínio/aplicação;
- autorização aplicada no backend;
- isolamento entre empresas validado;
- endpoint e interface integrados;
- testes automatizados relevantes passando;
- cenário E2E executado;
- migration validada, quando aplicável;
- documentação e backlog atualizados;
- deploy e rollback conhecidos.

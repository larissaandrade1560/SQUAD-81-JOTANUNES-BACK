# Research: US4 — Checklist admissional e liberação para acesso

**Feature**: `006-checklist-admissional`  
**Date**: 2026-09-27

## 1. Fonte canônica da liberação

**Decision**: Manter uma regra pura `RegraLiberacaoTrabalhador` no Domain. Ela recebe um retrato da mobilização, itens/versões já interpretados, saldo de EPI, integração e instante de avaliação; retorna `liberado` e todos os impedimentos com códigos estáveis.

**Rationale**: A decisão precisa ser única, testável sem banco e reutilizável por mutações e consultas. A mobilização persiste somente `Aguardando` ou `Liberado`; a lista de impedimentos é derivada para não criar uma segunda fonte de verdade.

**Alternatives considered**:
- Distribuir condições entre controllers e casos de uso: rejeitado por duplicação e dificuldade de testar a tabela completa.
- Persistir cada impedimento: rejeitado porque validade e catálogo mudam e produziriam projeção obsoleta.

## 2. Históricos de EPI e integração

**Decision**: Criar entidades e tabelas append-only próprias para `MovimentoEpi` e `IntegracaoObra`, relacionadas à mobilização e ao item de checklist correspondente.

**Rationale**: Ambos são eventos operacionais com regras diferentes das versões documentais. Histórico próprio preserva entregas, substituições, devoluções e novas integrações após vencimento/refazer.

**Alternatives considered**:
- Guardar os eventos em `DocumentoVersao.CamposJson`: rejeitado por misturar movimento, formulário e arquivo e dificultar saldo/concorrência.
- Manter somente o último registro: rejeitado por perder rastreabilidade.

## 3. Semântica de EPI

**Decision**: Uma `Entrega` cria saldo ativo; `Substituicao` referencia um movimento ativo, preserva a quantidade ativa e registra a troca; `Devolucao` referencia entrega/substituição e reduz o saldo sem ultrapassá-lo. O requisito é cumprido quando existe ao menos uma entrega histórica e o saldo ativo agregado é positivo.

**Rationale**: A referência de origem evita inferir qual entrega foi devolvida ou substituída e torna a regra determinística sob vários EPIs e CAs.

**Alternatives considered**:
- Contar toda substituição como nova entrega: rejeitado porque pode inflar o saldo.
- Deduzir movimentos somente por nome/CA/data: rejeitado por ambiguidade e concorrência.

## 4. Integração vigente

**Decision**: Cada realização cria um novo registro. A integração efetiva é a mais recente concluída para a mobilização; é válida quando houve aceite, não está marcada para refazer e `ValidoAte` é nulo ou posterior ao instante avaliado. Marcar para refazer preserva o registro e o invalida.

**Rationale**: O histórico fica auditável e uma nova integração pode substituir uma anterior vencida sem apagar evidência.

**Alternatives considered**:
- Atualizar uma única linha: rejeitado por perda de histórico.
- Tratar integração como PDF: rejeitado porque a entrega é formulário interno sem arquivo obrigatório.

## 5. Cadastro e checklist

**Decision**: Tratar `MOB_CADASTRO` como evidência derivada da própria mobilização. O coordenador marca o item como cumprido quando nome/CPF do funcionário, função e vínculos obrigatórios estão presentes; ele volta a pendente se uma alteração válida puder tornar o cadastro incompleto.

**Rationale**: O formulário-pai já é a evidência do cadastro; exigir outra versão de documento manteria o primeiro item indefinidamente pendente.

**Alternatives considered**:
- Ignorar o item na regra: rejeitado porque a UI e o checklist divergiriam.
- Criar `DocumentoVersao` artificial: rejeitado por adicionar versão sem ação do usuário.

## 6. Metadados documentais

**Decision**: Manter `DocumentoVersao.CamposJson`, mas desserializar por código do catálogo em DTOs tipados e produzir JSON canônico. Validar estrutura no envio e repetir a validação semântica na aprovação contra mobilização, funcionário, empresa, função e parâmetros atuais.

**Rationale**: Aproveita versionamento e armazenamento existentes, dá erro cedo ao remetente e impede que o Analista confie em dados obsoletos ou adulterados.

**Alternatives considered**:
- Validar somente no cliente: rejeitado por segurança.
- Criar tabela para cada tipo documental: rejeitado pelo custo de schema e pouca variação esperada no MVP.

## 7. Validade e S-2190

**Decision**: Derivar prazos no backend. S-2190 fica `Preliminar` até `DataAdmissao + S2190_PRAZO_SUBSTITUICAO_DIAS`; S-2200 válido o substitui. ASO usa `ProximoExame` quando informado ou `DataExame + ASO_PERIODICIDADE_DIAS`; NR-18 usa `DataTreinamento + NR18_PERIODICIDADE_MESES`. Valores devem ser inteiros positivos e coerentes com a unidade da chave.

**Rationale**: O catálogo é a autoridade normativa e não pode ser contornado por `ValidoAte` livre do cliente ou constante em código.

**Alternatives considered**:
- Usar somente `AprovarVersaoRequest.ValidoAte`: rejeitado por permitir divergência do catálogo.
- Fixar 30, 365 e 24 meses: rejeitado por contrariar a configuração administrativa.

## 8. Recálculo, transação e tempo

**Decision**: Coordenar análise, item, histórico operacional e situação da mobilização na mesma `ITransactionalExecutor`, adquirindo lock de mobilização e item. Recalcular após aprovação/rejeição, EPI e integração; também reconciliar em `GET` de mobilização/liberação para detectar vencimentos causados apenas pelo tempo.

**Rationale**: Evita estados parciais e resolve expiração sem introduzir scheduler. Chamadas transacionais aninhadas já reutilizam a transação do EF no projeto.

**Alternatives considered**:
- Evento assíncrono: rejeitado por consistência eventual e nova infraestrutura.
- Job periódico agora: adiado porque aumenta operação e não é necessário para corrigir na primeira leitura.
- Recalcular apenas nas escritas: rejeitado porque um Liberado vencido ficaria visível até outra mutação.

## 9. Idempotência e concorrência

**Decision**: Exigir `Idempotency-Key` em registros de EPI e integração, persistida com unicidade por tipo de operação e mobilização. Mesma chave e mesmo conteúdo retorna o registro original; mesma chave com conteúdo diferente retorna `409`. Locks e validação de saldo protegem requisições concorrentes.

**Rationale**: Eventos legítimos podem ter dados iguais; chave explícita é a única forma de distinguir retry de novo evento sem heurística.

**Alternatives considered**:
- Deduplicar por data/EPI/instrutor: rejeitado porque bloquearia eventos legítimos.
- Aceitar duplicatas e apenas tornar recálculo idempotente: rejeitado porque o histórico ficaria incorreto.

## 10. Contrato HTTP e negações

**Decision**: Manter as operações sob `/api/mobilizacoes/{id}`. Consulta usa `InternalOrOwnMO`; EPI é registrado por `TerceirizadoMaoDeObra` da empresa proprietária; integração por `Interno`. Usar `401` sem identidade, `403` para papel/tipo sem capacidade, `404` genérico para inexistente ou outro tenant, `400` para validação e `409` para conflito/idempotência.

**Rationale**: Reutiliza policies e comportamento de isolamento já validados. Não existe endpoint para forçar situação Liberado.

**Alternatives considered**:
- Controller admissional separado: rejeitado por duplicar escopo e navegação.
- `403` cross-tenant: rejeitado por permitir enumeração.
- `422` para regra incompleta: rejeitado porque liberação é cálculo, não comando manual.

## 11. Experiência frontend

**Decision**: Centralizar checklist, pendências, versões, EPI e integração em `MobilizacaoPage`; ampliar `mobilizacoesService` e reutilizar `documentosVersaoService`. `FuncionarioDocumentosPage` permanece legado e não alimenta a regra.

**Rationale**: A tela já é o detalhe canônico da mobilização e usa os itens reais. Uma segunda tela criaria dois checklists divergentes. A UI exibe ações conforme o papel, mantendo a API como autoridade.

**Alternatives considered**:
- Evoluir `FuncionarioDocumentosPage`: rejeitado porque usa documentos fixos sem contexto de mobilização.
- Criar uma nova página exclusiva: rejeitado por duplicar carregamento e navegação.

## 12. Persistência, migração e rollback

**Decision**: Uma migration aditiva `AddEpiEIntegracao` cria as duas tabelas, FKs restritivas, índices por mobilização/data e unicidade de idempotência; inclui `.Designer.cs` e snapshot. Mobilizações existentes ficam Aguardando até terem evidências. Em produção, manter as tabelas no rollback e preferir reversão do código compatível ou forward fix.

**Rationale**: Não há backfill confiável para eventos que nunca foram registrados. Remover tabelas depois de uso destruiria dados de conformidade.

**Alternatives considered**:
- Preencher EPI/integração a partir de PDFs: rejeitado por ausência de fonte confiável e escopo de OCR.
- Executar `Down` em produção: rejeitado após qualquer escrita real.

## 13. Estratégia de testes

**Decision**: Cobrir a tabela da regra e os saldos no Domain; validadores, atomicidade e coordenação na Application; migrations, contrato HTTP, autorização, concorrência e isolamento com PostgreSQL Testcontainers; fluxos e papéis no Vitest; cenários 1–3 no ambiente publicado.

**Rationale**: A mudança atravessa regra crítica, dados pessoais, contratos e persistência. EF InMemory não demonstra locks, índices, transações nem comportamento PostgreSQL.

**Alternatives considered**:
- Somente testes unitários: rejeitado por não validar migration/HTTP/tenant.
- Trocar Testcontainers por provider em memória quando Docker faltar: rejeitado pela constituição; o bloqueio deve ser relatado.

## 14. Divergências existentes a corrigir na implementação

**Decision**: Padronizar `ItemChecklist.TitularId` de requisito Trabalhador como `Mobilizacao.Id` e resolver o `Funcionario.Id` através da mobilização em auditoria/validação. Corrigir também a UI que hoje oferece criação de mobilização ao Admin apesar de o endpoint permitir somente empresa de mão de obra.

**Rationale**: A criação atual já usa `Mobilizacao.Id`, que identifica unicamente obra/contrato/período do checklist. Alterar para funcionário misturaria mobilizações distintas.

**Alternatives considered**:
- Mudar `TitularId` para `Funcionario.Id`: rejeitado porque um trabalhador pode ter várias mobilizações e checklists independentes.

# Research: RF17 — Histórico de Auditoria Documental

## 1. Fonte da auditoria

**Decision**: Persistir eventos de domínio explícitos em uma tabela própria, criados pelos casos de uso que realizam a mudança documental.

**Rationale**: Os casos de uso conhecem a ação, o ator, a versão anterior, a origem e os detalhes permitidos. Isso produz uma trilha consultável e semanticamente correta.

**Alternatives considered**:

- Logs da aplicação: retenção, consulta e atomicidade não atendem à RF17.
- Interceptor automático de persistência: não possui contexto de negócio suficiente e esconderia comportamento crítico.
- Eventos pós-commit/outbox: impediria atomicidade imediata ou adicionaria processamento assíncrono desnecessário, pois estado e auditoria vivem no mesmo PostgreSQL.

## 2. Atomicidade

**Decision**: Criar uma porta de executor transacional e um adapter EF/Npgsql que execute mudança e auditoria na mesma transação, dentro da execution strategy já configurada.

**Rationale**: Os repositórios atuais chamam `SaveChangesAsync` individualmente e alguns fluxos realizam vários saves. Uma transação externa preserva os repositórios existentes e reverte todos os saves intermediários se a auditoria falhar.

**Alternatives considered**:

- Remover `SaveChangesAsync` de todos os repositórios: arquitetura mais limpa a longo prazo, porém mudança transversal desproporcional para esta feature.
- Gravar evento depois da operação: deixa janela de estado sem auditoria e viola FR-009.

## 3. Arquivos no R2

**Decision**: Manter upload antes da transação PostgreSQL e adicionar exclusão compensatória de melhor esforço se a persistência falhar.

**Rationale**: R2 não participa de transação distribuída. O arquivo só se torna visível pelo produto após o commit da referência no banco; a compensação reduz órfãos sem introduzir coordenador distribuído.

**Alternatives considered**:

- Transação distribuída: não suportada pelo conjunto atual e excessiva para o risco.
- Ignorar órfãos: mantém consistência funcional, mas acumula custo e dados sem referência.
- Upload depois do commit: pode deixar documento persistido sem arquivo disponível.

## 4. Versionamento dos documentos legados

**Decision**: Criar `DocumentoArquivoVersao` para arquivos de `DocumentoEmpresa` e `DocumentoFuncionario`, com exatamente uma origem, número crescente e uma versão vigente. A migração registra o arquivo atual como baseline, mas não cria eventos históricos.

**Rationale**: Hoje o reenvio substitui os metadados no documento raiz e perde a referência anterior. O R2 usa nova chave, mas a chave antiga deixa de ser alcançável. Uma versão explícita satisfaz FR-004 sem acoplar documentos legados a processos/checklists.

**Alternatives considered**:

- Reutilizar `DocumentoVersao`: exige `ItemChecklistId` e uma migração grande dos fluxos legados.
- Guardar somente a versão no evento: não fornece integridade/referência adequada para a versão anterior.
- Criar duas tabelas quase idênticas: aumenta duplicação; uma tabela com duas FKs opcionais e constraint “exatamente uma” preserva integridade.

## 5. Eventos retroativos

**Decision**: Não criar eventos anteriores à ativação. Somente versões-base serão geradas a partir de arquivos atuais comprováveis.

**Rationale**: Autoria e ação histórica nem sempre podem ser inferidas. Criar eventos aparentando certeza violaria FR-018. Os históricos existentes em `DocumentoVersao` e `AnaliseDocumento` permanecem disponíveis.

**Alternatives considered**:

- Inferir eventos pelas datas atuais: cria evidência potencialmente falsa.
- Ocultar todo o histórico anterior: quebra FR-019.

## 6. Imutabilidade

**Decision**: Entidade sem mutadores, repository somente de adição/consulta, nenhum endpoint de escrita e trigger PostgreSQL que rejeita `UPDATE` e `DELETE`.

**Rationale**: A proteção em camadas torna a imutabilidade verificável também contra regressões na aplicação.

**Alternatives considered**:

- Apenas omitir endpoints: não protege contra um repository futuro.
- Permissão exclusiva de banco: o aplicativo usa um único papel para todas as tabelas, exigindo alteração operacional maior.

## 7. Idempotência e concorrência

**Decision**: Usar uma chave de negócio única por transição e constraints de versão. Aprovação/rejeição exigem versão vigente e item pendente; vencimento só ocorre em `Aprovado → Vencido` e usa a versão/ciclo de validade na chave. A repetição da mesma transição confirmada retorna sucesso idempotente reutilizando o resultado existente; vencimento já processado é no-op bem-sucedido; uma transição incompatível que perdeu a concorrência retorna `409 documento_estado_conflitante`, sempre sem escrita parcial.

**Rationale**: Os fluxos atuais permitem chamadas repetidas e uploads concorrentes. Checagem apenas em memória não impede duas requisições de confirmar a mesma transição.

**Alternatives considered**:

- Somente verificar estado antes da gravação: sofre race condition.
- Somente chave única do evento sem transação: a decisão poderia ser salva antes do conflito; a transação é necessária para rollback.

## 8. Estrutura e privacidade dos dados

**Decision**: Persistir colunas tipadas e snapshots mínimos de nome/perfil, empresa e funcionário; gerar o texto de detalhes na apresentação. Não persistir CPF/CNPJ completo, e-mail, token, segredo, storage key no evento, conteúdo, hash ou request body.

**Rationale**: Colunas explícitas formam uma allowlist auditável, evitam N+1 e mantêm o evento legível após renomeação ou indisponibilidade da origem.

**Alternatives considered**:

- JSON genérico de detalhes: flexível, porém facilita vazamento e dificulta validação do contrato.
- Consultar sempre as entidades atuais: altera a leitura histórica e quebra quando a origem não existe.

## 9. Consulta e paginação

**Decision**: `GET /api/auditoria/eventos` com filtros AND, página 1-based, `pageSize` 50 por padrão/máximo 100, total e ordenação `ocorridoEm DESC, id DESC`. `de` é inclusivo e `ate` exclusivo.

**Rationale**: O produto exige total e navegação por páginas. Offset é suficiente para 100.000 eventos com índices e conjunto estável; o ID desempata instantes iguais.

**Alternatives considered**:

- Cursor/keyset: mais estável sob inserções concorrentes, mas não oferece navegação por página/total naturalmente e aumenta o contrato do MVP.
- Carregar todos os eventos: não atende desempenho nem crescimento.

## 10. Autorização e superfície HTTP

**Decision**: Uma única operação de leitura protegida pela policy `Interno`, sem endpoint de detalhe ou mutação. Atualizar a matriz canônica junto com o controller.

**Rationale**: Admin e Analista precisam da visão global; terceirizados não podem receber nem contagem. A lista já contém todos os detalhes seguros necessários.

**Alternatives considered**:

- Consulta tenant para terceirizados: contradiz o escopo aprovado.
- Endpoint mutável: viola append-only.
- URL frontend gerada pela API: acopla as camadas; a API retorna discriminador e IDs.

## 11. Vencimento

**Decision**: Extrair uma transição auditada compartilhada por listagens/pendências e pelo recálculo de processo. Manter o disparo lazy atual no MVP, mas com transação, update concorrência-seguro e evento único.

**Rationale**: Hoje vencimentos ocorrem como efeito colateral de várias consultas e podem duplicar eventos. Um serviço único elimina divergência sem introduzir scheduler.

**Alternatives considered**:

- Job dedicado: melhor evolução operacional, mas requer scheduler/monitoramento fora do escopo.
- Registrar em cada chamador: duplica regra e idempotência.

## 12. Testes e desempenho

**Decision**: Testar regras em Domain/Application e persistência, transação, trigger, concorrência e HTTP com PostgreSQL 16 real via Testcontainers. Validar 100.000 eventos sintéticos, p95 da primeira página e plano de consulta.

**Rationale**: Provider em memória não reproduz índices, constraints, trigger nem transações do PostgreSQL. O volume deriva diretamente de SC-005.

**Alternatives considered**:

- Mockar toda persistência: não prova os riscos principais.
- Executar carga em produção: introduz dados artificiais e risco operacional; a carga deve ocorrer em ambiente descartável.

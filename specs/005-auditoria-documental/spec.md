# Feature Specification: RF17 — Histórico de Auditoria Documental

**Integration Branch**: `develop` | **Spec Kit Feature ID**: `005-auditoria-documental`

**Created**: 2026-09-26

**Status**: Draft

**Input**: User description: "concluir a RF17"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Registrar toda mudança documental relevante (Priority: P1)

Como responsável da Jotanunes, quero que cada envio, reenvio, aprovação, rejeição e vencimento de documento gere um registro histórico, para que seja possível reconstruir o que aconteceu sem depender do estado atual do documento.

**Why this priority**: A trilha confiável é o objetivo central da RF17 e sustenta investigação, conformidade e responsabilização.

**Independent Test**: Executar, uma vez cada, as cinco mudanças documentais suportadas e confirmar que cada mudança concluída gera exatamente um evento com ação, data, ator, documento e contexto de origem corretos.

**Acceptance Scenarios**:

1. **Given** um documento ainda sem versão, **When** um usuário autorizado conclui o primeiro envio, **Then** um evento de envio é registrado com a versão, o ator e a origem correspondentes.
2. **Given** um documento rejeitado ou vencido, **When** um usuário autorizado conclui o reenvio, **Then** uma nova versão é preservada e um único evento de reenvio referencia a versão nova e a anterior.
3. **Given** uma versão pendente, **When** um integrante autorizado da equipe interna a aprova, **Then** um evento de aprovação registra o analista, a decisão, o comentário informado e a validade aplicável.
4. **Given** uma versão pendente, **When** um integrante autorizado da equipe interna a rejeita, **Then** um evento de rejeição registra o analista e o motivo obrigatório.
5. **Given** um documento aprovado com validade expirada, **When** sua situação muda para vencido, **Then** um único evento de vencimento é registrado, identificado como ação automática quando não houver ator humano.
6. **Given** uma alteração documental que não consegue registrar sua auditoria, **When** a operação é processada, **Then** ela não é apresentada como concluída e nenhum estado parcial fica visível.

---

### User Story 2 - Consultar e localizar eventos de auditoria (Priority: P1)

Como Administrador ou Analista da Jotanunes, quero consultar a trilha documental em ordem cronológica inversa e aplicar filtros, para localizar rapidamente uma alteração e entender seu contexto.

**Why this priority**: Registrar eventos sem permitir uma consulta eficiente não atende à necessidade operacional de investigação e acompanhamento.

**Independent Test**: Preparar eventos de empresas, documentos, escopos, ações e datas diferentes; consultar a auditoria com cada filtro isolado e combinado; confirmar resultados, ordenação, paginação e acesso à origem.

**Acceptance Scenarios**:

1. **Given** eventos existentes, **When** um Administrador ou Analista abre a auditoria sem filtros, **Then** vê a página mais recente ordenada do evento mais novo para o mais antigo.
2. **Given** eventos de períodos, ações, empresas e escopos diferentes, **When** o usuário combina filtros, **Then** somente os eventos que atendem a todos os critérios são apresentados.
3. **Given** mais eventos do que cabem em uma página, **When** o usuário navega entre páginas, **Then** não vê lacunas nem duplicidades e conhece o total de resultados.
4. **Given** um evento cuja origem ainda existe e está acessível, **When** o usuário seleciona a origem, **Then** consegue seguir para o documento ou contexto correspondente.
5. **Given** um evento cuja origem deixou de existir ou não está mais disponível, **When** o histórico é consultado, **Then** o evento continua legível e a origem é indicada como indisponível, sem quebrar a consulta.

---

### User Story 3 - Preservar uma trilha íntegra e restrita (Priority: P1)

Como responsável por conformidade, quero que a trilha seja imutável, completa e acessível somente à equipe interna autorizada, para que ela funcione como evidência confiável sem ampliar a exposição de dados pessoais.

**Why this priority**: Uma auditoria alterável, incompleta ou exposta a terceiros perde valor probatório e cria risco de privacidade.

**Independent Test**: Tentar consultar a auditoria como usuário não autenticado e terceirizado, tentar alterar ou excluir eventos, repetir uma mesma solicitação e inspecionar os dados apresentados para confirmar autorização, imutabilidade, unicidade e minimização.

**Acceptance Scenarios**:

1. **Given** um usuário não autenticado, **When** tenta consultar a auditoria, **Then** o acesso é recusado sem revelar eventos.
2. **Given** um usuário terceirizado de qualquer tipo de empresa, **When** tenta consultar a auditoria, **Then** o acesso é recusado sem revelar existência, volume ou conteúdo dos eventos.
3. **Given** um evento registrado, **When** qualquer usuário tenta alterá-lo ou excluí-lo, **Then** a operação não está disponível e o registro original permanece inalterado.
4. **Given** uma repetição idempotente da mesma mudança documental, **When** não ocorre uma nova transição de estado, **Then** nenhum evento duplicado é criado.
5. **Given** um evento consultado por usuário autorizado, **When** seus detalhes são apresentados, **Then** não contêm senha, token, segredo, conteúdo do arquivo ou identificador pessoal completo desnecessário.

### Edge Cases

- Eventos ocorridos no mesmo instante mantêm uma ordenação determinística entre páginas.
- Um intervalo com data inicial posterior à data final é rejeitado com orientação clara para correção.
- Datas são armazenadas e comparadas como instantes UTC. A interface exibe datas no fuso local informado pelo navegador e usa UTC como fallback quando esse fuso não estiver disponível.
- Empresa, funcionário, usuário ou documento renomeado, inativado ou removido não altera o retrato histórico gravado no evento.
- Um vencimento verificado repetidas vezes gera apenas o primeiro evento correspondente à transição para vencido.
- Uma tentativa negada, uma validação que falha ou um upload incompleto não gera evento de mudança documental concluída.
- Motivos e comentários aceitam até 2.000 caracteres. Valores com 2.001 ou mais caracteres são rejeitados com erro de validação, sem truncamento silencioso, evento ou estado parcial.
- A consulta sem resultados apresenta estado vazio e preserva os filtros escolhidos.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST registrar eventos para envio inicial, reenvio, aprovação, rejeição e vencimento de documentos empresariais, de funcionários e de requisitos documentais versionados.
- **FR-002**: Cada mudança documental concluída MUST produzir exatamente um evento. A repetição da mesma transição já confirmada MUST retornar sucesso idempotente reutilizando o resultado existente; vencimento já processado MUST ser no-op bem-sucedido; uma transição incompatível MUST retornar `409 documento_estado_conflitante`. Nenhum desses caminhos MUST produzir escrita ou evento adicional.
- **FR-003**: Cada evento MUST possuir identificador único, código da ação, instante de ocorrência, ator ou indicação de ação automática, referência do documento, versão envolvida, empresa e escopo documental.
- **FR-004**: Eventos de reenvio MUST identificar a nova versão e, quando existente, a versão substituída, sem sobrescrever o histórico anterior.
- **FR-005**: Eventos de aprovação MUST preservar decisão, comentário informado e validade aplicável; eventos de rejeição MUST preservar o motivo obrigatório.
- **FR-006**: Eventos automáticos de vencimento MUST ser criados somente quando ocorrer a transição efetiva para vencido e MUST NOT se repetir em verificações posteriores do mesmo estado.
- **FR-007**: O retrato de ator, empresa, funcionário e documento armazenado no evento MUST continuar legível mesmo após renomeação, inativação ou indisponibilidade do registro de origem.
- **FR-008**: Eventos registrados MUST ser append-only: o produto MUST NOT oferecer alteração ou exclusão de evento de auditoria.
- **FR-009**: A conclusão da mudança documental e o registro de seu evento MUST ser consistentes; o sistema MUST NOT confirmar a mudança quando a auditoria obrigatória não puder ser preservada.
- **FR-010**: Somente usuários autenticados com perfil Administrador ou Analista da Jotanunes MUST poder consultar a auditoria global.
- **FR-011**: Usuários não autenticados ou terceirizados MUST NOT receber dados, contagens ou indícios sobre os eventos de auditoria.
- **FR-012**: Usuários autorizados MUST poder filtrar eventos por período, ação, empresa e escopo documental, isoladamente ou em combinação.
- **FR-013**: A consulta MUST apresentar eventos do mais recente para o mais antigo, com critério determinístico de desempate.
- **FR-014**: A consulta MUST ser paginada, informar o total de resultados e preservar os filtros durante a navegação.
- **FR-015**: Cada resultado MUST mostrar data e hora, ação, origem, detalhes contextuais e responsável de forma compreensível para a equipe interna.
- **FR-016**: Quando a origem existir e o usuário tiver acesso, ele MUST poder navegar do evento para o contexto documental correspondente; origens indisponíveis MUST ser sinalizadas sem apagar o evento.
- **FR-017**: Os detalhes MUST aplicar minimização de dados e MUST NOT armazenar ou exibir senha, token, segredo, conteúdo do arquivo, corpo bruto de requisição ou identificador pessoal completo sem necessidade de negócio.
- **FR-018**: A captura MUST iniciar com a ativação da feature; o sistema MUST NOT fabricar eventos retroativos cuja autoria, data ou ação não possam ser comprovadas.
- **FR-019**: O histórico preexistente de versões e análises MUST continuar acessível pelos fluxos atuais, mesmo quando não houver evento global retroativo correspondente.
- **FR-020**: A operação de consulta da auditoria MUST integrar-se ao inventário de acesso do produto e permanecer negada por padrão para perfis não autorizados.

### Key Entities

- **Evento de Auditoria Documental**: registro imutável de uma mudança concluída, contendo ação, instante, detalhes seguros, referências e retratos históricos necessários à compreensão.
- **Referência Documental**: identifica o documento, seu escopo e a versão envolvida, incluindo a relação entre versão nova e substituída quando houver reenvio.
- **Retrato do Ator**: identificação estável e dados mínimos de apresentação do usuário responsável no instante do evento, ou indicação de processo automático.
- **Contexto de Origem**: empresa e, quando aplicável, funcionário, processo ou requisito ao qual o documento pertencia no instante da ação.
- **Critérios de Consulta**: período, ação, empresa, escopo e paginação escolhidos pelo usuário autorizado.

## Scope Boundaries

### Included

- Auditoria global de envio, reenvio, aprovação, rejeição e vencimento de documentos.
- Documentos empresariais, documentos de funcionários e documentos versionados vinculados a requisitos.
- Consulta paginada e filtrável para Administrador e Analista, com navegação para a origem quando disponível.
- Preservação da trilha e dos retratos históricos após mudanças nos registros de origem.

### Excluded

- Exportação da auditoria em CSV ou outros formatos.
- Auditoria de pagamentos e comprovantes de pagamento.
- Eventos administrativos de usuários, classificação genérica ou validações não documentais.
- Timeline de auditoria embutida no detalhe de cada documento; o incremento entrega a consulta global e mantém os históricos de versão já existentes.
- Reconstrução automática de eventos anteriores à ativação da feature.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% das mudanças documentais suportadas e concluídas nos cenários de aceite produzem exatamente um evento com ação, instante, ator e origem corretos.
- **SC-002**: 0 eventos registrados podem ser alterados ou excluídos por funcionalidades do produto.
- **SC-003**: 100% das tentativas de consulta por usuário não autenticado ou terceirizado são recusadas sem retornar eventos ou contagens.
- **SC-004**: Administradores e Analistas localizam um evento por período, ação, empresa ou escopo em até 3 minutos, sem apoio técnico.
- **SC-005**: A primeira página de uma consulta com até 100.000 eventos disponíveis é apresentada em até 3 segundos em condições operacionais normais.
- **SC-006**: Paginação repetida sobre um conjunto estável apresenta 0 eventos duplicados ou omitidos.
- **SC-007**: Inspeção dos eventos e das respostas de consulta encontra 0 ocorrências de senhas, tokens, segredos, conteúdo de arquivos ou identificadores pessoais completos desnecessários.
- **SC-008**: Em falha simulada de gravação da auditoria, 100% das mudanças associadas permanecem não concluídas, sem estado parcial visível.

## Assumptions

- Administrador e Analista possuem o mesmo direito de consulta global; nenhuma consulta de auditoria é oferecida a terceirizados neste incremento.
- Os fluxos atuais já identificam o usuário autenticado, a empresa e o documento envolvidos em cada ação.
- A captura inicia após a implantação da feature. Dados antigos não serão convertidos em eventos sem evidência confiável, mas históricos de versões e análises existentes serão preservados.
- O histórico é mantido por prazo indeterminado neste incremento; eventual política corporativa de retenção ou descarte exigirá decisão de produto e mudança governada posterior.
- O horário canônico do evento é UTC e independente do fuso de exibição; a interface o apresenta no fuso local informado pelo navegador, com fallback UTC, sem alterar o instante registrado.
- A consulta global é suficiente para o MVP; exportação, timeline por documento e novos domínios de auditoria serão incrementos separados.

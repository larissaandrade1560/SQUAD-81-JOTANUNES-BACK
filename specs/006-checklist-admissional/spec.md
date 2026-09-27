# Feature Specification: US4 — Checklist admissional e liberação para acesso

**Integration Branch**: `develop` | **Spec Kit Feature ID**: `006-checklist-admissional`

**Created**: 2026-09-27

**Status**: Draft

**Input**: User description: "Comece a implementação do US4-Checklist admissional e liberado para acesso. Essa tarefa está no ClickUp, com suas respectivas subtasks."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Determinar a liberação do trabalhador (Priority: P1)

Como responsável pela segurança da obra, quero que a situação da mobilização reflita automaticamente o cumprimento integral do checklist admissional, para que somente trabalhadores aptos e com documentação válida sejam liberados para acesso.

**Why this priority**: Esta é a regra central da US4 e impede acesso indevido ao canteiro. Os demais fluxos fornecem os dados usados por essa decisão.

**Independent Test**: Preparar uma mobilização com cadastro completo, identidade aprovada, vínculo válido, ASO apto e vigente, NR-18 vigente, ordem de serviço assinada, EPI entregue e integração concluída; confirmar a liberação automática e, depois, invalidar um item para confirmar o retorno a Aguardando.

**Acceptance Scenarios**:

1. **Given** uma mobilização em Aguardando com todos os requisitos admissionais cumpridos, **When** o último requisito é aprovado ou registrado, **Then** a situação muda automaticamente para Liberado para acesso.
2. **Given** uma mobilização em Aguardando com ao menos um requisito ausente, rejeitado, vencido ou inválido, **When** sua liberação é avaliada, **Then** ela permanece Aguardando e cada impedimento é informado.
3. **Given** uma mobilização Liberada, **When** um requisito obrigatório vence, é rejeitado ou passa a exigir nova realização, **Then** a situação retorna automaticamente para Aguardando e o motivo é apresentado.
4. **Given** um processo de contratação Qualificado, **When** a mobilização do trabalhador ainda não cumpre o checklist admissional, **Then** o trabalhador permanece Aguardando.
5. **Given** uma mobilização Afastada ou Desmobilizada, **When** os requisitos admissionais são reavaliados, **Then** essa avaliação não promove a mobilização para Liberado.

---

### User Story 2 - Validar os documentos admissionais (Priority: P1)

Como Analista da Jotanunes, quero analisar os dados essenciais dos documentos admissionais, para decidir com segurança se identidade, vínculo, saúde ocupacional, capacitação e ciência das regras da obra estão válidos.

**Why this priority**: A liberação depende de evidências estruturadas e verificáveis; aprovar apenas um arquivo sem conferir seus dados não demonstra conformidade.

**Independent Test**: Enviar e analisar, separadamente, um documento oficial, um comprovante eSocial, um ASO, um certificado NR-18 e uma ordem de serviço, verificando um caso válido e ao menos um caso inválido de cada requisito.

**Acceptance Scenarios**:

1. **Given** RG, CIN ou CNH legível, **When** nome e CPF correspondem ao trabalhador, **Then** um único documento oficial com foto pode cumprir o requisito de identidade.
2. **Given** um comprovante S-2200, **When** empregador, trabalhador, função, admissão e situação do vínculo correspondem à mobilização, **Then** o vínculo é considerado comprovado.
3. **Given** um comprovante S-2190 aceito, **When** o S-2200 ainda não foi apresentado, **Then** o vínculo é marcado como preliminar e permanece válido somente durante o prazo configurado para substituição.
4. **Given** um ASO com identificação, função, riscos, exame, profissional responsável, registro, conclusão e validação, **When** a conclusão é apto ou contém restrição compatível registrada pelo Analista, **Then** o requisito pode ser aprovado sem exigir exames clínicos, laudos ou resultados médicos anexos.
5. **Given** um ASO com restrição incompatível com a função, **When** ele é analisado, **Then** o requisito não permite a liberação.
6. **Given** um certificado NR-18, **When** seus dados e validade atendem aos parâmetros vigentes do catálogo, **Then** o requisito é aprovado.
7. **Given** uma ordem de serviço assinada, **When** ela cobre atividades, riscos, medidas preventivas, proibições, emergência, EPI, data e ciência do trabalhador, **Then** o requisito é aprovado.
8. **Given** metadados obrigatórios ausentes ou incompatíveis com a mobilização, **When** o documento é enviado ou analisado, **Then** a operação é recusada com indicação dos campos que precisam ser corrigidos.

---

### User Story 3 - Registrar e acompanhar movimentos de EPI (Priority: P1)

Como responsável da empresa de mão de obra, quero registrar entregas, substituições e devoluções de EPI por trabalhador, para manter o histórico de fornecimento e atender ao requisito de liberação.

**Why this priority**: O EPI representa um histórico operacional, e não um documento único que vence. Sem ao menos uma entrega efetiva, o trabalhador não pode ser liberado.

**Independent Test**: Registrar uma entrega, uma substituição e uma devolução, consultar o histórico em ordem cronológica e confirmar como cada sequência afeta a liberação.

**Acceptance Scenarios**:

1. **Given** uma mobilização da própria empresa, **When** um responsável autorizado registra a entrega de EPI com quantidade positiva, CA, data, orientação, responsabilidade de guarda e aceite, **Then** o movimento é preservado no histórico e a liberação é reavaliada.
2. **Given** uma entrega anterior, **When** uma substituição é registrada, **Then** ambos os movimentos permanecem consultáveis.
3. **Given** que todos os EPIs entregues foram devolvidos e não houve nova entrega, **When** a liberação é reavaliada, **Then** o requisito de EPI fica pendente.
4. **Given** uma empresa de materiais ou uma empresa de mão de obra diferente da responsável pela mobilização, **When** tenta registrar ou consultar movimentos protegidos, **Then** o acesso é recusado sem expor os dados do trabalhador.

---

### User Story 4 - Registrar a integração da obra (Priority: P1)

Como Administrador ou Analista da Jotanunes, quero registrar a integração realizada pelo trabalhador na obra, para concluir o último controle interno necessário à liberação sem exigir upload de arquivo.

**Why this priority**: A integração é um controle exclusivo da Jotanunes e o último bloqueio típico do fluxo de acesso.

**Independent Test**: Com todos os demais requisitos cumpridos, registrar a integração como usuário interno e confirmar a liberação; marcar a integração para refazer e confirmar o bloqueio.

**Acceptance Scenarios**:

1. **Given** uma mobilização com os demais requisitos cumpridos, **When** um Administrador ou Analista registra data e hora, conteúdo, instrutor, aceite do trabalhador e os dados opcionais aplicáveis, **Then** a integração é concluída e a liberação é reavaliada sem exigir arquivo.
2. **Given** uma integração válida, **When** ela vence ou é marcada para refazer, **Then** o trabalhador retorna a Aguardando até realizar nova integração válida.
3. **Given** um usuário terceirizado, **When** tenta registrar ou alterar a integração, **Then** o acesso é recusado.

---

### User Story 5 - Visualizar pendências para liberação (Priority: P2)

Como usuário envolvido na mobilização, quero ver a situação real do trabalhador e a lista de requisitos que ainda impedem o acesso, para corrigir pendências sem depender de suporte.

**Why this priority**: A decisão automática precisa ser explicável para que a empresa e a equipe interna consigam agir sobre os bloqueios.

**Independent Test**: Abrir uma mobilização incompleta, conferir a lista de pendências, cumprir os requisitos um a um e confirmar que cada pendência desaparece até a situação mudar para Liberado.

**Acceptance Scenarios**:

1. **Given** uma mobilização incompleta, **When** um usuário autorizado consulta a liberação, **Then** vê se o trabalhador está liberado e recebe uma lista de códigos e motivos para todos os requisitos pendentes.
2. **Given** uma pendência corrigida, **When** a mobilização é consultada novamente, **Then** o item correspondente deixa de aparecer sem ocultar outras pendências.
3. **Given** todos os requisitos cumpridos, **When** a mobilização é consultada, **Then** a lista de pendências está vazia e a situação exibida é Liberado para acesso.
4. **Given** um usuário de empresa de mão de obra, **When** consulta uma mobilização da própria empresa, **Then** vê somente os dados necessários para tratar suas pendências.
5. **Given** um usuário não autenticado, uma empresa diferente ou um fornecedor de materiais, **When** tenta consultar a liberação, **Then** o acesso é recusado sem revelar a existência ou os dados da mobilização.

### Edge Cases

- O S-2190 não é substituído pelo S-2200 dentro do prazo configurado: o vínculo preliminar expira e bloqueia ou reverte a liberação.
- O prazo de substituição do S-2190 ou os parâmetros da NR-18 são alterados no catálogo: avaliações posteriores usam os parâmetros vigentes, sem prazo fixo independente.
- O ASO ou a NR-18 vence depois da liberação: a mobilização volta para Aguardando quando a regra é reavaliada.
- O ASO registra restrição compatível: o Analista pode aprová-lo com a restrição registrada; restrição incompatível bloqueia a liberação.
- Uma integração vence ou é marcada para refazer: ela deixa de cumprir o requisito até novo registro válido.
- Há entrega, substituição e devolução de EPI: o histórico completo é preservado e a regra considera se resta uma entrega válida, sem atribuir vencimento ao conjunto como documento.
- O mesmo evento que conclui um requisito é repetido: o histórico não deve ganhar registros indevidos nem produzir uma transição de situação duplicada.
- Uma falha ocorre durante o registro de EPI, integração ou análise documental: nenhum estado parcial é apresentado como concluído e a situação anterior da mobilização é preservada.
- A mobilização está Afastada ou Desmobilizada: o cumprimento documental não substitui a transição operacional necessária para reativá-la.
- A consulta contém mobilização inexistente ou fora do escopo da empresa: a resposta não permite distinguir recurso ausente de recurso pertencente a outra empresa.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST manter cada mobilização em Aguardando enquanto qualquer requisito obrigatório de liberação estiver ausente, pendente, rejeitado, vencido ou inválido.
- **FR-002**: O sistema MUST definir Liberado para acesso somente quando cadastro, identidade, vínculo, ASO, NR-18, ordem de serviço, EPI e integração estiverem simultaneamente válidos.
- **FR-003**: A qualificação do processo de contratação MUST NOT liberar automaticamente qualquer trabalhador.
- **FR-004**: O sistema MUST recalcular a situação da mobilização após aprovação ou rejeição admissional, registro de movimento de EPI e registro, vencimento ou marcação para refazer da integração.
- **FR-005**: O sistema MUST reverter uma mobilização Liberada para Aguardando quando um requisito obrigatório deixar de ser válido.
- **FR-006**: A avaliação admissional MUST NOT promover automaticamente mobilizações Afastadas ou Desmobilizadas.
- **FR-007**: O sistema MUST aceitar RG, CIN ou CNH como documento oficial com foto e validar legibilidade, nome e CPF contra o titular da mobilização.
- **FR-008**: O sistema MUST considerar o S-2200 válido somente quando empregador, trabalhador, função ou categoria, data de admissão e situação do vínculo forem compatíveis com a mobilização.
- **FR-009**: O sistema MUST aceitar o S-2190 como vínculo preliminar durante o prazo de substituição configurado e MUST bloqueá-lo quando esse prazo expirar sem S-2200 válido.
- **FR-010**: O sistema MUST NOT exigir fotografia de CTPS física para comprovar vínculo no escopo desta feature.
- **FR-011**: O sistema MUST registrar no ASO os dados necessários para identificar trabalhador, empresa, função, riscos, exame, profissional responsável, registro profissional, conclusão, atividades específicas, validação e próximo exame quando aplicável.
- **FR-012**: O sistema MUST aceitar ASO sem exames clínicos, laudos ou resultados médicos anexos e MUST bloquear ASO sem médico ou registro profissional.
- **FR-013**: O sistema MUST permitir aprovação de ASO com restrição compatível registrada pelo Analista e MUST bloquear restrição incompatível com a função.
- **FR-014**: O sistema MUST validar NR-18 com identificação, treinamento, conteúdo, carga horária, data, local, instrutor, responsável técnico, assinatura e validade segundo parâmetros do catálogo.
- **FR-015**: O sistema MUST validar que a ordem de serviço assinada cubra atividades, riscos, medidas preventivas, proibições, emergência, uso de EPI, data e ciência do trabalhador.
- **FR-016**: O sistema MUST recusar o envio ou a aprovação quando metadados admissionais obrigatórios estiverem ausentes ou incompatíveis, informando os campos inválidos.
- **FR-017**: O sistema MUST manter movimentos de EPI dos tipos entrega, substituição e devolução, com EPI, quantidade, CA, data, orientação de uso, responsabilidade de guarda e aceite.
- **FR-018**: O requisito de EPI MUST exigir ao menos uma entrega válida e MUST ficar pendente quando todas as entregas tiverem sido devolvidas sem nova entrega.
- **FR-019**: O histórico de EPI MUST NOT vencer como um documento único e MUST preservar todos os movimentos registrados.
- **FR-020**: O sistema MUST permitir que somente usuário autorizado da empresa de mão de obra responsável registre movimentos de EPI da mobilização.
- **FR-021**: O sistema MUST permitir que somente Administrador ou Analista da Jotanunes registre a integração da obra.
- **FR-022**: A integração MUST registrar obra, mobilização, data e hora, conteúdo, instrutor, aceite do trabalhador, avaliação quando houver, validade quando aplicável e indicação de necessidade de refazer, sem arquivo obrigatório.
- **FR-023**: Integração vencida ou marcada para refazer MUST bloquear a liberação até que nova integração válida seja registrada.
- **FR-024**: Usuários autorizados MUST consultar o resultado da liberação com indicador de liberado e lista completa de impedimentos, cada um com código estável e motivo compreensível.
- **FR-025**: Usuários internos MUST consultar qualquer mobilização; usuários da empresa de mão de obra MUST acessar somente mobilizações da própria empresa; fornecedores de materiais MUST NOT acessar fluxos de mobilização.
- **FR-026**: Tentativas sem autenticação MUST ser recusadas; tentativas com papel ou tipo de empresa insuficiente MUST ser recusadas; tentativas entre empresas MUST ocultar a existência do recurso.
- **FR-027**: Respostas, histórico operacional e registros de diagnóstico MUST minimizar dados pessoais e MUST NOT expor conteúdo documental, CPF completo, credenciais ou outros dados desnecessários.
- **FR-028**: Operações que alteram requisito ou situação MUST ser concluídas de forma integral, sem apresentar registro parcial ou liberação inconsistente em caso de falha.
- **FR-029**: A interface da mobilização MUST apresentar o checklist admissional real, a situação atual, o histórico de EPI, o registro de integração permitido ao papel e os impedimentos restantes em um único fluxo coerente.

### Key Entities

- **Mobilização**: vínculo do trabalhador com empresa, contrato e obra; mantém função, período, situação operacional e a referência dos requisitos admissionais.
- **Item admissional**: requisito aplicável à mobilização, com código, obrigatoriedade, situação, validade e evidências analisadas.
- **Versão admissional**: arquivo ou registro estruturado submetido para um item, com metadados informados, decisão, validade e histórico.
- **Movimento de EPI**: evento imutável de entrega, substituição ou devolução associado à mobilização, incluindo identificação do EPI, quantidade, CA, data, orientação, responsabilidade e aceite.
- **Integração da obra**: realização registrada pela Jotanunes para uma mobilização e obra, com conteúdo, instrutor, aceite, avaliação, validade e necessidade de refazer.
- **Resultado de liberação**: avaliação atual da mobilização, contendo a decisão e todos os impedimentos que precisam ser resolvidos.
- **Parâmetro normativo**: valor administrável que define prazos e critérios variáveis, incluindo substituição do S-2190 e requisitos de NR-18.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em 100% dos cenários de aceite com um ou mais requisitos inválidos, a mobilização permanece ou retorna a Aguardando e todos os impedimentos presentes são exibidos.
- **SC-002**: Em 100% dos cenários de aceite com os oito requisitos válidos, a mobilização muda para Liberado para acesso sem ação manual adicional após o último registro ou aprovação.
- **SC-003**: Um usuário autorizado identifica o que falta para liberar uma mobilização em até 1 minuto a partir da tela da mobilização.
- **SC-004**: Cada um dos cinco tipos documentais estruturados possui ao menos um cenário válido e um inválido aceitos corretamente em uma bateria de homologação.
- **SC-005**: Em 100% das tentativas de acesso sem autenticação, entre empresas ou por fornecedor de materiais, nenhum dado da mobilização, documento, EPI ou integração é exposto.
- **SC-006**: Uma entrega de EPI e uma integração válidas aparecem no histórico e afetam a decisão de liberação na primeira consulta realizada após a conclusão da operação.
- **SC-007**: Vencimento de ASO, NR-18, S-2190 preliminar ou integração, e a marcação de integração para refazer, revertem corretamente a liberação em todos os cenários correspondentes da homologação.
- **SC-008**: Os cenários de bloqueio sem integração, liberação completa e reversão por ASO vencido são concluídos no ambiente publicado sem ocorrência de recurso de liberação indisponível.
- **SC-009**: Pelo menos 90% dos usuários de homologação completam o fluxo principal de identificar pendências e registrar a ação permitida ao seu papel na primeira tentativa, sem suporte técnico.

## Assumptions

- A US3 já fornece a mobilização em Aguardando, vinculada à empresa de mão de obra, ao contrato e à obra, com checklist admissional criado.
- Os itens padrão são cadastro, documento oficial com foto, vínculo eSocial, ASO admissional, NR-18, ordem de serviço, EPI e integração; o catálogo pode acrescentar requisitos por função, risco ou obra.
- O prazo inicial para substituir S-2190 por S-2200 é de 30 dias corridos, mantido como parâmetro administrável.
- Carga horária e periodicidade da NR-18, assim como validade do ASO, seguem parâmetros do catálogo e regras aplicáveis à função.
- Campos estruturados são informados no envio ou na análise; leitura automática de documentos não integra esta entrega.
- Integração é um registro interno da Jotanunes e não exige arquivo; EPI é histórico de movimentos e não um documento com vencimento global.
- Rejeições continuam exigindo motivo e novas versões preservam o histórico já existente.
- Autenticação, papéis Administrador e Analista, isolamento entre empresas e vínculo do usuário terceirizado com sua empresa já existem e serão reutilizados.
- Pagamentos, OCR, fotografia de CTPS, exames ou laudos clínicos, FGTS ou DCTFWeb mensal, afastamento, desmobilização e reativação operacional permanecem fora do escopo.
- A entrega depende da conclusão ordenada das subtarefas de persistência de EPI e integração, regra de liberação, validadores, experiência completa de uso e homologação no ambiente publicado.

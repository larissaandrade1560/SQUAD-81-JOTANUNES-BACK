# Feature Specification: Endurecimento de acesso e isolamento

**Feature ID**: `004-harden-access-security` — branch de trabalho ainda não criada

**Created**: 2026-09-26

**Status**: Draft

**Input**: User description: "Comece com a melhora na segurança apresentada, priorizando autorização, isolamento entre empresas e configuração segura de produção."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Isolar os dados de cada empresa (Priority: P1)

Como usuário de uma empresa terceirizada, quero visualizar e alterar somente os dados pertencentes à minha empresa, para que informações de outras parceiras não sejam expostas ou modificadas indevidamente.

**Why this priority**: O sistema concentra documentos empresariais, dados de trabalhadores, pagamentos e informações de obras. Uma falha de isolamento pode expor dados pessoais, trabalhistas e comerciais entre empresas distintas.

**Independent Test**: Criar duas empresas com usuários e dados próprios, autenticar-se como uma delas e tentar consultar ou alterar, por listagem e por identificador direto, os recursos da outra. O teste entrega valor quando nenhum dado da outra empresa é retornado ou alterado.

**Acceptance Scenarios**:

1. **Given** duas empresas terceirizadas com dados próprios, **When** o usuário da empresa A consulta empresas, funcionários, documentos, processos, mobilizações ou pagamentos, **Then** recebe somente os dados que sua função permite e que pertencem à empresa A.
2. **Given** um identificador válido de um recurso da empresa B, **When** o usuário da empresa A tenta acessá-lo diretamente, **Then** o sistema nega a operação sem confirmar a existência ou revelar dados do recurso.
3. **Given** um usuário terceirizado, **When** ele tenta consultar indicadores globais ou alocações agregadas de todas as empresas em uma obra, **Then** o sistema nega o acesso.
4. **Given** um usuário terceirizado autorizado sobre um recurso da própria empresa, **When** realiza uma operação prevista para seu perfil, **Then** a operação continua disponível sem regressão funcional.
5. **Given** um fornecedor de materiais, **When** tenta utilizar funcionalidades exclusivas de mão de obra, **Then** o sistema nega a operação mesmo que o usuário informe manualmente um endereço ou identificador válido.

---

### User Story 2 - Aplicar menor privilégio por perfil (Priority: P1)

Como responsável pelo sistema, quero que Administrador, Analista e Terceirizado tenham somente as permissões necessárias às suas atribuições, para reduzir acessos indevidos e erros operacionais.

**Why this priority**: Estar autenticado não deve conceder acesso geral. As responsabilidades dos três perfis são diferentes e precisam ser aplicadas de forma consistente em todas as operações.

**Independent Test**: Executar uma matriz de operações com os três perfis e verificar que cada ação administrativa, analítica e operacional é permitida ou negada conforme a responsabilidade definida.

**Acceptance Scenarios**:

1. **Given** um Administrador ativo, **When** gerencia cadastros, configurações e usuários dentro das funções previstas, **Then** o sistema permite a operação.
2. **Given** um Analista ativo, **When** consulta dados gerenciais ou analisa documentação, **Then** o sistema permite a operação.
3. **Given** um Analista, **When** tenta executar uma ação reservada ao Administrador, **Then** o sistema nega a operação.
4. **Given** um Terceirizado, **When** tenta executar validação documental, auditoria, gestão de usuários ou consulta gerencial global, **Then** o sistema nega a operação.
5. **Given** um usuário inativo ou associado a uma empresa inativa, **When** tenta iniciar ou continuar uma sessão protegida, **Then** o acesso é recusado.
6. **Given** uma operação protegida, **When** é solicitada sem autenticação válida ou com sessão expirada, **Then** o sistema não executa a operação e orienta a nova autenticação sem revelar dados protegidos.

---

### User Story 3 - Iniciar produção somente com configuração segura (Priority: P1)

Como responsável pela operação, quero impedir que o sistema entre em produção com segredos de exemplo, credenciais padrão ou recursos de diagnóstico expostos, para evitar uma implantação vulnerável por configuração incorreta.

**Why this priority**: Uma configuração insegura pode comprometer todo o controle de acesso, mesmo quando as regras funcionais de autorização estão corretas.

**Independent Test**: Tentar iniciar o ambiente de produção sem os valores de segurança obrigatórios, com valores conhecidos de desenvolvimento e com criação automática de contas padrão. O sistema deve recusar todas essas situações e aceitar somente uma configuração explicitamente segura.

**Acceptance Scenarios**:

1. **Given** um ambiente de produção sem o segredo obrigatório para validar sessões, **When** a inicialização é solicitada, **Then** o sistema não fica disponível e informa qual configuração obrigatória está ausente sem exibir segredos.
2. **Given** um ambiente de produção configurado com valor de exemplo conhecido, **When** a inicialização é solicitada, **Then** o sistema recusa a configuração.
3. **Given** um banco de produção vazio, **When** o sistema inicia normalmente, **Then** nenhuma conta com credencial padrão conhecida é criada automaticamente.
4. **Given** a necessidade excepcional de criar o primeiro administrador, **When** o procedimento autorizado é executado, **Then** ele exige uma credencial exclusiva fornecida para aquele ambiente e deixa a operação rastreável.
5. **Given** um ambiente de produção, **When** uma pessoa não autorizada tenta acessar documentação interativa ou recursos de diagnóstico, **Then** o acesso é negado.
6. **Given** uma aplicação web hospedada em origem não autorizada, **When** tenta realizar requisições pelo navegador, **Then** o sistema não aceita a origem.

---

### User Story 4 - Verificar e sustentar a política de acesso (Priority: P2)

Como equipe responsável pela manutenção, quero uma referência verificável das permissões e evidências automatizadas de que elas continuam válidas, para prevenir regressões de segurança quando novos módulos forem criados.

**Why this priority**: O sistema cresce por requisitos funcionais independentes. Sem uma regra transversal verificável, novos fluxos podem voltar a conceder acesso apenas por o usuário estar autenticado.

**Independent Test**: Revisar a matriz de acesso, executar seus cenários automatizados e confirmar que uma nova operação protegida não pode ser considerada pronta sem perfil, escopo e comportamento de negação definidos.

**Acceptance Scenarios**:

1. **Given** o catálogo atual de operações protegidas, **When** a equipe consulta a matriz de acesso, **Then** identifica para cada operação os perfis permitidos, o escopo dos dados e o resultado esperado para acessos negados.
2. **Given** uma alteração em uma operação protegida, **When** a validação automatizada é executada, **Then** qualquer ampliação acidental de acesso causa falha verificável antes da entrega.
3. **Given** uma tentativa de acesso negada, **When** a equipe consulta os registros operacionais, **Then** consegue identificar data, categoria da operação e contexto de segurança suficiente para investigação, sem encontrar senhas, tokens ou conteúdo documental sensível.

### Edge Cases

- O usuário pertence a uma empresa que foi desativada após a sessão ter sido iniciada.
- O perfil do usuário é alterado enquanto existe uma sessão ativa.
- Um recurso muda de vínculo ou é desativado entre a consulta e a alteração.
- Um identificador tem formato válido, mas aponta para recurso inexistente ou de outra empresa.
- Um identificador inexistente e um identificador pertencente a outro tenant percorrem o mesmo fluxo de autorização e produzem o mesmo status e formato de resposta; a implementação evita consultas ou mensagens que revelem explicitamente o vínculo externo, sem prometer igualdade exata de latência.
- Uma empresa de materiais possui dados antigos de funcionários ou pagamentos criados antes do endurecimento.
- Uma origem autorizada muda de domínio ou utiliza endereço de preview temporário.
- Uma configuração obrigatória existe, mas contém valor vazio, conhecido, fraco ou de exemplo.
- A inicialização excepcional do primeiro administrador é solicitada mais de uma vez.
- O sistema recebe uma sessão expirada, adulterada, emitida para outro ambiente ou associada a usuário inativo.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST manter uma relação explícita das operações públicas; qualquer operação não classificada como pública MUST exigir autenticação válida.
- **FR-002**: As operações públicas MUST se limitar ao acesso inicial, conclusão de convite válido e verificação mínima de disponibilidade, sem exposição de dados de negócio.
- **FR-003**: O sistema MUST aplicar permissões por perfil em todas as operações protegidas, independentemente do que estiver visível na interface do usuário.
- **FR-004**: O sistema MUST restringir usuários terceirizados aos recursos pertencentes à empresa associada à sua identidade.
- **FR-005**: A restrição de empresa MUST ser aplicada em listagens, detalhes, downloads, alterações e operações acionadas por identificador direto.
- **FR-006**: Tentativas de um terceirizado acessar recurso de outra empresa MUST ser negadas sem revelar se o recurso existe.
- **FR-007**: Indicadores globais, pendências globais, validação documental, auditoria e consultas agregadas por obra MUST ser acessíveis somente à equipe interna autorizada.
- **FR-008**: O Administrador MUST poder realizar as operações administrativas previstas pelo produto, incluindo gestão de cadastros e acessos.
- **FR-009**: O Analista MUST poder consultar informações internas e analisar documentos, mas MUST NOT realizar operações reservadas ao Administrador.
- **FR-010**: O Terceirizado MUST NOT validar documentos, gerir usuários internos, consultar auditoria global ou realizar operações administrativas.
- **FR-011**: Fornecedores de materiais MUST NOT acessar cadastro de trabalhadores, mobilização, folha, pagamento salarial ou comprovantes de trabalhadores.
- **FR-012**: Usuários inativos e usuários vinculados a empresas inativas MUST ter acesso protegido recusado, inclusive quando possuírem uma sessão emitida anteriormente.
- **FR-013**: Sessões expiradas, adulteradas ou emitidas para outro ambiente MUST ser recusadas em todas as operações protegidas.
- **FR-014**: O ambiente de produção MUST recusar inicialização quando qualquer segredo obrigatório de autenticação estiver ausente, vazio ou corresponder a valor conhecido de desenvolvimento.
- **FR-015**: O ambiente de produção MUST NOT criar automaticamente usuários com documentos ou senhas padrão conhecidas.
- **FR-016**: Um procedimento excepcional de bootstrap MAY criar o primeiro Administrador somente quando explicitamente habilitado. A credencial MUST ser fornecida exclusivamente por configuração segura do ambiente, possuir pelo menos 16 caracteres, não corresponder a valores conhecidos de desenvolvimento e não possuir valor padrão ou fallback. Cada tentativa MUST gerar evento sanitizado de sucesso, recusa ou erro, sem registrar documento completo, senha ou hash.
- **FR-017**: O procedimento excepcional de bootstrap MUST NOT recriar, reativar ou redefinir silenciosamente uma conta administrativa existente.
- **FR-018**: Recursos de diagnóstico e documentação interativa MUST permanecer indisponíveis ao público em produção.
- **FR-019**: Requisições originadas por aplicações web MUST ser aceitas somente de origens explicitamente autorizadas para o ambiente.
- **FR-020**: O sistema MUST registrar tentativas relevantes de acesso negado com data, categoria da operação, perfil e contexto suficiente para investigação.
- **FR-021**: Registros de segurança MUST NOT conter senhas, segredos, tokens de sessão, tokens de convite ou conteúdo de documentos enviados.
- **FR-022**: O sistema MUST manter uma matriz canônica com uma entrada para cada combinação de método HTTP e rota protegida, informando perfis permitidos, escopo dos dados e, para cada cenário de autenticação, autorização ou limite de tenant, o status HTTP e o comportamento esperado ou sua não aplicabilidade.
- **FR-023**: A validação da matriz MUST cobrir acesso permitido, ausência de autenticação, perfil insuficiente, empresa incorreta, usuário inativo e empresa inativa.
- **FR-024**: Novas operações protegidas MUST ter perfil, escopo e comportamento de negação definidos antes de serem consideradas prontas para entrega.
- **FR-025**: O endurecimento MUST preservar os fluxos legítimos já disponíveis aos usuários autorizados de cada perfil.
- **FR-026**: Mensagens de negação MUST ser compreensíveis para o usuário legítimo sem revelar dados internos, existência de recursos de terceiros ou detalhes de configuração.
- **FR-027**: Cada requisição protegida MUST realizar no máximo uma consulta adicional para revalidar a identidade corrente. A quantidade de consultas de autorização em listagens MUST permanecer constante, independentemente da quantidade de itens retornados.

### Key Entities

- **Identidade autenticada**: representa o usuário reconhecido, seu perfil, situação de atividade e eventual empresa associada.
- **Empresa associada**: delimita o conjunto de recursos que um usuário terceirizado pode consultar ou alterar.
- **Política de acesso**: descreve quais perfis podem executar uma operação e qual escopo de dados se aplica.
- **Operação protegida**: ação de consulta ou alteração que exige autenticação, perfil e, quando aplicável, vínculo com a empresa.
- **Configuração de segurança do ambiente**: conjunto de valores obrigatórios que permite ao ambiente iniciar de forma segura, sem armazenar seus valores na especificação.
- **Evento de segurança**: registro sanitizado de tentativa negada ou procedimento excepcional relevante para investigação.

## Scope Boundaries

### Included

- Autorização por perfil.
- Isolamento de dados entre empresas terceirizadas.
- Bloqueio de módulos de mão de obra para fornecedores de materiais.
- Validação da situação do usuário e da empresa durante operações protegidas.
- Inicialização segura do ambiente de produção.
- Restrição de recursos de diagnóstico e origens web.
- Matriz de acesso e validação automatizada de regressões.
- Registro sanitizado de eventos de segurança relevantes.

### Out of Scope

- Substituição do mecanismo atual de autenticação por login social ou provedor externo.
- Autenticação multifator.
- Recuperação pública de senha.
- Redesenho visual das telas existentes.
- Auditoria funcional completa de alterações documentais prevista no RF17.
- Criptografia ou anonimização adicional dos dados já persistidos.
- Mudança das responsabilidades de negócio definidas para Administrador, Analista e Terceirizado.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% das operações protegidas existentes estão classificadas na matriz de acesso com perfil, escopo e comportamento de negação definidos.
- **SC-002**: Em uma bateria com pelo menos duas empresas distintas, nenhuma tentativa de consulta, download ou alteração cruzada revela ou modifica recursos da outra empresa.
- **SC-003**: 100% das ações reservadas ao Administrador são recusadas para Analista e Terceirizado nos cenários de aceite aplicáveis.
- **SC-004**: 100% das consultas gerenciais globais são recusadas para usuários terceirizados.
- **SC-005**: O ambiente de produção recusa 100% dos cenários de inicialização com segredo ausente, vazio ou de desenvolvimento conhecido.
- **SC-006**: Nenhuma conta com credencial padrão conhecida é criada durante uma inicialização normal de produção.
- **SC-007**: 100% dos cenários críticos de autorização — sem sessão, perfil insuficiente, outra empresa, usuário inativo e empresa inativa — possuem validação repetível e passam antes da entrega.
- **SC-008**: Os fluxos legítimos selecionados para regressão de Administrador, Analista e Terceirizado continuam sendo concluídos com sucesso após o endurecimento.
- **SC-009**: Para qualquer operação descoberta em runtime, a matriz permite localizar diretamente, por método HTTP e rota, os perfis permitidos, o escopo, o status HTTP e o comportamento de autenticação, autorização e limite de tenant, sem depender da leitura do código-fonte.
- **SC-010**: Uma inspeção dos eventos de segurança gerados nos cenários de aceite encontra zero senhas, segredos, tokens ou conteúdo documental sensível.
- **SC-011**: Os testes de integração confirmam uma única consulta de identidade por requisição protegida e ausência de crescimento linear das consultas de autorização nas listagens cobertas.

## Assumptions

- Os perfis existentes — Administrador, Analista e Terceirizado — serão mantidos.
- Administradores possuem gestão ampla; Analistas realizam consultas internas e validação documental; Terceirizados operam somente os dados da própria empresa.
- A listagem básica de obras ativas pode continuar disponível ao terceirizado quando necessária para seus vínculos, mas alocações agregadas e dados de outras empresas permanecem restritos à equipe interna.
- Para impedir enumeração, uma tentativa de acesso direto a recurso de outra empresa terá comportamento indistinguível de um recurso indisponível para aquele usuário.
- Login, conclusão de convite válido e verificação mínima de disponibilidade continuarão públicos; dados de negócio continuarão protegidos.
- O mecanismo de autenticação existente será preservado; esta feature endurece sua configuração e aplicação de permissões.
- Contas de demonstração poderão existir apenas em ambientes não produtivos ou em ambientes isolados destinados explicitamente a testes.
- A configuração das origens autorizadas será específica por ambiente e controlada pela equipe de operação.

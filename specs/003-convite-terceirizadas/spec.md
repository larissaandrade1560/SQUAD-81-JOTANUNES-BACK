# Feature Specification: Fluxo de convite de empresas terceirizadas

**Feature Branch**: `003-convite-terceirizadas`

**Created**: 2026-09-26

**Status**: Draft

**Input**: User description: "preciso criar um fluxo de convite para as empresas terceirizadas, onde o analista ou o administrador convida a empresa terceirizada, chega notificação no email dessa empresa, e um link para ela mudar a senha. O login dessa empresa deverá ser feito após mudar a senha com: email que recebeu o link, e a senha que ela escolheu ao alterar."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Convidar a empresa e disparar o e-mail (Priority: P1)

O Administrador ou o Analista da Jotanunes seleciona uma empresa terceirizada já cadastrada e envia um convite de acesso. A empresa recebe, no e-mail informado no convite, uma notificação de que foi convidada e um link para definir a senha.

**Why this priority**: Sem o convite e o e-mail, a empresa não consegue entrar no portal. É o ponto de entrada do fluxo.

**Independent Test**: Convidar uma empresa de teste com um e-mail conhecido e confirmar que o convite fica pendente e que a mensagem chega com o link, sem a empresa ainda conseguir entrar no sistema.

**Acceptance Scenarios**:

1. **Given** uma empresa terceirizada cadastrada e um Administrador autenticado, **When** ele envia o convite informando o e-mail da empresa, **Then** o sistema registra o convite como pendente e dispara a notificação para aquele e-mail com o link para definir a senha.
2. **Given** uma empresa terceirizada cadastrada e um Analista autenticado, **When** ele envia o convite da mesma forma, **Then** o convite é registrado e o e-mail é enviado — o Analista não precisa ser Administrador para convidar.
3. **Given** o e-mail enviado, **When** o destinatário abre a mensagem, **Then** ela identifica a Jotanunes, informa que foi criado um acesso ao Portal de Terceirizadas, mostra o nome da empresa, contém um único botão/link “Definir minha senha”, informa validade de 48 horas e que a mensagem pode ser ignorada se o convite não for reconhecido.
4. **Given** empresa sem e-mail de contato válido, **When** o convite é enviado sem um e-mail preenchido, **Then** o sistema recusa o envio e pede um e-mail válido antes de concluir.
5. **Given** um usuário com perfil Terceirizado, **When** tenta enviar convite, **Then** a ação é recusada.
6. **Given** falha no disparo da notificação, **When** o convite é solicitado, **Then** a equipe vê erro para tentar de novo e o acesso **não** fica como convite enviado/pendente utilizável.

---

### User Story 2 - Definir a senha pelo link do convite (Priority: P1)

A pessoa responsável na empresa abre o link recebido por e-mail e escolhe uma senha nova. Só depois disso o acesso da empresa passa a existir de fato.

**Why this priority**: O convite só entrega valor quando a empresa consegue concluir a senha sem suporte da Jotanunes.

**Independent Test**: Abrir um link de convite válido, definir senha e confirmação, e verificar que o convite deixa de estar pendente — sem usar a tela de login ainda.

**Acceptance Scenarios**:

1. **Given** um link de convite válido e ainda não usado, **When** a empresa informa uma senha nova e a confirmação igual, **Then** a senha é gravada, o convite é concluído e a empresa é orientada a entrar no sistema com o e-mail que recebeu o convite e a senha escolhida.
2. **Given** senha e confirmação diferentes, **When** a empresa tenta concluir, **Then** o sistema recusa e pede que as duas senhas coincidam; o convite permanece utilizável.
3. **Given** senha que não atende à política mínima (tamanho e complexidade já usados no cadastro interno), **When** a empresa tenta concluir, **Then** o sistema recusa com mensagem clara e o link continua válido.
4. **Given** um link já utilizado com sucesso, **When** alguém o abre novamente, **Then** o sistema informa que o convite não é mais válido e não permite definir outra senha por aquele link.
5. **Given** um link expirado ou inválido, **When** a empresa o abre, **Then** vê uma mensagem de que o convite expirou ou é inválido e que deve solicitar um novo convite à Jotanunes; nenhuma senha é alterada.

---

### User Story 3 - Entrar com o e-mail do convite e a senha escolhida (Priority: P1)

Depois de definir a senha, a empresa entra no portal usando exatamente o e-mail que recebeu o link e a senha que escolheu. Antes de concluir essa etapa, o login da empresa é recusado.

**Why this priority**: É o resultado de negócio do fluxo: a empresa passa a operar no sistema com as credenciais que ela mesma definiu.

**Independent Test**: Tentar entrar antes de definir a senha (deve falhar); definir a senha; entrar com o e-mail do convite e a senha escolhida e ver o acesso de terceirizada daquela empresa.

**Acceptance Scenarios**:

1. **Given** convite ainda pendente (senha não definida), **When** alguém tenta entrar com o e-mail do convite e qualquer senha, **Then** o acesso é recusado e não há sessão da empresa.
2. **Given** convite concluído, **When** a empresa informa o mesmo e-mail que recebeu o link e a senha escolhida, **Then** entra no sistema no perfil de terceirizada, vinculada à empresa convidada.
3. **Given** convite concluído, **When** informa o e-mail correto e senha errada, **Then** o acesso é recusado com mensagem genérica (sem revelar se o e-mail existe).
4. **Given** convite concluído, **When** informa um e-mail diferente do que recebeu o convite, **Then** o acesso daquela empresa não é concedido.
5. **Given** Administrador ou Analista, **When** entra com a identificação já usada internamente, **Then** o acesso interno continua funcionando — o login por e-mail aplica-se à empresa terceirizada, não à equipe Jotanunes.

---

### User Story 4 - Reenviar convite e acompanhar a situação (Priority: P2)

Se o e-mail não chegou, o link expirou ou a empresa precisa redefinir a senha, o Administrador ou o Analista reenvia o convite. Internamente, a equipe vê se o acesso está pendente, ativo ou com convite expirado.

**Why this priority**: Evita suporte manual e desbloqueia empresas que perderam o e-mail, sem ser necessário para o primeiro caminho feliz.

**Independent Test**: Deixar um convite expirar (ou marcá-lo como expirado em ambiente de teste), reenviar, e concluir o novo link; o link anterior deixa de funcionar.

**Acceptance Scenarios**:

1. **Given** convite pendente ou expirado, **When** Administrador ou Analista reenvia o convite, **Then** um novo e-mail é disparado com um novo link; o link anterior deixa de valer.
2. **Given** empresa com acesso já ativo, **When** a equipe Jotanunes envia um novo convite para alterar a senha, **Then** a empresa recebe um novo link; até concluir o novo link, o acesso com a senha atual continua válido; ao concluir, passa a valer somente a senha nova.
3. **Given** um convite, **When** a equipe consulta a empresa ou a gestão de acessos, **Then** vê a situação: pendente, ativo ou convite expirado, e o e-mail usado no convite.
4. **Given** o mesmo e-mail já usado como acesso de outra empresa, **When** um convite novo tenta reutilizar esse e-mail, **Then** o sistema recusa e pede um e-mail exclusivo.

---

### Edge Cases

- Empresa inativa ou descredenciada: o convite não é enviado; se o acesso já existia, o login da empresa é recusado até a empresa voltar a ficar apta segundo as regras cadastrais vigentes.
- Reenvio em sequência: cada reenvio invalida o link anterior; apenas o mais recente vale.
- E-mail com maiúsculas/minúsculas diferentes: o endereço é tratado de forma equivalente (a empresa entra com o mesmo e-mail, independentemente da capitalização).
- Empresa de mão de obra e empresa de materiais: ambas podem ser convidadas; o convite não altera a classificação nem as funcionalidades de cada tipo.
- Pessoa da empresa que clica no link em outro aparelho ou navegador: o link continua válido até expirar ou ser usado, sem exigir que quem convidou esteja presente.
- Tentativa de usar o link depois que a senha já foi definida por outro colaborador da mesma empresa: o link original não vale mais; é preciso novo convite para alterar a senha.
- Equipe interna tentando entrar com e-mail no lugar da identificação interna (ou o contrário): cada público usa o identificador do seu tipo de acesso; a recusa não revela detalhes da conta.
- Convite iniciado e e-mail de destino alterado no cadastro da empresa antes do envio: vale o e-mail confirmado no momento do envio do convite, que será o identificador de login.
- Falha ao entregar a notificação: o convite não é tratado como enviado; a equipe pode repetir a ação.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST permitir que Administrador e Analista convidem uma empresa terceirizada já cadastrada, informando o e-mail que receberá o acesso.
- **FR-002**: O e-mail do convite MUST ser o identificador de login daquela empresa após a conclusão da senha.
- **FR-003**: O sistema MUST enviar ao e-mail informado uma notificação de convite contendo um link para definir ou alterar a senha.
- **FR-004**: O convite MUST permanecer pendente até a empresa concluir a definição de senha pelo link válido.
- **FR-005**: Enquanto o convite estiver pendente, a empresa MUST NOT conseguir entrar no sistema com aquele e-mail.
- **FR-006**: Ao abrir um link válido, a empresa MUST conseguir definir uma senha e confirmá-la; senhas diferentes ou fora da política mínima MUST ser recusadas sem invalidar o link.
- **FR-007**: Após definir a senha com sucesso, a empresa MUST entrar somente com o e-mail que recebeu o link e a senha escolhida, no perfil Terceirizado vinculado àquela empresa.
- **FR-008**: Link expirado, inválido ou já utilizado MUST impedir nova definição de senha e exibir orientação para solicitar outro convite.
- **FR-009**: O prazo de validade do link MUST ser de 48 horas a partir do envio; após esse prazo o convite fica expirado.
- **FR-010**: Administrador e Analista MUST poder reenviar o convite; o reenvio MUST invalidar o link anterior e disparar um novo e-mail.
- **FR-011**: Se a empresa já tiver acesso ativo, um novo convite MUST permitir alterar a senha; a senha anterior MUST continuar válida até a nova ser concluída.
- **FR-012**: O e-mail de acesso MUST ser exclusivo entre empresas; o sistema MUST recusar convite com e-mail já associado a outra empresa.
- **FR-013**: Usuário Terceirizado MUST NOT convidar empresas nem reenviar convites.
- **FR-014**: O sistema MUST recusar convite sem e-mail válido (formato de e-mail reconhecível).
- **FR-015**: Equipe Jotanunes (Administrador e Analista) MUST continuar entrando com a identificação já utilizada internamente; este fluxo MUST NOT exigir e-mail para o login interno.
- **FR-016**: Não MUST existir cadastro público, autoinscrição nem “esqueci minha senha” aberto na tela de entrada; a (re)definição de senha da empresa ocorre só pelo convite enviado pela Jotanunes.
- **FR-017**: A equipe Jotanunes MUST consultar a situação do acesso da empresa (pendente, ativo, convite expirado) e o e-mail associado.
- **FR-018**: O convite MUST vincular o acesso à empresa convidada; a sessão da terceirizada MUST operar apenas sobre os dados daquela empresa.
- **FR-019**: Senhas MUST NOT ser enviadas em texto no e-mail; o e-mail contém o link, não a senha escolhida.
- **FR-020**: Cada empresa MUST possuir no máximo um acesso de login por este fluxo (um e-mail de entrada por empresa). Criar vários logins distintos para a mesma empresa fica fora deste incremento.
- **FR-021**: A notificação MUST identificar a Jotanunes, citar o Portal de Terceirizadas, a razão social da empresa, o CTA “Definir minha senha”, a validade de 48 horas e a orientação para ignorar a mensagem se o convite não for reconhecido. MUST haver versão em texto simples além da versão formatada.
- **FR-022**: Se o disparo da notificação falhar, o sistema MUST informar a equipe e MUST NOT deixar um convite utilizável como se tivesse sido enviado.

### Key Entities

- **Empresa terceirizada**: Pessoa jurídica já cadastrada (mão de obra ou materiais), destinatária do convite e titular do acesso.
- **Convite de acesso**: Solicitação enviada por Administrador ou Analista, com e-mail de destino, situação (pendente, concluído/ativo, expirado) e prazo do link.
- **Acesso da empresa**: Credencial da terceirizada após a senha ser definida: e-mail do convite, senha escolhida e vínculo com a empresa.
- **Link de definição de senha**: Endereço de uso único, com validade, que permite definir ou alterar a senha sem estar autenticada.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em ambiente controlado, 100% dos convites enviados com e-mail válido geram notificação recebida pelo destinatário em até 5 minutos, contendo o link de senha.
- **SC-002**: Uma empresa consegue, no primeiro contato, abrir o e-mail, definir a senha e entrar no sistema em até 5 minutos, sem ligar para o suporte da Jotanunes.
- **SC-003**: 100% das tentativas de entrada com o e-mail do convite antes de definir a senha são recusadas.
- **SC-004**: Após a senha definida, 100% das entradas com o e-mail do convite e a senha correta concedem o acesso daquela empresa; senha incorreta ou e-mail diferente não concedem.
- **SC-005**: 100% dos links após 48 horas ou já utilizados não permitem nova definição de senha.
- **SC-006**: Analista e Administrador conseguem concluir o envio de um convite em até 2 minutos a partir do cadastro da empresa, sem permissão extra além do próprio perfil.
- **SC-007**: Pelo menos 90% das empresas de um lote de aceite concluem a definição de senha no primeiro link válido, sem reenvio.
- **SC-008**: Login da equipe interna permanece inalterado: contas de Administrador e Analista já existentes continuam entrando com a identificação anterior, em verificação de regressão do caminho de entrada.

## Assumptions

- A empresa já está cadastrada (razão social, CNPJ, classificação). Este fluxo não cria a empresa; só concede acesso ao portal.
- O e-mail do convite é informado ou confirmado no momento do envio; quando a empresa já tiver e-mail de contato, ele pode ser sugerido, mas o valor enviado é o que vale como login.
- Há um acesso por empresa (e-mail + senha). Colaboradores da mesma empresa compartilham esse acesso neste incremento.
- “Mudar a senha” no primeiro convite significa definir a senha inicial; a empresa não recebe senha provisória no e-mail.
- Política mínima de senha reutiliza a já aplicada a usuários internos (mínimo de 6 caracteres).
- Validade do link: 48 horas, alinhada a convites corporativos usuais e ao estado “convite expirado” previsto na gestão de acessos.
- Não há autoatendimento de recuperação de senha na tela de entrada; perda de senha ou de e-mail resolve-se com novo convite pela Jotanunes.
- Perfis internos (Administrador e Analista) não passam a usar e-mail neste incremento.
- Ambos os tipos de empresa (mão de obra e materiais) podem ser convidados; o que cada uma pode fazer no portal continua determinado pela classificação já existente.
- Notificação é exclusivamente por e-mail (versão formatada + texto simples); não há canal obrigatório dentro do sistema para a empresa ainda sem acesso.
- Contas de terceirizada criadas anteriormente só com documento/senha de teste não são o caminho de produção; o acesso oficial da empresa passa a ser o deste convite.
- O Analista pode convidar e reenviar convite, mas a gestão de contas internas da Jotanunes permanece restrita ao Administrador (fora deste incremento).

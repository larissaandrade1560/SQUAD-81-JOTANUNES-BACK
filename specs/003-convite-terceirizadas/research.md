# Research: Fluxo de convite de empresas terceirizadas

**Feature**: `003-convite-terceirizadas`  
**Date**: 2026-09-26

## 1. Envio de e-mail: porta + MailKit (SMTP substituível)

**Decision**: Porta de domínio (Application/Domain não conhecem provedor):

```csharp
Task SendAsync(string to, string subject, string textBody, string? htmlBody, CancellationToken cancellationToken);
```

Infrastructure:

- `MailKitEmailSender` — `MimeMessage` + `BodyBuilder` (texto + HTML) + `MailKit.Net.Smtp.SmtpClient` (`ConnectAsync` / `AuthenticateAsync` / `SendAsync` / `DisconnectAsync`). `IOptions<EmailOptions>`. **Nenhum** host, user ou nome de provedor hardcoded (incluindo Resend).
- `LoggingEmailSender` — quando SMTP **não** está configurado **e** o ambiente é `Development`: loga destinatário, assunto e o conteúdo necessário ao teste local; o **link com token MAY aparecer só em Development**. Qualquer outro ambiente MUST NOT logar o token.
- Produção sem SMTP: não usar log como “envio”; o send MUST falhar de forma visível.

Opções (`Email__*` / seção `Email`), padrão R2:

| Chave | Papel |
|-------|--------|
| `Email__Host` | Host SMTP |
| `Email__Port` | Porta (padrão 587) |
| `Email__User` | Usuário SMTP |
| `Email__Password` | Senha/API key — **nunca** no git |
| `Email__From` | Remetente verificado |
| `Email__UseStartTls` | `true` → `SecureSocketOptions.StartTls` |

`EmailOptions.IsConfigured`: Host e From preenchidos e Port > 0.

URL do link: `App__PublicBaseUrl` → `{App:PublicBaseUrl}/definir-senha?token={token}`. Sem domínio hardcoded.

**Produção (Resend como SMTP, não SDK)**: `Host=smtp.resend.com`, `Port=587`, `User=resend`, `Password=<RESEND_API_KEY>`, `From=<e-mail do domínio verificado>`, `UseStartTls=true`. A aplicação trata o Resend como qualquer SMTP substituível. **Não** adicionar pacote/SDK Resend.

**DI** (só Infrastructure; use cases não escolhem sender):

```text
Email:IsConfigured     → MailKitEmailSender
Development && !SMTP   → LoggingEmailSender
Production && !SMTP    → falha no envio (não silenciar)
```

**Rationale**: Constituição: Domain MUST NOT depender de SMTP. MailKit é o cliente SMTP atual em .NET; `System.Net.Mail.SmtpClient` está obsoleto. SMTP genérico cobre Resend, Mailtrap e Mailpit sem segunda dependência de provedor.

**Alternatives considered**:
- SDK Resend (`resend-dotnet`): rejeitado — acopla a um fornecedor; o pedido é SMTP substituível.
- ASP.NET Identity `IEmailSender`: puxa Identity; FR-016 proíbe auto-cadastro.
- Logar o link em produção: vaza o segredo do convite.

## 2. Token do link (não Identity)

**Decision**: 32 bytes via `RandomNumberGenerator`, encode Base64URL no query `token`. Persistir **SHA-256** (hex) em `ConviteAcesso.TokenHash`. Validade **48 h** (`ExpiraEm`). Uso único (`UsadoEm`). Reenvio: `InvalidadoEm` no convite vigente não usado; novo registro. Lookup: hash do token recebido, match em convite não invalidado, não usado, não expirado. Token plaintext existe **somente** na memória no momento de montar o link do e-mail.

**Rationale**: FR-008/FR-009/FR-010/FR-019. Token em texto só viaja no e-mail; o banco não guarda o segredo reversível.

**Alternatives considered**:
- `DataProtectorTokenProvider` do Identity: acopla Identity e complica API stateless no Render.
- JWT no link: revogar/reenviar exige denylist — a tabela de convite já faz isso.
- Token em texto no banco: dump libera todos os convites.

## 3. Um usuário Terceirizado por empresa

**Decision**: Convite **reutiliza** o `Usuario` perfil Terceirizado já vinculado à empresa, se existir; senão cria um com `Documento` = CNPJ da empresa, `NomeExibicao` = razão social, `Email` normalizado (trim + lowercase), `Ativo = false`, `PasswordHash` nulo. Índice único filtrado: um Terceirizado por `empresa_id`; e-mail único quando não nulo. Convite **não** usa `UsuarioEmpresaRules` atual (hoje bloqueia Materiais): MO e Materiais podem ser convidadas. CRUD `POST /api/usuarios` permanece Admin-only e fora deste incremento para provisionar terceirizada.

**Rationale**: FR-020; contas de teste com CPF; spec de materiais.

**Alternatives considered**:
- Sempre criar usuário novo com CNPJ: quebra empresas que já têm usuário CPF.
- Vários usuários por empresa (JN-08 completo): fora do incremento.
- Manter bloqueio “só MO”: contradiz a spec.

## 4. Login dual sem quebrar o contrato interno

**Decision**: `LoginRequest` continua com `documento` + `senha`. O campo `documento` passa a ser **identificador**: se contém `@`, busca por e-mail; senão, `Usuario.NormalizeDocumento`. Terceirizado **com e-mail e senha já definidos** MUST autenticar só por e-mail. Terceirizado legado sem e-mail continua por documento. Internos inalterados. JWT `sub` permanece `Documento`. Pendente (`PasswordHash` nulo ou `Ativo == false` no primeiro convite) falha com mensagem genérica. Empresa `Ativo == false`: login recusado.

**Rationale**: FR-007, FR-015, SC-008.

**Alternatives considered**:
- Campo novo obrigatório `identificador`: quebra testes existentes.
- Dois endpoints de login: UX pior.
- Aceitar CNPJ após convite: contradiz FR-007.

## 5. Autorização do convite (Admin e Analista)

**Decision**: Política `Interno` (`perfil` Administrador ou Analista). `POST /api/empresas/{id}/convite` com essa política. Terceirizado 403. Endpoints de token em `AuthController` com `[AllowAnonymous]`. Frontend: `EmpresasPage`; rota pública `/definir-senha`. `/usuarios` permanece Admin.

**Rationale**: FR-001, FR-013.

**Alternatives considered**:
- Abrir `UsuariosController` ao Analista: expõe CRUD interno.
- Só Admin convida: contradiz a spec.

## 6. Redefinição com acesso já ativo (FR-011)

**Decision**: Reconvite em usuário já ativo: gera novo `ConviteAcesso` sem desativar o usuário nem apagar `PasswordHash`. Login com senha atual segue válido até `DefinirSenha` no novo token. Primeiro convite: `Ativo = false` até definir senha.

**Rationale**: FR-005 vs FR-011.

**Alternatives considered**:
- `Ativo = false` no reset: derruba a empresa até clicar no e-mail.
- Senha provisória no e-mail: FR-019.

## 7. Situação exibida (derivada)

**Decision**: Não persistir enum de situação. Derivar:
- **Pendente** — convite não usado, não invalidado, não expirado, e o usuário ainda não tem senha **após envio bem-sucedido**.
- **Convite expirado** — último convite não usado com `ExpiraEm` no passado, sem senha.
- **Ativo** — usuário tem senha e `Ativo`; reconvite pendente não muda o rótulo.
- Sem convite — nenhum convite válido / envio falhou (invalidado).

**Rationale**: FR-017, FR-022.

**Alternatives considered**:
- Coluna `situacao` atualizada por job: fácil dessincronizar.

## 8. Template HTML + texto (Application)

**Decision**: `ConviteEmailComposer` na Application monta assunto, `textBody` e `htmlBody` (FR-021). MailKit só despacha. Conteúdo: identificação Jotanunes; Portal de Terceirizadas; razão social; CTA “Definir minha senha” apontando para o link com token; validade 48 h; ignorar se não reconhecer o convite. Sem senha no corpo.

**Rationale**: Cliente de e-mail sem HTML ainda lê o texto; o botão no HTML usa o mesmo URL.

**Alternatives considered**:
- Só texto: pior UX, ainda atende o link.
- Template na Infrastructure: misturaria copy de produto com SMTP.

## 9. Falha de envio (FR-022)

**Decision**: Ordem no use case:

1. Validar.
2. Gerar token em memória; persistir usuário + `ConviteAcesso` (**só** `TokenHash`).
3. `SaveChanges`.
4. `IEmailSender.SendAsync`.
5. Se o send falhar: `InvalidadoEm` no convite, `SaveChanges`, lançar erro de aplicação (sem token/credenciais no log). HTTP 503 com mensagem para tentar de novo. O convite **não** permanece utilizável (não é “Pendente”).

Não enviar o e-mail **antes** de persistir: se o save falhasse depois, o link do e-mail não existiria no banco.

Log de erro no adapter: destinatário + assunto + tipo/mensagem da falha; MUST NOT incluir token, `Email__Password` nem API key.

**Rationale**: FR-022; convite “Pendente” sem e-mail engana a equipe.

**Alternatives considered**:
- Enviar antes de persistir: link órfão.
- Deixar Pendente após falha: contradiz FR-022.
- Engolir a exceção do SMTP: envio silencioso falso.

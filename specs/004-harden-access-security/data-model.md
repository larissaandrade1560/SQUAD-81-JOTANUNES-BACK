# Data Model: Endurecimento de acesso e isolamento

Não há novas tabelas. O modelo descreve objetos em memória e regras sobre `Usuario` e `Empresa` existentes.

## CurrentIdentity

| Campo | Regra |
| --- | --- |
| UsuarioId | obrigatório; confirmado no banco |
| Perfil | valor persistido atual |
| EmpresaId | obrigatório para Terceirizado |
| TipoEmpresa | obrigatório para Terceirizado |
| UsuarioAtivo | deve ser verdadeiro |
| EmpresaAtiva | deve ser verdadeiro para Terceirizado |

## AccessScope

- `Internal(CurrentIdentity)`: somente Administrador/Analista, dentro da policy da operação.
- `Company(CurrentIdentity, EmpresaId, TipoEmpresa)`: somente Terceirizado da mesma empresa.

Não existe estado nulo/global implícito. Operação MO exige `TipoEmpresa=MaoDeObra`.

## Security options

### JwtOptions

`Issuer`, `Audience` e `SigningKey` obrigatórios; chave mínima de 32 bytes e não conhecida; `ExpirationMinutes` positivo e limitado.

### CorsOptions

`AllowedOrigins` é lista produtiva não vazia de origens HTTP/HTTPS absolutas e exatas; sem wildcard, path, query, fragmento ou localhost.

### BootstrapAdminOptions

`Enabled=false` por padrão. Quando habilitado, documento, nome e senha são obrigatórios e sem fallback. A senha possui pelo menos 16 caracteres, é fornecida somente por configuração segura do ambiente e não pode corresponder a valor conhecido de desenvolvimento.

Transições: desligado não age; habilitado+válido+sem admin cria uma vez; admin existente recusa sem alterar; inválido aborta startup. Toda tentativa emite resultado sanitizado (`Succeeded`, `RefusedExistingAdmin` ou `InvalidConfiguration`) sem documento completo, senha ou hash.

## SecurityEvent

Permitidos: `EventId`, `ReasonCode`, data, método, route pattern, `UsuarioId?`, `Perfil?`, `EmpresaId?`, status e `TraceId`.

Proibidos: senha/hash, bearer, convite, headers completos, body, query sensível, CPF/CNPJ, nomes e conteúdo/nome de arquivo.

`ISecurityEventSink`, definido na Application, recebe somente o modelo sanitizado. A API/Infrastructure implementa a porta com logging estruturado; `AccessScopeGuard` não depende de ASP.NET nem de uma biblioteca concreta de logs.

## Ownership

| Família | Caminho |
| --- | --- |
| Empresa/Sócio | `EmpresaId` |
| Funcionário/documento | `Funcionario.EmpresaId` |
| Documento empresa | `DocumentoEmpresa.EmpresaId` |
| Contrato | `Contrato.EmpresaId` |
| Processo/checklist/versão | `Processo → Contrato → EmpresaId` |
| Mobilização | `Mobilizacao → Funcionario → EmpresaId` |
| Pagamento/comprovante | `Pagamento → Funcionario → EmpresaId` |

Ownership é verificado antes de mapear, baixar ou persistir.

## Negação

- `Unauthenticated`/`InactiveIdentity`/claims divergentes → `401`.
- `InsufficientRole`/`CompanyTypeDenied` → `403`.
- `TenantResourceMissingOrForeign` → `404` genérico.

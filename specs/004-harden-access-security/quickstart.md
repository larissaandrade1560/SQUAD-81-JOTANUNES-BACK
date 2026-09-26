# Quickstart: validar endurecimento de acesso

## Pré-requisitos

- .NET SDK 8 e Docker ativo para PostgreSQL efêmero.
- Node.js 22.
- Nenhum segredo real em fixtures ou logs.

Contratos: [matriz canônica](./contracts/access-matrix.json), [leitura da matriz](./contracts/access-matrix.md), [HTTP](./contracts/http-security.md), [produção](./contracts/production-security.md).

## 1. Suítes

```bash
dotnet test backend/JotaNunesForms.sln --configuration Release
npm --prefix frontend test
npm --prefix frontend run build
```

Sem Docker, a suíte HTTP deve falhar com pré-requisito claro, nunca trocar silenciosamente para banco em memória.

A validação da matriz deve ocorrer localmente e em CI sem buscar schemas ou referências pela rede. O `access-matrix.schema.json` versionado no repositório é a única referência utilizada.

## 2. Inventário e isolamento

- Somente `/`, login e convite são anônimos; formulários sem sessão retorna `401`.
- Fixtures criam MO A, MO B e Materiais.
- Sessão A nunca lista B; ID existente de B e ID inexistente retornam o mesmo `404` e não alteram B.
- Endpoint novo não classificado, duplicado ou público fora da allowlist faz o teste de inventário falhar.

## 3. Perfis e tipo

- Analista em ação Admin → `403`.
- Terceirizado em dashboard/validação/pendências/alocação global → `403`.
- Materiais em qualquer operação de funcionário/mobilização/pagamento → `403`, inclusive dados legados.
- Happy paths permitidos de Admin, Analista, MO e Materiais continuam funcionando.

## 4. Revogação

Emita sessão, depois desative usuário/empresa ou altere perfil/empresa. A próxima operação protegida deve recusar claims antigas (`401`).

## 5. Produção

Os testes de startup cobrem JWT ausente/vazio/curto/conhecido e CORS inválido: todos abortam sem imprimir valores. Com configuração válida, o host inicia.

Confirme também: nenhuma conta demo; bootstrap exige senha de pelo menos 16 caracteres, cria uma vez e registra sucesso/recusa/erro sem credencial; Swagger retorna `404`; somente origem produtiva exata recebe header CORS.

## 6. Logs

Gere `401`, `403`, tenant-`404` e tentativas de bootstrap. Confirme `EventId`, reason code e `TraceId`; confirme ausência de bearer, senha, segredo, convite, documento completo, body e conteúdo documental.

## 7. Consultas de autorização

- Cada requisição protegida executa no máximo uma consulta adicional de identidade.
- A contagem de consultas de autorização nas listagens permanece constante ao aumentar a quantidade de resultados.

## 8. Deploy

1. Configurar JWT/CORS no Render e manter bootstrap off.
2. Remover/rotacionar credenciais e chave conhecidas; fazer backup.
3. Avisar que sessões atuais exigirão novo login.
4. Publicar API, executar smoke tests por perfil e depois publicar frontend.
5. Em falha, voltar versão sem reativar defaults inseguros.

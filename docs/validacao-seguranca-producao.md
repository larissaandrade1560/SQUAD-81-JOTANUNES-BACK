# Smoke tests de segurança em produção

Este roteiro valida, após o deploy da feature 004, os controles de autenticação, autorização, isolamento entre empresas, CORS e exposição da API. É um smoke test operacional; não substitui as suítes automatizadas nem autoriza testes destrutivos em dados reais.

## Cuidados antes de começar

- Aguarde CI verde para frontend e backend e confirme que a versão nova está **Live** no Render e publicada no Cloudflare Pages. Os testes backend usam .NET 8 e Docker; Vitest/build usam Node 22.
- Confirme a URL atual da API e do frontend nos provedores. As URLs registradas neste repositório são `https://squad-81-jotanunes-back.onrender.com` e `https://jotanunes-forms.pages.dev`; elas podem mudar.
- Use contas de teste previamente autorizadas para Administrador, Analista, terceirizado de Mão de Obra (MO), terceirizado de Materiais e, para isolamento, duas empresas de teste distintas (A e B). Não use credenciais de clientes nem dados pessoais reais.
- Prefira operações `GET` e preflight `OPTIONS`. **Não** teste upload/reenvio, cadastro, aprovação/rejeição, convite, pagamento, exclusão ou bootstrap Admin em produção. Esses fluxos podem persistir dados, enviar e-mail ou alterar documentos.
- Não ative `set -x`, não cole tokens em tickets/terminal compartilhado e não salve HARs ou respostas com dados pessoais. Use janela privada e encerre a sessão de teste ao final.
- Tokens emitidos pela versão anterior, sem `usuario_id`, serão recusados. Avise os usuários de que será necessário entrar novamente após o deploy.

## 1. Verificar a configuração do deploy

No painel do Render, confira os nomes/estado das variáveis sem revelar seus valores:

- `ASPNETCORE_ENVIRONMENT=Production`.
- `Jwt__SigningKey` definida como segredo aleatório forte (mínimo 32 caracteres), `Jwt__Issuer`, `Jwt__Audience` e `Jwt__ExpirationMinutes` válidos. Não copie o segredo para o terminal ou logs.
- `Cors__Origins` contém somente origens HTTPS exatas autorizadas, sem wildcard, localhost, path ou query.
- `BootstrapAdmin__Enabled=false` depois da criação inicial do Admin. Não ligue bootstrap para este teste.
- As migrations concluíram no startup, sem falhas. Revise logs com acesso autorizado e não compartilhe valores secretos.

Não provoque falha de startup removendo/alterando JWT ou CORS no serviço de produção: isso pode interromper o sistema. Os cenários de configuração inválida devem ser validados em CI/staging.

## 2. Preparar comandos de consulta

Em Bash, defina as URLs aprovadas e use esta função para imprimir somente o status HTTP de uma chamada autenticada. Faça login no frontend com uma conta de teste; se precisar do token, leia localmente `JSON.parse(sessionStorage.getItem('jnf-auth-session')).accessToken` no DevTools da mesma janela e digite-o no prompt oculto. Não use decodificadores externos, não o cole em comandos/tickets e não exporte HARs. O token não será incluído no histórico do shell nem nos argumentos do `curl`.

```bash
API_URL='https://squad-81-jotanunes-back.onrender.com'
FRONTEND_ORIGIN='https://jotanunes-forms.pages.dev'

read -r -s -p 'Token da conta de teste: ' ACCESS_TOKEN
printf '\n'

http_status() {
  curl --silent --show-error --output /dev/null --write-out 'HTTP %{http_code}\n' \
    --config <(printf 'header = "Authorization: Bearer %s"\n' "$ACCESS_TOKEN") \
    "${API_URL}$1"
}
```

Repita o prompt para cada perfil. Não imprima o corpo de endpoints administrativos: ele pode conter dados pessoais. Para o teste tenant-404 abaixo, use somente IDs de empresas de teste.

## 3. API anônima, sessão e Swagger

Execute sem token:

```bash
curl --silent --show-error --output /dev/null --write-out 'HTTP %{http_code}\n' "$API_URL/"
curl --silent --show-error --output /dev/null --write-out 'HTTP %{http_code}\n' "$API_URL/api/formularios"
curl --silent --show-error --output /dev/null --write-out 'HTTP %{http_code}\n' "$API_URL/api/auth/me"
curl --silent --show-error --output /dev/null --write-out 'HTTP %{http_code}\n' "$API_URL/swagger"
curl --silent --show-error --output /dev/null --write-out 'HTTP %{http_code}\n' "$API_URL/swagger/v1/swagger.json"
```

Esperado: `/` retorna `200`; `/api/formularios` e `/api/auth/me` retornam `401`; Swagger e o JSON de Swagger retornam `404` em Production. Não envie senhas erradas repetidamente para testar login; use um token inválido apenas uma vez, se necessário, contra `/api/formularios` e confirme `401`.

## 4. CORS: origem autorizada e origem não autorizada

Faça preflight para uma operação somente de leitura:

```bash
curl --include --request OPTIONS "$API_URL/api/formularios" \
  --header "Origin: $FRONTEND_ORIGIN" \
  --header 'Access-Control-Request-Method: GET' \
  --header 'Access-Control-Request-Headers: authorization'
```

Esperado: a resposta contém `Access-Control-Allow-Origin` exatamente igual a `$FRONTEND_ORIGIN`.

Repita com uma origem deliberadamente não autorizada:

```bash
curl --include --request OPTIONS "$API_URL/api/formularios" \
  --header 'Origin: https://origem-nao-autorizada.invalid' \
  --header 'Access-Control-Request-Method: GET' \
  --header 'Access-Control-Request-Headers: authorization'
```

Esperado: não há `Access-Control-Allow-Origin` para essa origem. Não é necessário que o servidor responda `403`; o critério é o navegador não receber autorização CORS para a origem não listada.

## 5. Matriz de perfil

Entre no frontend com uma conta de teste por vez. Confirme também em `/api/auth/me` que perfil e `tipoEmpresa` correspondem à conta. A resposta de login e a sessão terceirizada devem conter o tipo da empresa; para perfis internos ele deve ser nulo.

Para validar os status da API sem exibir conteúdo, use `http_status`:

```bash
http_status /api/auth/me
http_status /api/catalogo-requisitos
http_status /api/usuarios
http_status /api/funcionarios
http_status /api/mobilizacoes
http_status /api/pagamentos
```

Resultados esperados:

| Perfil de teste | `GET /api/catalogo-requisitos` | `GET /api/usuarios` | Funcionários, mobilizações e pagamentos |
| --- | ---: | ---: | --- |
| Administrador | `200` | `200` | `200`; dados globais internos |
| Analista | `200` | `403` | `200`; dados globais internos |
| MO | `403` | `403` | `200`; somente registros da própria empresa |
| Materiais | `403` | `403` | `403` em todos os três módulos MO |

Em cada sessão, confira visualmente que o menu corresponde ao perfil. Para MO, confira que as listas exibem apenas registros da empresa de teste A. Para Materiais, tente as chamadas `GET` acima mesmo que a interface oculte esses módulos: ocultar menu não substitui a autorização da API. Encerre e troque de sessão entre perfis.

## 6. Isolamento entre empresas (somente tenants de teste)

Faça login novamente como MO da empresa de teste A e substitua o token atual no prompt oculto. Use um ID conhecido da empresa de teste B e um UUID inexistente; não use IDs de clientes. O comando imprime apenas a resposta genérica e o status:

```bash
read -r -s -p 'Token MO da empresa de teste A: ' ACCESS_TOKEN
printf '\n'
```

Em seguida, execute:

```bash
EMPRESA_B_TEST_ID='<id-da-empresa-de-teste-b>'
ID_INEXISTENTE='00000000-0000-4000-8000-000000000000'

for EMPRESA_ID in "$EMPRESA_B_TEST_ID" "$ID_INEXISTENTE"; do
  curl --silent --show-error --write-out '\nHTTP %{http_code}\n' \
    --config <(printf 'header = "Authorization: Bearer %s"\n' "$ACCESS_TOKEN") \
    "$API_URL/api/empresas/$EMPRESA_ID"
done
```

Esperado para ambos: `404` e o mesmo corpo genérico de recurso não encontrado. A listagem `GET /api/empresas`, acessada pela interface com a conta A, deve conter somente a empresa A. Não teste downloads ou detalhes documentais de registros reais.

Depois da comparação, remova a função e o token da memória do shell:

```bash
unset ACCESS_TOKEN
unset -f http_status
```

## 7. Sessão frontend e revogação

- Em uma janela privada com conta de teste, confirme login, navegação permitida e restauração de `tipoEmpresa` após atualizar a página.
- Para um smoke test sem alteração no banco do teste de `401` no frontend, somente na janela privada e com conta descartável, substitua temporariamente `accessToken` em `sessionStorage['jnf-auth-session']` por `token-invalido-de-teste` e abra `/documentos` (uma leitura). Esperado: a API responde `401`, a sessão local é descartada e a aplicação volta ao login. Depois, feche a janela e faça login normalmente.
- Não desative nem altere perfil/empresa de usuário real para simular revogação. O cenário de token antigo após inativação/alteração cadastral deve ser executado em staging; em produção só pode ser tentado com conta descartável, janela aprovada e plano imediato de restauração.

## 8. Logs e critérios de interrupção

Após poucas chamadas, verifique nos logs protegidos do Render os eventos de challenge/forbid e tenant-404 (`authorization_challenge`, `authorization_forbid`, `tenant_access_denied`/`tenant_not_found`) com `TraceId` e reason code. Confirme que não aparecem bearer tokens, senhas, secrets, documentos completos, conteúdo de request ou arquivo. Não copie logs brutos para canais compartilhados.

Interrompa o smoke test e acione o responsável se uma conta sem sessão receber sucesso, Analista acessar rota Admin, Materiais receber `2xx` em rota MO, a empresa A visualizar recurso da B, Swagger estiver acessível ou uma origem não autorizada receber CORS. Não tente corrigir variáveis na produção sem change/rollback aprovado.

## Registro do resultado

Registre somente data/hora, versão/commit, endpoint ou cenário, status esperado/observado e `TraceId` quando necessário. Não registre tokens, senhas, IDs de clientes ou corpos de resposta com dados pessoais. Após o teste, faça logout, feche a janela privada e apague qualquer evidência local que contenha informação protegida.

Referências: [quickstart da feature 004](../specs/004-harden-access-security/quickstart.md), [matriz de acesso](../specs/004-harden-access-security/contracts/access-matrix.md), [contrato HTTP](../specs/004-harden-access-security/contracts/http-security.md) e [contrato de produção](../specs/004-harden-access-security/contracts/production-security.md).

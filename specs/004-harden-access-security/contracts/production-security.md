# Production Security Contract

## Startup

| Cenário em Production | Resultado |
| --- | --- |
| JWT ausente, vazio, curto ou conhecido | aborta |
| Issuer/Audience ausentes | aborta |
| Expiração inválida | aborta |
| CORS vazio, wildcard, localhost ou malformado | aborta |
| Configuração válida | inicia |

Falhas citam somente o nome da configuração.

## Bootstrap

| Cenário | Resultado |
| --- | --- |
| Development normal | demo idempotente permitida |
| Production normal/bootstrap off | nenhuma conta criada |
| Bootstrap válido e sem admin | cria um Admin |
| Admin existente | recusa sem alteração |
| Bootstrap incompleto, senha com menos de 16 caracteres ou valor conhecido | aborta |

Documento, nome e senha são fornecidos somente por configuração segura do ambiente, sem fallback. Toda tentativa gera evento `Succeeded`, `RefusedExistingAdmin` ou `InvalidConfiguration` sem documento completo, senha ou hash.

## Exposição e deploy

- Swagger UI/JSON: Development apenas; `404` em Production.
- CORS inicial: `https://jotanunes-forms.pages.dev` exato; localhost só Development; preview listado individualmente.
- Render exige `Jwt__SigningKey` secreto, issuer, audience, expiração e `Cors__Origins`; bootstrap fica desligado.

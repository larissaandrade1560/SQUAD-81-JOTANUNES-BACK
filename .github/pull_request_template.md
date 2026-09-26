## Segurança e controle de acesso

- [ ] Toda rota nova/alterada está classificada em `specs/004-harden-access-security/contracts/access-matrix.json`.
- [ ] O teste de inventário cobre a rota, verbo, política e condição pública.
- [ ] Listas de tenant são filtradas no backend; recursos alheios e inexistentes têm o mesmo `404` genérico.
- [ ] A negação não dispara mutações, downloads ou efeitos em storage.
- [ ] Materiais não recebe acesso a funcionários, mobilizações ou pagamentos, inclusive por dados legados.
- [ ] Eventos de segurança não incluem token, senha, documento completo, body, convite ou conteúdo documental.
- [ ] Mudanças de configuração de produção mantêm JWT/CORS fail-closed e bootstrap desabilitado por padrão.

## Validação

- [ ] `dotnet test JotaNunesForms.sln --configuration Release` (Docker ativo para os testes PostgreSQL).
- [ ] `npm --prefix ../frontend test`
- [ ] `npm --prefix ../frontend run build`

**Evidências / exceções:**


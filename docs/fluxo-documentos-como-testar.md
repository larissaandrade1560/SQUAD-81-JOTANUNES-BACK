# Como testar o fluxo dinâmico de documentos

**Feature:** `002-fluxo-documentos`  
**Ambiente:** produção (não local)  
**Última bateria E2E:** 27/09/2026 (tarde — cenários US4 antes pendentes)  
**Spec:** [`specs/002-fluxo-documentos/spec.md`](../specs/002-fluxo-documentos/spec.md)

Este incremento cobre **US1–US3** e **US4 (checklist admissional / liberação)**: abrir processo, qualificar empresa, mobilizar trabalhador, consultar liberação, registrar EPI (MO) e integração (interno). **US5** (seis status de pagamento) permanece fora deste escopo.

---

## 1. Onde testar

| Componente | URL |
|------------|-----|
| Frontend | https://jotanunes-forms.pages.dev |
| API | https://squad-81-jotanunes-back.onrender.com |

O plano gratuito do Render pode demorar até ~1 minuto na primeira requisição (cold start). Se o login falhar por rede, aguarde e tente de novo.

---

## 2. Contas

| Perfil | Documento | Senha | O que exercita |
|--------|-----------|-------|----------------|
| Administrador | `00000000001` | `senha123` | Empresas, processos, sócios, mobilização, catálogo, encaminhar |
| Analista | `12345678900` | `senha123` | Fila `/validacao` — aprovar / rejeitar |
| Terceirizado (mão de obra) | `11122233344` | `senha123` | Upload no checklist, pagamentos RF13 (legado) |

**MO em produção:** o usuário da empresa *Construtora Teste E2E* foi ativado por **convite**; o login no portal deve usar o **e-mail do convite** (`matheusss821@gmail.com`) e `senha123`. O CPF `11122233344` sozinho retorna “credenciais inválidas” (a API bloqueia login por documento quando há e-mail cadastrado). O documento continua aparecendo na sessão após login por e-mail.

Não há usuário terceirizado de **materiais** no seed. O bloqueio de mobilização para materiais testa-se com **Admin** (API) ou criando empresa tipo Materiais.

---

## 3. Telas do fluxo

| Rota | Quem usa |
|------|----------|
| `/processos` | Admin abre processo; todos os internos listam |
| `/processos/{id}` | Checklist, **Enviar PDF**, histórico (hash), **Encaminhar ao setor de contratos** |
| `/empresas` → **Sócios** | Quadro societário (2 requisitos por sócio no checklist) |
| `/mobilizacoes` | Formulário-pai; situação **Aguardando** até haver US4 |
| `/mobilizacoes/{id}` | Lotação + checklist admissional |
| `/validacao` | Analista: escopo **Checklist** = versões do processo |

Itens **Trabalhador** não entram no checklist do processo na abertura. Só aparecem depois de criar uma mobilização (e passam a contar no total de itens do processo).

---

## 4. Roteiro US1 — checklist dinâmico

Pré-requisito: empresa **Mão de Obra**, uma **obra** e (opcional) sócios cadastrados.

### 4.1 Processo A (mínimo)

1. Admin → **Processos** → **Abrir processo**.
2. Empresa MO + obra.
3. Serviço qualquer (ex.: alvenaria).
4. Sócios = **1**.
5. Deixe **desmarcados:** resíduos, laboratório, Simples, ART, mobilização.
6. **Gerar checklist**.

**Esperado:** ~7 itens ativos — os cinco corporativos (`CONTRATO_SOCIAL`, `COMPROVANTE_CNPJ`, `ENDERECO_COMERCIAL`, `CRF_FGTS`, `CND_FEDERAL`) + 2 do sócio 1. **Não** listar `LICENCA_MUNICIPAL_RESIDUOS`, `CGCRE_INMETRO`, `OPCAO_SIMPLES`, ART nem códigos admissionais (`ASO_ADMISSIONAL`, `MOB_CADASTRO`, …).

### 4.2 Processo B (condicionais + mobilização)

Na **mesma** empresa MO:

1. Abrir processo com resíduos, laboratório, Simples, ART, **2 sócios**, mobilização marcada.
2. Comparar com A.

**Esperado:** os cinco básicos **mais** licença / INMETRO / Simples / ART **mais** 4 itens de sócio (2×2). Sem itens de trabalhador **até** criar mobilização. Depois da mobilização o total do processo sobe (admissionais do trabalhador).

### 4.3 Empresa de materiais (sem admissional)

1. Admin → **Empresas** → **Nova empresa** → classificação **Fornecedora de Materiais**.
2. Abrir processo **sem** mobilização, 1 sócio, sem condicionais.

**Esperado:** corporativos + sócio; **nenhum** item admissional.

Abrir processo de materiais **com** mobilização marcada deve falhar na API (`400` — empresa de materiais não pode mobilizar). A UI não impede o checkbox; o erro aparece ao gerar.

### 4.4 Encaminhar ao setor de contratos

No checklist (Admin/Analista), **Encaminhar ao setor de contratos**. O cabeçalho passa a mostrar a data de encaminhamento. Não duplica arquivos.

### 4.5 Recalcular checklist (sem tela)

Não há formulário de edição do questionário. Com JWT de Admin:

```http
PATCH /api/processos-contratacao/{processoId}
Content-Type: application/json

{
  "servicoContratado": "mesmo ou novo texto",
  "transportaResiduos": false,
  "controleTecnologico": false,
  "optanteSimples": true,
  "exigeArt": false,
  "quantidadeSocios": 1,
  "mobilizaTrabalhadores": false
}
```

**Esperado:** itens que deixaram de aplicar ficam inativos (histórico preservado); novos (ex.: `OPCAO_SIMPLES`) nascem pendentes.

---

## 5. Roteiro US2 — versão, análise e qualificação

Use o **processo A** (sem trabalhador), para a qualificação não se misturar com admissionais.

1. No checklist, **Enviar PDF** em cada item ativo (PDF qualquer, ≤ 10 MB).
2. **Histórico:** `v1 · vigente · SHA-256 …`.
3. Sair → entrar como **Analista** → `/validacao`.
4. Filtrar visualmente escopo **Checklist** (não o PDF antigo de funcionário RF05).
5. **Rejeitar** um item (ex.: contrato social) **sem** motivo → o campo obrigatório impede o envio.
6. Rejeitar **com** motivo.
7. Admin (ou terceirizado) reenvia PDF no mesmo item → `v2` vigente; `v1` permanece com a justificativa.
8. Analista **Aprova** todos os itens corporativos/sócio obrigatórios (incluindo a v2).

**Esperado:**

- Processo **Qualificado** (não só Aberto).
- Nenhuma mobilização em **Liberado** (continua **Aguardando** se existir).

Aprovar/rejeitar versão na API:

```http
POST /api/validacao/versoes/{versaoId}/aprovar
POST /api/validacao/versoes/{versaoId}/rejeitar
{ "motivo": "texto obrigatório" }
```

---

## 6. Roteiro US3 — mobilização

1. Processo **com** mobilização (B).
2. Admin ou Terceirizado MO → **Mobilização** → **Nova mobilização**.
3. Processo, nome, CPF, função; opcional data fim e turno.
4. Criar.

**Esperado:** situação **Aguardando**; histórico de lotação “em curso · Abertura da mobilização”; checklist admissional (ASO, documento com foto, EPI, eSocial, integração, cadastro, NR-18, ordem de serviço) todos **Não enviado**.

### 6.1 Editar (sem tela)

```http
PATCH /api/mobilizacoes/{id}
{
  "funcao": "Pedreiro (editado)",
  "dataFimObra": "2026-12-31",
  "turnoJornada": "Noturno 8h"
}
```

A situação **não** muda para Liberado por este PATCH.

### 6.2 Materiais bloqueados

Com o `contratoId` / `empresaId` da empresa Materiais:

```http
POST /api/mobilizacoes
```

**Esperado:** `403` — “Empresa de materiais não mobiliza trabalhadores.”

---

## 7. Catálogo extra (SC-008)

Não há tela de catálogo. Admin:

```http
POST /api/catalogo-requisitos
{
  "codigo": "TESTE_EXTRA_ADMISSAO",
  "nome": "Documento extra admissional teste",
  "titular": 4,
  "aplicacao": 1,
  "tipoEntrega": 1,
  "camada": 3,
  "condicao": null,
  "exigeValidade": false,
  "permiteVencerComoDocumento": false
}
```

`titular: 4` = Trabalhador. A **próxima** mobilização deve listar o código novo no checklist admissional, sem alterar código do produto.

`GET /api/catalogo-requisitos` lista o seed (`CONTRATO_SOCIAL` … `INTEGRACAO_OBRA`).

---

## 8. Fora deste incremento (não exigir no aceite)

| História | O que acontece hoje |
|----------|---------------------|
| **US4** Liberado para acesso | `GET /api/mobilizacoes/{id}/liberacao` retorna impedimentos; EPI (`/epi`) e integração (`/integracao`) disponíveis conforme papel. Validadores admissionais aplicam-se aos cinco códigos documentais. |
| **US5** Seis status de pagamento | `/pagamentos` ainda é o RF13 (`No prazo`, atraso, etc.). Sem Regularizado com atraso / reuso 409 desta spec. |
| S-2190 preliminar | Não implementado. |
| OCR / campos extraídos | Metadados manuais; sem OCR. |
| FGTS/DCTFWeb mensal, lotes de material | Fora do MVP. |

---

## 9. Testes automatizados (opcional)

No backend, os motores de regra:

```bash
dotnet test backend/tests/JotaNunesForms.Domain.Tests
dotnet test backend/tests/JotaNunesForms.Application.Tests
```

Cobre geração de checklist (A vs B, materiais, sócios) e recálculo de situação do processo (qualificado sem titular trabalhador).

---

## 10. Checklist rápido de aceite (US1–US3)

- [ ] Dois processos da mesma empresa geram checklists diferentes (condicionais / sócios / mobilização).
- [ ] Materiais sem mobilização: só corporativo + sócio.
- [ ] Materiais não cria mobilização (`403` / `400` na abertura com flag).
- [ ] Encaminhar ao setor de contratos registra no processo.
- [ ] Upload gera versão com hash; rejeição exige motivo; reenvio cria v2.
- [ ] Corporativo aprovado → processo **Qualificado**; trabalhador **não** Liberado.
- [ ] Mobilização nasce **Aguardando** com lotação e itens admissionais.
- [ ] Novo código no catálogo aparece na próxima mobilização.

---

## Referências

- Spec e critérios: `specs/002-fluxo-documentos/spec.md`
- Contrato HTTP: `specs/002-fluxo-documentos/contracts/api-fluxo-documentos.md`
- Quickstart (inclui US4/US5 futuros): `specs/002-fluxo-documentos/quickstart.md`

## US4 — evidência publicada (T083)

**Deploy verificado:** API `200` (`/`), frontend Pages `200`, commit `862db30`+ em `develop` (Render manual deploy).

**Mobilização de referência (produção):** `9461000a-f349-4442-826c-f2ca0b1ce79a` (Joana Extra Catalogo).  
**Mobilização auxiliar (vencimentos):** `47590616-647d-4da3-9f68-f66736e79eca` (Trabalhador Vencimento E2E).

**Upload admissional em produção:** itens do catálogo com `tipoEntrega` = upload exigem `multipart` com `arquivo` (PDF) **e** `camposJson`; só JSON retorna `400` “Envie um arquivo PDF.”.

| Cenário | Status | Evidência |
|---------|--------|-----------|
| Checklist incompleto + impedimentos (quickstart §1, §9) | OK | UI `/mobilizacoes/{id}`: painel **Liberação** lista 8 códigos estáveis (`IDENTIDADE_PENDENTE` … `REQUISITO_ADICIONAL_PENDENTE`); `MOB_CADASTRO` aprovado no checklist; `liberado=false`, situação Aguardando. API `GET /liberacao` alinhada. |
| Integração interna + idempotência (§5) | OK parcial | `POST /integracao` Admin → `201`; retry mesma `Idempotency-Key` → `200`; `INTEGRACAO_PENDENTE` removido até outros itens pendentes. UI mostra histórico “Instrutor Teste”. |
| Reversão integração refazer (§6) | OK | `POST /integracao/refazer` → `200`; impedimento `INTEGRACAO_REFAZER` na API e na UI após refresh. |
| Autorização (§7) | OK | Anônimo `401`; UUID inexistente `404`; Admin `POST /epi` → `403`; MO `POST /integracao` → `403`. MO autentica com e-mail do convite + `senha123`; CPF `11122233344` no formulário → erro (regra `UsaLoginPorEmail`). |
| Liberação completa Aguardando → Liberado | OK | Mobilização `9461000a-f349-4442-826c-f2ca0b1ce79a`: upload PDF+JSON dos 5 códigos admissionais + `TESTE_EXTRA_ADMISSAO`; Analista aprovou todas as versões; EPI ativo; Admin `POST /integracao` → `201` com `liberado=true`, `impedimentos=[]`, `situacao=2` (Liberado). UI MO: texto **Liberado para acesso** no cabeçalho. |
| Reversão por ASO/NR-18 vencido | OK | Mobilização auxiliar `47590616-647d-4da3-9f68-f66736e79eca`: ASO com `dataExame` 2020-01-10 aprovado → `GET /liberacao` inclui `ASO_VENCIDO`; NR-18 com `data` 2020-01-01 aprovado → também `NR18_VENCIDA` (reconciliação na leitura). |
| EPI entrega/substituição/devolução (§4) | OK | Entrega/substituição/idempotência já validados. Devolução parcial (Luva) → `201`, `saldoAtivo=1`, permanece Liberado. Devolução total do saldo ativo (Capacete Novo) → `201`, `saldoAtivo=0`, `liberado=false`, `EPI_SEM_ENTREGA_ATIVA`; nova entrega (Bota) restaura `liberado=true`. **Nota:** devolução com `movimentoOrigemId` inválido (ex.: entrega já zerada por substituição) retornou `500` em um tentativa — usar origem com saldo ativo. |
| Validadores admissionais §2 (5 códigos) | OK | Produção exige **PDF + `camposJson`**. Inválidos: CPF divergente → `400` “CPF não corresponde…”; NR-18 carga 1h → `400` “Carga horária abaixo…”. Válidos: `DOC_OFICIAL_FOTO`, `ESOCIAL_VINCULO` (S-2200), `ASO_ADMISSIONAL`, `NR18_BASICA`, `ORDEM_SERVICO` enviados e aprovados na mobilização de referência. |
| Negação Materials / cross-tenant MO A×B | OK parcial | Terceirizado Materiais (`matsss821@gmail.com` / `senha123`): `GET /mobilizacoes/{id}`, `GET /liberacao`, `POST /mobilizacoes` → **403**. MO com UUID inexistente → **404** corpo genérico. **MO empresa B:** não há segunda empresa MO em produção; isolamento entre tenants MO não foi exercitado (apenas Materials × mobilização MO). |
| Cenário S-2190 preliminar (quickstart §3) | Não executado | Exige mobilização isolada com S-2190 e prazo normativo; fora da tabela original T083, registrado para rodada futura. |

- Entregas gerais: `docs/resumo-entregas.md`

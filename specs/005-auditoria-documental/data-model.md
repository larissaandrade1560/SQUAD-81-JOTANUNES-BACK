# Data Model: RF17 — Histórico de Auditoria Documental

## EventoAuditoriaDocumento

Registro imutável de uma transição documental concluída.

| Campo | Tipo conceitual | Regras |
|------|-----------------|--------|
| `Id` | UUID | Obrigatório; gerado uma vez. |
| `ChaveNegocio` | string (200) | Obrigatória e única; identifica a transição para impedir duplicidade. |
| `Codigo` | enum | `DocumentoEnviado`, `DocumentoReenviado`, `DocumentoAprovado`, `DocumentoRejeitado`, `DocumentoVencido`. |
| `OcorreuEm` | instante UTC | Obrigatório e imutável. |
| `AtorTipo` | enum | `Usuario` ou `Sistema`. |
| `AtorUsuarioId` | UUID opcional | Obrigatório para ator humano; sem cascade-delete. |
| `AtorNome` | string (200) | Snapshot obrigatório; `Sistema` para ação automática. |
| `AtorPerfil` | string (40) opcional | Snapshot somente para ator humano. |
| `Escopo` | enum | `Empresa`, `Funcionario` ou `Requisito`. |
| `OrigemTipo` | enum | `DocumentoEmpresa`, `DocumentoFuncionario` ou `DocumentoVersao`. |
| `DocumentoId` | UUID | Identificador estável da origem documental. |
| `VersaoId` | UUID opcional | Versão nova/decidida/vencida. |
| `VersaoNumero` | inteiro opcional | Positivo quando a origem possuir versão. |
| `VersaoAnteriorId` | UUID opcional | Obrigatório no reenvio quando houver versão anterior. |
| `VersaoAnteriorNumero` | inteiro opcional | Positivo quando `VersaoAnteriorId` existir. |
| `EmpresaId` | UUID | Obrigatório; snapshot lógico, sem cascade-delete. |
| `EmpresaRazaoSocial` | string (200) | Snapshot obrigatório. |
| `FuncionarioId` | UUID opcional | Obrigatório no escopo funcionário. |
| `FuncionarioNome` | string (200) opcional | Snapshot obrigatório quando `FuncionarioId` existir. |
| `ProcessoId` | UUID opcional | Para origem `DocumentoVersao`, quando aplicável. |
| `ItemChecklistId` | UUID opcional | Para origem `DocumentoVersao`. |
| `TipoDocumentoRotulo` | string (160) | Referência segura; não inclui nome bruto do arquivo. |
| `Motivo` | string (2000) opcional | Permitido somente em rejeição; texto normalizado. |
| `Comentario` | string (2000) opcional | Permitido em aprovação/rejeição. |
| `ValidoAte` | instante UTC opcional | Permitido em aprovação/vencimento. |

### Invariantes

- Não possui operação pública de alteração ou remoção.
- Ator `Usuario` exige ID, nome e perfil; ator `Sistema` não aceita ID/perfil.
- Rejeição exige motivo; demais códigos não aceitam motivo de rejeição.
- Motivo e comentário aceitam no máximo 2.000 caracteres; valores maiores são rejeitados antes da persistência, sem truncamento.
- Reenvio exige versão atual e anterior distintas.
- Escopo `Funcionario` exige funcionário; outros escopos não inventam funcionário.
- Nenhum campo aceita CPF/CNPJ completo, token, segredo, storage key, conteúdo do arquivo, hash ou body bruto.
- `UPDATE` e `DELETE` são rejeitados no PostgreSQL.

### Chaves de idempotência

- Envio: `documento_enviado:{origemTipo}:{versaoId}`.
- Reenvio: `documento_reenviado:{origemTipo}:{versaoId}`.
- Decisão terminal: `documento_analisado:{origemTipo}:{versaoId}`; aprovação e rejeição competem pela mesma transição.
- Vencimento: `documento_vencido:{origemTipo}:{versaoId}`; uma nova versão cria novo ciclo legítimo.

Uma chave já confirmada para a mesma transição produz sucesso idempotente e reutiliza o resultado existente. Uma transição terminal incompatível retorna `409 documento_estado_conflitante`; uma verificação de vencimento já efetivada é no-op bem-sucedido. Nenhum conflito cria nova versão, análise ou evento.

### Índices

- Único em `ChaveNegocio`.
- `(OcorreuEm DESC, Id DESC)`.
- `(EmpresaId, OcorreuEm DESC, Id DESC)`.
- `(Codigo, OcorreuEm DESC, Id DESC)`.
- `(Escopo, OcorreuEm DESC, Id DESC)`.
- `(OrigemTipo, DocumentoId, OcorreuEm DESC)` para localizar a cadeia documental.

## DocumentoArquivoVersao

Preserva arquivos versionados dos fluxos legados `DocumentoEmpresa` e `DocumentoFuncionario`.

| Campo | Tipo conceitual | Regras |
|------|-----------------|--------|
| `Id` | UUID | Obrigatório. |
| `DocumentoEmpresaId` | UUID opcional | Exatamente uma das duas origens deve existir. |
| `DocumentoFuncionarioId` | UUID opcional | Exatamente uma das duas origens deve existir. |
| `Numero` | inteiro | Positivo, crescente dentro do documento. |
| `NomeArquivo` | string (260) | Obrigatório; nunca exposto na auditoria global. |
| `StorageKey` | string (512) | Obrigatório; somente persistência/download autorizado. |
| `ContentType` | string (128) | Obrigatório. |
| `TamanhoBytes` | inteiro longo | Maior que zero. |
| `EnviadoPorUsuarioId` | UUID opcional | Nulo apenas para baseline legado sem autoria comprovada. |
| `EnviadoEm` | instante UTC | Obrigatório; no baseline usa a data comprovada do documento atual. |
| `Vigente` | booleano | Exatamente uma versão vigente por documento. |

### Invariantes e relações

- Constraint XOR exige `DocumentoEmpresaId` ou `DocumentoFuncionarioId`, nunca ambos/nem nenhum.
- Número é único dentro de cada documento.
- Índice único parcial garante somente uma versão vigente por documento.
- Reenvio marca a versão anterior como não vigente e cria a próxima; a versão anterior nunca é apagada.
- FKs para os documentos usam `Restrict`; não há cascade-delete de histórico.
- A migração cria versão 1 para cada arquivo legado atual sem criar `EventoAuditoriaDocumento`.

## DocumentoVersao (existente, reforçado)

- Continua representando versões de itens de checklist.
- Adicionar unicidade `(ItemChecklistId, Numero)`.
- Adicionar índice único parcial para uma única versão `Vigente` por item.
- Upload concorrente que viole a unicidade perde a transação completa, inclusive o evento.

## AnaliseDocumento (existente, reforçado pelo caso de uso)

- Aprovação/rejeição só é aceita para versão vigente cujo item esteja pendente de análise.
- A chave única do evento de decisão, dentro da mesma transação, impede duas decisões terminais concorrentes.
- Dados históricos existentes não recebem evento retroativo.

## ConsultaEventosAuditoria

Objeto de consulta, não persistido.

| Campo | Regra |
|------|-------|
| `De` | RFC 3339, inclusivo, opcional. |
| `Ate` | RFC 3339, exclusivo, opcional; deve ser maior que `De`. |
| `Codigo` | Um dos cinco códigos públicos, opcional. |
| `EmpresaId` | UUID válido, opcional. |
| `Escopo` | `empresa`, `funcionario` ou `requisito`, opcional. |
| `Page` | Inteiro de 1 em diante; padrão 1. |
| `PageSize` | Inteiro entre 1 e 100; padrão 50. |

## State Transitions

```text
arquivo inexistente ──envio──► versão 1 vigente + evento enviado

versão vigente rejeitada/vencida
  └──reenvio──► versão anterior não vigente
                + nova versão vigente
                + evento reenviado

versão vigente pendente
  ├──aprovação──► aprovada + evento aprovado
  └──rejeição───► rejeitada + evento rejeitado

versão vigente aprovada e expirada
  └──verificação──► vencida + evento vencido (Sistema)
```

Repetição sem nova transição não cria análise, versão ou evento. Toda transição e seu evento pertencem à mesma transação PostgreSQL.

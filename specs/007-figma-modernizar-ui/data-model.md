# Data model (UI): Validação documental — piloto modernização

Esta feature **não** altera persistência nem entidades de domínio no backend. O modelo abaixo descreve **estado de interface** e **projeções** sobre dados já retornados pela API.

## Entidades consumidas (existentes)

### ValidacaoDocumentoItem

Fonte: `GET /api/validacao/fila` → `validacaoService.listValidacaoFila()`.

| Campo | Uso na UI modernizada |
|-------|------------------------|
| `escopo` | Badge de escopo; roteamento aprovar/rejeitar |
| `empresaRazaoSocial` | Tabela; busca textual |
| `funcionarioNome` | Tabela; busca textual |
| `tipoRotulo` | Tabela (“tipo de documento”) |
| `catalogoCodigo` | Coluna código (se exibida) |
| `nomeArquivo` | Tabela |
| `tamanhoBytes` | Coluna tamanho (formatado) |
| `enviadoEm` | Coluna data solicitação |
| `status` / `statusRotulo` | Badge; filtros; agregação nos cards |
| `id` | Chave de linha; ações |

Status numéricos usados hoje na página (`ValidacaoPage`): `1` em análise, `2` aprovado, `3` rejeitado, `4` warning/pendência — filtros e cards devem alinhar-se aos valores **presentes na fila** (documentos pendentes de validação).

## Estado local da página (novo/estendido)

| Estado | Tipo | Regras |
|--------|------|--------|
| `fila` | `ValidacaoDocumentoItem[]` | Fonte única após load |
| `loading` | `boolean` | Exibe skeleton ou mensagem de carregamento |
| `error` | `string?` | `role="alert"` |
| `searchQuery` | `string` | Filtra fila por substring (case-insensitive) em `empresaRazaoSocial`, `funcionarioNome`, `tipoRotulo`, `nomeArquivo`, `catalogoCodigo` |
| `statusFilter` | `'todos' \| 'aguardando' \| 'em_analise'` | Interseção com `searchQuery`; regras em MetricSummary abaixo |
| `rejectTarget`, `motivo`, `rejectError`, `actingId` | existentes | Preservar fluxo RF10 |

## Projeções derivadas (somente UI)

### MetricSummary (regras fechadas)

Calculada em memória a partir de `fila` (não persistida):

| Métrica | Regra (sobre `fila` retornada por `listValidacaoFila`) |
|---------|--------------------------------------------------------|
| `totalDocumentos` | `fila.length` |
| `emAnalise` | itens com `status === 1` |
| `aguardandoValidacao` | itens com `status !== 1` (pendentes de conclusão na fila atual; tipicamente antes/ além de “em análise” conforme RF12) |

**Filtros de chip** (`statusFilter`):

| Chip UI | Inclui itens onde |
|---------|-------------------|
| `todos` | sem filtro de status |
| `em_analise` | `status === 1` |
| `aguardando` | `status !== 1` |

Se a fila real usar apenas um subconjunto de status, ajustar contagens na implementação **sem inventar status** — documentar desvio na tabela de desvios do handoff.

### FilaFiltrada

`filaFiltrada = applySearchAndStatus(fila, searchQuery, statusFilter)` — única lista renderizada na tabela.

## Relacionamentos

```text
listValidacaoFila() → fila → MetricSummary (derive)
                         → filaFiltrada → tabela + ações (aprovar/rejeitar)
```

## Validação

- Busca/filtro não podem exibir itens que não estão em `fila` (sem chamada extra à API).
- Ações de linha usam `escopo` + `id` do item visível (pós-filtro).

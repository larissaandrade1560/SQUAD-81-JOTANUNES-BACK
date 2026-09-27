# Access Matrix Contract

A fonte canônica de operações é [`access-matrix.json`](./access-matrix.json), validada pelo vocabulário independente de [`access-matrix.schema.json`](./access-matrix.schema.json). A matriz registra cada combinação de método HTTP e rota sem curingas, reticências ou agrupamentos implícitos.

## Capacidades

- **Public**: não exige sessão e não expõe dados de negócio.
- **Any**: exige identidade ativa.
- **Admin**: somente Administrador ativo.
- **Internal**: Administrador ou Analista ativo.
- **Own**: terceiro ativo limitado ao tenant atual.
- **MO**: exige empresa do tipo Mão de Obra; empresa de Materiais recebe `403`.
- Formas compostas como **InternalOrOwn** preservam o escopo global interno e aplicam ownership ao terceiro.

Os valores aceitos de `access`, `scope` e `tenantBoundaryOutcome` estão definidos exclusivamente no JSON Schema e são validados antes da desserialização tipada. Valores fora desse vocabulário falham antes da execução dos cenários HTTP.

## Contrato de negação

Os defaults do JSON são obrigatórios para toda operação protegida:

- identidade ausente ou inválida: `401`;
- perfil ou tipo de empresa incompatível: `403`;
- recurso inexistente: `404` genérico.

Cada operação declara `tenantBoundaryOutcome` com status e comportamento: `200/filtered` para coleções escopadas que omitem registros externos, `404/generic-not-found` para operações que recebem identificador direto de recurso e `null/not-applicable` para operações públicas, globais ou sem entrada de tenant.

Um identificador inexistente e um identificador de outro tenant percorrem o mesmo fluxo de autorização e retornam o mesmo status e formato de resposta. Não existe promessa de igualdade exata de latência.

## Validação

O teste de inventário descobre as operações em runtime por `EndpointDataSource`, normaliza constraints de rota e compara o conjunto `(method, route)` com o JSON. Operação ausente, duplicada ou anônima fora da allowlist falha.

Testes de perfil e tenant usam o mesmo JSON para selecionar os cenários. A fixture rejeita qualquer combinação diferente de `200/filtered` em coleção escopada, `404/generic-not-found` em recurso identificado ou `null/not-applicable` em operação pública/global. A documentação não mantém uma segunda tabela manual de permissões ou vocabulário duplicado.

O endpoint `GET /api/auditoria/eventos` da RF17 está classificado na fonte canônica como `Internal` e `global`: somente Administrador e Analista ativos podem consultar a trilha documental.

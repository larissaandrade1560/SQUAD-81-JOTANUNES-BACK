# Casos de Uso por Ator — Sistema de Gestão de Terceirizados Jotanunes

## 1. Visão Geral

O sistema possui três grupos principais de usuários:

1. **Funcionários da Jotanunes**
   - Administrador / Responsável
   - Analista da Construtora
2. **Usuários de empresas fornecedoras de mão de obra**
3. **Usuários de empresas fornecedoras de materiais**

A separação é importante porque cada grupo possui responsabilidades, permissões e fluxos diferentes dentro do sistema.

---

## 2. Funcionário da Jotanunes

O usuário interno da Jotanunes pode possuir dois perfis principais:

- **Administrador / Responsável:** possui controle sobre cadastros, obras e parâmetros do sistema.
- **Analista da Construtora:** responsável pela análise, aprovação e rejeição fundamentada de documentos e comprovantes.

### Casos de Uso

| ID | Caso de Uso | Ator |
|---|---|---|
| UC-JN-01 | Autenticar-se no sistema | Todos os funcionários |
| UC-JN-02 | Consultar dashboard gerencial | Administrador / Analista |
| UC-JN-03 | Cadastrar empresa terceirizada | Administrador |
| UC-JN-04 | Editar dados de empresa terceirizada | Administrador |
| UC-JN-05 | Classificar empresa como Mão de Obra ou Materiais | Administrador |
| UC-JN-06 | Cadastrar obra | Administrador |
| UC-JN-07 | Editar e gerenciar obra | Administrador |
| UC-JN-08 | Consultar empresas terceirizadas | Administrador / Analista |
| UC-JN-09 | Consultar funcionários terceirizados | Administrador / Analista |
| UC-JN-10 | Consultar terceirizados por obra | Administrador / Analista |
| UC-JN-11 | Consultar documentos pendentes de análise | Analista |
| UC-JN-12 | Visualizar documento enviado | Analista |
| UC-JN-13 | Aprovar documento | Analista |
| UC-JN-14 | Rejeitar documento | Analista |
| UC-JN-15 | Informar motivo da rejeição | Analista |
| UC-JN-16 | Validar comprovante de pagamento | Analista |
| UC-JN-17 | Consultar empresas e funcionários com pendências | Administrador / Analista |
| UC-JN-18 | Consultar comprovantes em atraso | Administrador / Analista |
| UC-JN-19 | Consultar histórico de documentos | Administrador / Analista |
| UC-JN-20 | Consultar histórico e auditoria das operações | Administrador / Analista |

### Fluxo principal de validação

```text
Funcionário Jotanunes
        ↓
Consulta documentos enviados
        ↓
Seleciona documento
        ↓
Analisa documento
        ↓
 ┌──────┴──────┐
Aprovar      Rejeitar
  ↓              ↓
Atualiza       Informa
status         justificativa
  ↓              ↓
Registro de auditoria
```

A rejeição exige justificativa e toda aprovação, rejeição ou reenvio deve manter rastreabilidade.

---

## 3. Usuário de Empresa Fornecedora de Mão de Obra

Esse perfil possui o maior número de funcionalidades entre os usuários externos, pois precisa administrar tanto a documentação da empresa quanto a documentação e situação dos funcionários vinculados.

### Casos de Uso

| ID | Caso de Uso |
|---|---|
| UC-MO-01 | Autenticar-se no sistema |
| UC-MO-02 | Consultar dados da própria empresa |
| UC-MO-03 | Atualizar dados cadastrais da empresa |
| UC-MO-04 | Enviar documentos da empresa |
| UC-MO-05 | Consultar situação dos documentos da empresa |
| UC-MO-06 | Reenviar documento empresarial rejeitado |
| UC-MO-07 | Cadastrar funcionário |
| UC-MO-08 | Editar dados do funcionário |
| UC-MO-09 | Consultar funcionários cadastrados |
| UC-MO-10 | Enviar documentos do funcionário |
| UC-MO-11 | Consultar situação documental do funcionário |
| UC-MO-12 | Reenviar documento do funcionário rejeitado |
| UC-MO-13 | Vincular funcionário a uma obra |
| UC-MO-14 | Consultar vínculos com obras |
| UC-MO-15 | Consultar pendências que impedem atuação na obra |
| UC-MO-16 | Registrar data de pagamento do funcionário |
| UC-MO-17 | Consultar prazo limite do comprovante |
| UC-MO-18 | Enviar comprovante de pagamento |
| UC-MO-19 | Reenviar comprovante rejeitado |
| UC-MO-20 | Consultar situação do comprovante |
| UC-MO-21 | Consultar pagamentos e comprovantes em atraso |

### Fluxo funcional

```text
Empresa de Mão de Obra
        │
        ├── Gerenciar empresa
        │      └── Enviar documentos empresariais
        │
        ├── Gerenciar funcionários
        │      ├── Cadastrar funcionário
        │      ├── Enviar documentos
        │      └── Vincular à obra
        │
        └── Gerenciar pagamentos
               ├── Registrar pagamento
               ├── Consultar prazo
               └── Enviar comprovante
```

### Regra de liberação para obra

```text
Funcionário
    ↓
Documentação completa e aprovada?
    ↓
    Não ──→ Não pode ser liberado para obra
    ↓ Sim
Pode ser autorizado
```

### Regras principais

- Apenas empresas fornecedoras de mão de obra cadastram trabalhadores.
- Cada trabalhador deve permanecer vinculado à empresa responsável.
- A documentação obrigatória do funcionário deve estar aprovada antes de sua autorização para atuar em obra.
- O sistema deve bloquear a liberação quando existirem pendências documentais.
- Comprovantes de pagamento devem ser vinculados a funcionário e período.
- O comprovante deve ser enviado em até **3 dias corridos** após a data de pagamento registrada.
- O sistema deve identificar comprovantes enviados com atraso e comprovantes pendentes em atraso.

---

## 4. Usuário de Empresa Fornecedora de Materiais

Esse perfil possui um escopo mais restrito. O foco é a documentação cadastral e empresarial da fornecedora.

### Casos de Uso

| ID | Caso de Uso |
|---|---|
| UC-MAT-01 | Autenticar-se no sistema |
| UC-MAT-02 | Consultar dados da própria empresa |
| UC-MAT-03 | Atualizar dados cadastrais da empresa |
| UC-MAT-04 | Enviar documentos empresariais |
| UC-MAT-05 | Consultar situação dos documentos |
| UC-MAT-06 | Visualizar motivo de rejeição |
| UC-MAT-07 | Reenviar documento rejeitado |
| UC-MAT-08 | Consultar pendências documentais |

### Funcionalidades que não se aplicam

O usuário de fornecedor de materiais não deve possuir funcionalidades relacionadas a:

```text
✗ Cadastro de funcionários
✗ Documentos de funcionários
✗ Vinculação de funcionário à obra
✗ Registro de salário ou pagamento
✗ Envio de comprovante salarial
✗ Controle do prazo de 3 dias
```

Essas funcionalidades devem ficar indisponíveis para esse perfil, pois fornecedores de materiais são isentos de cadastro de funcionários, folha e comprovantes salariais.

---

## 5. Estrutura de Acesso do Sistema

```text
                    LOGIN
                      │
             Identifica usuário
                      │
       ┌──────────────┼───────────────┐
       ↓              ↓               ↓
   JOTANUNES      MÃO DE OBRA     MATERIAIS
       │              │               │
 Dashboard        Empresa          Empresa
 Empresas         Funcionários     Documentos
 Obras            Obras            Pendências
 Validação        Documentos
 Pendências       Pagamentos
 Auditoria        Pendências
```

O sistema pode utilizar um único frontend React, alterando menus, páginas e permissões conforme o perfil e o tipo da empresa.

---

## 6. Modelo de Perfis e Tipo de Fornecedor

Não é recomendado tratar **Mão de Obra** e **Materiais** como perfis de usuário, pois esses valores representam o tipo da empresa.

Uma modelagem mais adequada seria:

```csharp
public enum UserRole
{
    JotanunesAdministrador,
    JotanunesAnalista,
    Terceirizado
}
```

E para a empresa terceirizada:

```csharp
public enum SupplierType
{
    MaoDeObra,
    Materiais
}
```

Estrutura conceitual:

```text
Quem é o usuário?
    │
    ├── Jotanunes
    │     ├── Administrador
    │     └── Analista
    │
    └── Terceirizado
          │
          └── Empresa
                ├── Mão de Obra
                └── Materiais
```

Dessa forma:

- **Role** define as permissões do usuário.
- **SupplierType** define as regras de negócio aplicáveis à empresa.
- Um usuário terceirizado herda as funcionalidades disponíveis de acordo com o tipo da empresa à qual está vinculado.

---

## 7. Resumo de Responsabilidades

| Funcionalidade | Jotanunes | Mão de Obra | Materiais |
|---|---:|---:|---:|
| Gerenciar empresas | ✓ | Própria empresa | Própria empresa |
| Gerenciar obras | ✓ | Consulta/vínculo | Não |
| Cadastrar funcionários | Consulta | ✓ | Não |
| Enviar documentos empresariais | Valida | ✓ | ✓ |
| Enviar documentos de funcionários | Valida | ✓ | Não |
| Aprovar/rejeitar documentos | ✓ | Não | Não |
| Vincular funcionário à obra | Consulta/valida | ✓ | Não |
| Registrar pagamentos | Consulta/valida | ✓ | Não |
| Enviar comprovantes | Valida | ✓ | Não |
| Consultar pendências | ✓ | ✓ | ✓ |
| Consultar auditoria | ✓ | Limitado às próprias informações, se previsto | Limitado às próprias informações, se previsto |

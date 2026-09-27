using System.Globalization;
using System.Text.Json;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Documentos.Validadores;

public sealed class AsoValidador : IValidadorAdmissional
{
    public ValidacaoAdmissionalResultado Validar(string? camposJson, ValidacaoAdmissionalContexto contexto)
    {
        if (string.IsNullOrWhiteSpace(camposJson))
        {
            return ValidacaoAdmissionalResultado.Falha("camposJson", "Informe os metadados do ASO.");
        }

        try
        {
            using var document = JsonDocument.Parse(camposJson);
            var root = document.RootElement;
            var erros = new List<ValidacaoAdmissionalCampoErro>();

            ValidarTexto(root, "trabalhador", contexto.Funcionario.Nome, erros, "trabalhador");
            ValidarTexto(root, "empresa", contexto.Empresa.RazaoSocial, erros, "empresa");
            if (!root.TryGetProperty("funcao", out var funcao) || funcao.ValueKind != JsonValueKind.String
                || !ValidacaoAdmissionalHelpers.FuncaoCompativel(contexto.Mobilizacao.Funcao, funcao.GetString()!))
            {
                erros.Add(new("funcao", "Função incompatível com a mobilização."));
            }

            if (!root.TryGetProperty("riscos", out var riscos) || riscos.ValueKind != JsonValueKind.Array || riscos.GetArrayLength() == 0)
            {
                erros.Add(new("riscos", "Informe ao menos um risco."));
            }

            if (!root.TryGetProperty("atividadesEspecificas", out var atividades) || atividades.ValueKind != JsonValueKind.Array || atividades.GetArrayLength() == 0)
            {
                erros.Add(new("atividadesEspecificas", "Informe ao menos uma atividade específica."));
            }

            var dataExame = default(DateOnly);
            if (!root.TryGetProperty("dataExame", out var dataExameProp) || dataExameProp.ValueKind != JsonValueKind.String
                || !DateOnly.TryParse(dataExameProp.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out dataExame))
            {
                erros.Add(new("dataExame", "Data do exame inválida."));
            }

            if (!root.TryGetProperty("medico", out var medico) || medico.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(medico.GetString()))
            {
                erros.Add(new("medico", "Nome do médico é obrigatório."));
            }

            if (!root.TryGetProperty("crm", out var crm) || crm.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(crm.GetString()))
            {
                erros.Add(new("crm", "CRM é obrigatório."));
            }

            string? conclusao = null;
            if (!root.TryGetProperty("conclusao", out var conclusaoProp) || conclusaoProp.ValueKind != JsonValueKind.String)
            {
                erros.Add(new("conclusao", "Conclusão é obrigatória."));
            }
            else
            {
                conclusao = conclusaoProp.GetString();
            }
            if (conclusao is "Inapto")
            {
                erros.Add(new("conclusao", "Trabalhador inapto não pode ser aprovado."));
            }
            else if (conclusao == "AptoComRestricao")
            {
                if (!root.TryGetProperty("restricao", out var restricao) || restricao.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(restricao.GetString()))
                {
                    erros.Add(new("restricao", "Restrição é obrigatória para apto com restrição."));
                }

                if (!root.TryGetProperty("restricaoCompativel", out var compativel) || compativel.ValueKind != JsonValueKind.True)
                {
                    erros.Add(new("restricaoCompativel", "Restrição deve ser compatível com a função."));
                }
            }

            if (!root.TryGetProperty("validacaoAssinatura", out var assinatura) || assinatura.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(assinatura.GetString()))
            {
                erros.Add(new("validacaoAssinatura", "Validação de assinatura é obrigatória."));
            }

            if (erros.Count > 0)
            {
                return ValidacaoAdmissionalResultado.Falha(erros);
            }

            DateTime? validoAte = null;
            if (root.TryGetProperty("proximoExame", out var proximo) && proximo.ValueKind == JsonValueKind.String
                && DateOnly.TryParse(proximo.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var proximoData))
            {
                validoAte = proximoData.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            }
            else if (contexto.ParametrosNormativos.TryGetValue(ParametroNormativoChaves.AsoPeriodicidadeDias, out var diasStr)
                && ValidacaoAdmissionalHelpers.TryParsePositiveInt(diasStr, out var dias))
            {
                validoAte = dataExame.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(dias);
            }

            var canonico = AdmissionalJsonCanonicalizer.Canonicalize(root);
            return ValidacaoAdmissionalResultado.Sucesso(canonico, validoAte, SituacaoItemChecklist.Aprovado);
        }
        catch (JsonException)
        {
            return ValidacaoAdmissionalResultado.Falha("camposJson", "JSON inválido.");
        }
    }

    private static void ValidarTexto(JsonElement root, string campo, string esperado, List<ValidacaoAdmissionalCampoErro> erros, string label)
    {
        if (!root.TryGetProperty(campo, out var valor) || valor.ValueKind != JsonValueKind.String
            || !ValidacaoAdmissionalHelpers.NomesEquivalentes(valor.GetString(), esperado))
        {
            erros.Add(new(campo, $"{label} não corresponde à mobilização."));
        }
    }
}

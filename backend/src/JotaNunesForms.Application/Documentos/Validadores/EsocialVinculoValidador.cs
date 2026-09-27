using System.Globalization;
using System.Text.Json;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Documentos.Validadores;

public sealed class EsocialVinculoValidador : IValidadorAdmissional
{
    public ValidacaoAdmissionalResultado Validar(string? camposJson, ValidacaoAdmissionalContexto contexto)
    {
        if (string.IsNullOrWhiteSpace(camposJson))
        {
            return ValidacaoAdmissionalResultado.Falha("camposJson", "Informe os metadados do vínculo.");
        }

        try
        {
            using var document = JsonDocument.Parse(camposJson);
            var root = document.RootElement;
            var erros = new List<ValidacaoAdmissionalCampoErro>();

            string? evento = null;
            if (!root.TryGetProperty("evento", out var eventoProp) || eventoProp.ValueKind != JsonValueKind.String)
            {
                erros.Add(new("evento", "Evento eSocial é obrigatório."));
            }
            else
            {
                evento = eventoProp.GetString();
            }
            if (evento is not ("S2200" or "S2190"))
            {
                erros.Add(new("evento", "Evento deve ser S2200 ou S2190."));
            }

            if (!root.TryGetProperty("cnpjEmpregador", out var cnpjProp) || cnpjProp.ValueKind != JsonValueKind.String)
            {
                erros.Add(new("cnpjEmpregador", "CNPJ do empregador é obrigatório."));
            }
            else
            {
                try
                {
                    if (Empresa.NormalizeCnpj(cnpjProp.GetString()!) != contexto.Empresa.Cnpj)
                    {
                        erros.Add(new("cnpjEmpregador", "CNPJ não corresponde à empresa da mobilização."));
                    }
                }
                catch (ArgumentException)
                {
                    erros.Add(new("cnpjEmpregador", "CNPJ inválido."));
                }
            }

            if (!root.TryGetProperty("cpfTrabalhador", out var cpfProp) || cpfProp.ValueKind != JsonValueKind.String)
            {
                erros.Add(new("cpfTrabalhador", "CPF do trabalhador é obrigatório."));
            }
            else
            {
                try
                {
                    if (Funcionario.NormalizeCpf(cpfProp.GetString()!) != contexto.Funcionario.Cpf)
                    {
                        erros.Add(new("cpfTrabalhador", "CPF não corresponde ao trabalhador."));
                    }
                }
                catch (ArgumentException)
                {
                    erros.Add(new("cpfTrabalhador", "CPF inválido."));
                }
            }

            if (!root.TryGetProperty("funcaoCategoria", out var funcaoProp) || funcaoProp.ValueKind != JsonValueKind.String
                || !ValidacaoAdmissionalHelpers.FuncaoCompativel(contexto.Mobilizacao.Funcao, funcaoProp.GetString()!))
            {
                erros.Add(new("funcaoCategoria", "Função/categoria incompatível com a mobilização."));
            }

            DateOnly? dataAdmissao = null;
            if (!root.TryGetProperty("dataAdmissao", out var dataProp) || dataProp.ValueKind != JsonValueKind.String
                || !DateOnly.TryParse(dataProp.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedData))
            {
                erros.Add(new("dataAdmissao", "Data de admissão inválida."));
            }
            else
            {
                dataAdmissao = parsedData;
                var referencia = DateOnly.FromDateTime(contexto.Mobilizacao.CriadoEm.ToUniversalTime());
                if (parsedData > referencia)
                {
                    erros.Add(new("dataAdmissao", "Data de admissão não pode ser posterior à mobilização."));
                }
            }

            if (!root.TryGetProperty("situacaoVinculo", out var situacaoProp) || situacaoProp.ValueKind != JsonValueKind.String
                || !ValidacaoAdmissionalHelpers.SituacaoVinculoAtiva(situacaoProp.GetString()))
            {
                erros.Add(new("situacaoVinculo", "Situação do vínculo deve indicar vínculo ativo."));
            }

            if (erros.Count > 0)
            {
                return ValidacaoAdmissionalResultado.Falha(erros);
            }

            DateTime? validoAte = null;
            SituacaoItemChecklist? situacao = SituacaoItemChecklist.Aprovado;
            if (evento == "S2190" && dataAdmissao is not null)
            {
                situacao = SituacaoItemChecklist.Preliminar;
                if (contexto.ParametrosNormativos.TryGetValue(ParametroNormativoChaves.S2190PrazoSubstituicaoDias, out var diasStr)
                    && ValidacaoAdmissionalHelpers.TryParsePositiveInt(diasStr, out var dias))
                {
                    validoAte = dataAdmissao.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(dias);
                }
            }

            var canonico = AdmissionalJsonCanonicalizer.Canonicalize(root);
            return ValidacaoAdmissionalResultado.Sucesso(canonico, validoAte, situacao);
        }
        catch (JsonException)
        {
            return ValidacaoAdmissionalResultado.Falha("camposJson", "JSON inválido.");
        }
    }
}

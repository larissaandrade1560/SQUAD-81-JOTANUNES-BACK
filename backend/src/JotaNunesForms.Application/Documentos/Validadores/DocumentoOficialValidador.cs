using System.Text.Json;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Documentos.Validadores;

public sealed class DocumentoOficialValidador : IValidadorAdmissional
{
    private static readonly HashSet<string> TiposPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "RG", "CIN", "CNH",
    };

    public ValidacaoAdmissionalResultado Validar(string? camposJson, ValidacaoAdmissionalContexto contexto)
    {
        if (string.IsNullOrWhiteSpace(camposJson))
        {
            return ValidacaoAdmissionalResultado.Falha("camposJson", "Informe os metadados do documento.");
        }

        try
        {
            using var document = JsonDocument.Parse(camposJson);
            var root = document.RootElement;
            var erros = new List<ValidacaoAdmissionalCampoErro>();

            if (!root.TryGetProperty("tipoDocumento", out var tipo) || tipo.ValueKind != JsonValueKind.String
                || !TiposPermitidos.Contains(tipo.GetString() ?? string.Empty))
            {
                erros.Add(new("tipoDocumento", "Informe RG, CIN ou CNH."));
            }

            if (!root.TryGetProperty("legivel", out var legivel) || legivel.ValueKind != JsonValueKind.True)
            {
                erros.Add(new("legivel", "O documento deve estar legível."));
            }

            if (!root.TryGetProperty("nome", out var nomeProp) || nomeProp.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(nomeProp.GetString()))
            {
                erros.Add(new("nome", "Nome é obrigatório."));
            }
            else if (!ValidacaoAdmissionalHelpers.NomesEquivalentes(nomeProp.GetString(), contexto.Funcionario.Nome))
            {
                erros.Add(new("nome", "Nome não corresponde ao trabalhador da mobilização."));
            }

            if (!root.TryGetProperty("cpf", out var cpfProp) || cpfProp.ValueKind != JsonValueKind.String)
            {
                erros.Add(new("cpf", "CPF é obrigatório."));
            }
            else
            {
                try
                {
                    var cpf = Funcionario.NormalizeCpf(cpfProp.GetString()!);
                    if (cpf != contexto.Funcionario.Cpf)
                    {
                        erros.Add(new("cpf", "CPF não corresponde ao trabalhador da mobilização."));
                    }
                }
                catch (ArgumentException)
                {
                    erros.Add(new("cpf", "CPF inválido."));
                }
            }

            if (erros.Count > 0)
            {
                return ValidacaoAdmissionalResultado.Falha(erros);
            }

            var canonico = AdmissionalJsonCanonicalizer.Canonicalize(root);
            return ValidacaoAdmissionalResultado.Sucesso(canonico);
        }
        catch (JsonException)
        {
            return ValidacaoAdmissionalResultado.Falha("camposJson", "JSON inválido.");
        }
    }
}

using System.Globalization;
using System.Text.Json;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Documentos.Validadores;

public sealed class OrdemServicoValidador : IValidadorAdmissional
{
    public ValidacaoAdmissionalResultado Validar(string? camposJson, ValidacaoAdmissionalContexto contexto)
    {
        if (string.IsNullOrWhiteSpace(camposJson))
        {
            return ValidacaoAdmissionalResultado.Falha("camposJson", "Informe os metadados da ordem de serviço.");
        }

        try
        {
            using var document = JsonDocument.Parse(camposJson);
            var root = document.RootElement;
            var erros = new List<ValidacaoAdmissionalCampoErro>();

            foreach (var campo in new[]
                     {
                         "funcaoAtividades", "riscos", "medidasPreventivas", "proibicoes", "emergencia", "usoEpi",
                     })
            {
                if (!root.TryGetProperty(campo, out var valor) || valor.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(valor.GetString()))
                {
                    erros.Add(new(campo, "Campo obrigatório."));
                }
            }

            if (!root.TryGetProperty("data", out var dataProp) || dataProp.ValueKind != JsonValueKind.String
                || !DateOnly.TryParse(dataProp.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                erros.Add(new("data", "Data inválida."));
            }

            if (!root.TryGetProperty("cienciaTrabalhador", out var ciencia) || ciencia.ValueKind != JsonValueKind.True)
            {
                erros.Add(new("cienciaTrabalhador", "Ciência do trabalhador é obrigatória."));
            }

            if (erros.Count > 0)
            {
                return ValidacaoAdmissionalResultado.Falha(erros);
            }

            var canonico = AdmissionalJsonCanonicalizer.Canonicalize(root);
            return ValidacaoAdmissionalResultado.Sucesso(canonico, situacao: SituacaoItemChecklist.Aprovado);
        }
        catch (JsonException)
        {
            return ValidacaoAdmissionalResultado.Falha("camposJson", "JSON inválido.");
        }
    }
}

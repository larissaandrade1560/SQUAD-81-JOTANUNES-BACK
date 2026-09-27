using System.Globalization;
using System.Text.Json;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Documentos.Validadores;

public sealed class Nr18Validador : IValidadorAdmissional
{
    public ValidacaoAdmissionalResultado Validar(string? camposJson, ValidacaoAdmissionalContexto contexto)
    {
        if (string.IsNullOrWhiteSpace(camposJson))
        {
            return ValidacaoAdmissionalResultado.Falha("camposJson", "Informe os metadados do treinamento NR-18.");
        }

        try
        {
            using var document = JsonDocument.Parse(camposJson);
            var root = document.RootElement;
            var erros = new List<ValidacaoAdmissionalCampoErro>();

            if (!root.TryGetProperty("trabalhador", out var trabalhador) || trabalhador.ValueKind != JsonValueKind.String
                || !ValidacaoAdmissionalHelpers.NomesEquivalentes(trabalhador.GetString(), contexto.Funcionario.Nome))
            {
                erros.Add(new("trabalhador", "Trabalhador não corresponde à mobilização."));
            }

            if (!root.TryGetProperty("treinamentoInicial", out var inicial) || inicial.ValueKind != JsonValueKind.True)
            {
                erros.Add(new("treinamentoInicial", "Treinamento inicial deve ser confirmado."));
            }

            foreach (var campo in new[] { "conteudo", "local", "instrutor", "responsavelTecnico", "assinatura" })
            {
                if (!root.TryGetProperty(campo, out var valor) || valor.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(valor.GetString()))
                {
                    erros.Add(new(campo, "Campo obrigatório."));
                }
            }

            if (!root.TryGetProperty("cargaHorariaHoras", out var carga) || !carga.TryGetDecimal(out var horas) || horas <= 0)
            {
                erros.Add(new("cargaHorariaHoras", "Carga horária inválida."));
            }
            else if (contexto.ParametrosNormativos.TryGetValue(ParametroNormativoChaves.Nr18CargaHorariaInicial, out var minimoStr)
                && decimal.TryParse(minimoStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var minimo)
                && horas < minimo)
            {
                erros.Add(new("cargaHorariaHoras", "Carga horária abaixo do mínimo normativo."));
            }

            var dataTreinamento = default(DateOnly);
            if (!root.TryGetProperty("data", out var dataProp) || dataProp.ValueKind != JsonValueKind.String
                || !DateOnly.TryParse(dataProp.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out dataTreinamento))
            {
                erros.Add(new("data", "Data do treinamento inválida."));
            }

            if (erros.Count > 0)
            {
                return ValidacaoAdmissionalResultado.Falha(erros);
            }

            DateTime? validoAte = null;
            if (contexto.ParametrosNormativos.TryGetValue(ParametroNormativoChaves.Nr18PeriodicidadeMeses, out var mesesStr)
                && ValidacaoAdmissionalHelpers.TryParsePositiveInt(mesesStr, out var meses))
            {
                validoAte = dataTreinamento.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddMonths(meses);
            }

            var canonico = AdmissionalJsonCanonicalizer.Canonicalize(root);
            return ValidacaoAdmissionalResultado.Sucesso(canonico, validoAte, SituacaoItemChecklist.Aprovado);
        }
        catch (JsonException)
        {
            return ValidacaoAdmissionalResultado.Falha("camposJson", "JSON inválido.");
        }
    }
}

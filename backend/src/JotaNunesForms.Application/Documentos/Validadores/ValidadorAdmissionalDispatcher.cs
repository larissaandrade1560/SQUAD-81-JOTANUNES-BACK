using System.Text.Json;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Documentos.Validadores;

public sealed class ValidadorAdmissionalDispatcher
{
    private static readonly HashSet<string> CodigosAdmissionais = new(StringComparer.Ordinal)
    {
        CatalogoRequisitoCodigos.DocOficialFoto,
        CatalogoRequisitoCodigos.EsocialVinculo,
        CatalogoRequisitoCodigos.AsoAdmissional,
        CatalogoRequisitoCodigos.Nr18Basica,
        CatalogoRequisitoCodigos.OrdemServico,
    };

    private readonly Dictionary<string, IValidadorAdmissional> _validadores;
    private readonly ICatalogoRequisitoRepository _catalogo;

    public ValidadorAdmissionalDispatcher(ICatalogoRequisitoRepository catalogo)
    {
        _catalogo = catalogo;
        _validadores = new Dictionary<string, IValidadorAdmissional>(StringComparer.Ordinal)
        {
            [CatalogoRequisitoCodigos.DocOficialFoto] = new DocumentoOficialValidador(),
            [CatalogoRequisitoCodigos.EsocialVinculo] = new EsocialVinculoValidador(),
            [CatalogoRequisitoCodigos.AsoAdmissional] = new AsoValidador(),
            [CatalogoRequisitoCodigos.Nr18Basica] = new Nr18Validador(),
            [CatalogoRequisitoCodigos.OrdemServico] = new OrdemServicoValidador(),
        };
    }

    public static bool EhCodigoAdmissional(string codigo) => CodigosAdmissionais.Contains(codigo);

    public async Task<ValidacaoAdmissionalResultado> ValidarAsync(
        string requisitoCodigo,
        string? camposJson,
        Mobilizacao mobilizacao,
        Funcionario funcionario,
        Empresa empresa,
        CancellationToken cancellationToken = default)
    {
        if (!_validadores.TryGetValue(requisitoCodigo, out var validador))
        {
            return ValidacaoAdmissionalResultado.Falha("requisito", "Validador não configurado para o requisito.");
        }

        var parametros = await _catalogo.ListParametrosAsync(cancellationToken);
        var mapa = parametros.ToDictionary(p => p.Chave, p => p.Valor, StringComparer.Ordinal);
        var contexto = new ValidacaoAdmissionalContexto(
            requisitoCodigo,
            mobilizacao,
            funcionario,
            empresa,
            mapa,
            DateTime.UtcNow);

        return validador.Validar(camposJson, contexto);
    }
}

internal static class AdmissionalJsonCanonicalizer
{
    public static string Canonicalize(JsonElement root)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
        {
            WriteSorted(writer, root);
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteSorted(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteSorted(writer, property.Value);
                }

                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    WriteSorted(writer, item);
                }

                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }
}

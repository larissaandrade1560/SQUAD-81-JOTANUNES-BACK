using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Documentos.Validadores;

public sealed record ValidacaoAdmissionalCampoErro(string Campo, string Mensagem);

public sealed record ValidacaoAdmissionalResultado(
    bool Valido,
    string? CamposJsonCanonico,
    DateTime? ValidoAte,
    SituacaoItemChecklist? SituacaoDerivada,
    IReadOnlyList<ValidacaoAdmissionalCampoErro> Erros)
{
    public static ValidacaoAdmissionalResultado Sucesso(
        string camposJsonCanonico,
        DateTime? validoAte = null,
        SituacaoItemChecklist? situacao = null) =>
        new(true, camposJsonCanonico, validoAte, situacao, Array.Empty<ValidacaoAdmissionalCampoErro>());

    public static ValidacaoAdmissionalResultado Falha(IReadOnlyList<ValidacaoAdmissionalCampoErro> erros) =>
        new(false, null, null, null, erros);

    public static ValidacaoAdmissionalResultado Falha(string campo, string mensagem) =>
        Falha([new ValidacaoAdmissionalCampoErro(campo, mensagem)]);
}

public sealed record ValidacaoAdmissionalContexto(
    string RequisitoCodigo,
    Mobilizacao Mobilizacao,
    Funcionario Funcionario,
    Empresa Empresa,
    IReadOnlyDictionary<string, string> ParametrosNormativos,
    DateTime ReferenciaUtc);

public static class ValidacaoAdmissionalHelpers
{
    public static bool NomesEquivalentes(string? a, string? b)
    {
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
        {
            return false;
        }

        return string.Equals(NormalizeNome(a), NormalizeNome(b), StringComparison.Ordinal);
    }

    public static string NormalizeNome(string nome) =>
        string.Join(' ', nome.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();

    public static bool FuncaoCompativel(string funcaoMobilizacao, string funcaoInformada)
    {
        if (string.IsNullOrWhiteSpace(funcaoMobilizacao) || string.IsNullOrWhiteSpace(funcaoInformada))
        {
            return false;
        }

        var mob = NormalizeNome(funcaoMobilizacao);
        var inf = NormalizeNome(funcaoInformada);
        return mob == inf || mob.Contains(inf, StringComparison.Ordinal) || inf.Contains(mob, StringComparison.Ordinal);
    }

    public static bool TryParsePositiveInt(string? valor, out int numero) =>
        int.TryParse(valor, out numero) && numero > 0;

    public static bool SituacaoVinculoAtiva(string? situacao) =>
        !string.IsNullOrWhiteSpace(situacao)
        && !situacao.Contains("inativ", StringComparison.OrdinalIgnoreCase)
        && !situacao.Contains("deslig", StringComparison.OrdinalIgnoreCase);
}

public interface IValidadorAdmissional
{
    ValidacaoAdmissionalResultado Validar(string? camposJson, ValidacaoAdmissionalContexto contexto);
}

using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record CreateMobilizacaoRequest(
    Guid ContratoId,
    Guid ObraId,
    string Nome,
    string Cpf,
    string Funcao,
    Guid? ProcessoId = null,
    Guid? EmpresaId = null,
    DateOnly? DataFimObra = null,
    string? TurnoJornada = null);

public sealed record UpdateMobilizacaoRequest(
    string Funcao,
    DateOnly? DataFimObra,
    string? TurnoJornada);

public sealed record HistoricoLotacaoResponse(
    Guid Id,
    Guid ObraId,
    DateTime Inicio,
    DateTime? Fim,
    string? Motivo)
{
    public static HistoricoLotacaoResponse FromEntity(HistoricoLotacao lotacao) =>
        new(lotacao.Id, lotacao.ObraId, lotacao.Inicio, lotacao.Fim, lotacao.Motivo);
}

public sealed record ImpedimentoLiberacaoResponse(
    string Codigo,
    string RequisitoCodigo,
    string Motivo,
    Guid? ItemChecklistId);

public sealed record ResultadoLiberacaoResponse(
    Guid MobilizacaoId,
    SituacaoMobilizacao Situacao,
    bool Liberado,
    DateTime AvaliadoEm,
    IReadOnlyList<ImpedimentoLiberacaoResponse> Impedimentos);

public sealed record RegistrarMovimentoEpiRequest(
    TipoMovimentoEpi Tipo,
    Guid? MovimentoOrigemId,
    string Epi,
    int Quantidade,
    string NumeroCa,
    DateOnly Data,
    bool OrientacaoUso,
    bool ResponsabilidadeGuarda,
    bool AceiteTrabalhador);

public sealed record MovimentoEpiResponse(
    Guid Id,
    Guid MobilizacaoId,
    TipoMovimentoEpi Tipo,
    Guid? MovimentoOrigemId,
    string Epi,
    int Quantidade,
    string NumeroCa,
    DateOnly Data,
    bool OrientacaoUso,
    bool ResponsabilidadeGuarda,
    bool AceiteTrabalhador,
    DateTime RegistradoEm);

public sealed record MovimentoEpiResultResponse(
    MovimentoEpiResponse Movimento,
    int SaldoAtivo,
    ResultadoLiberacaoResponse Liberacao);

public sealed record RegistrarIntegracaoRequest(
    DateTimeOffset DataHora,
    string Conteudo,
    string Instrutor,
    string? Avaliacao,
    bool AceiteTrabalhador,
    DateTime? ValidoAte);

public sealed record IntegracaoObraResponse(
    Guid Id,
    Guid MobilizacaoId,
    Guid ObraId,
    DateTimeOffset DataHora,
    string Conteudo,
    string Instrutor,
    string? Avaliacao,
    bool AceiteTrabalhador,
    DateTime? ValidoAte,
    bool Refazer,
    string? MotivoRefazer,
    DateTime CriadoEm);

public sealed record IntegracaoResultResponse(
    IntegracaoObraResponse Integracao,
    ResultadoLiberacaoResponse Liberacao);

public sealed record MarcarIntegracaoRefazerRequest(string Motivo);

public sealed record MobilizacaoResponse(
    Guid Id,
    Guid FuncionarioId,
    string FuncionarioNome,
    string Cpf,
    Guid EmpresaId,
    Guid ContratoId,
    Guid ObraId,
    Guid ProcessoId,
    string Funcao,
    DateOnly? DataFimObra,
    string? TurnoJornada,
    SituacaoMobilizacao Situacao,
    DateTime CriadoEm,
    IReadOnlyList<HistoricoLotacaoResponse> Lotacoes,
    IReadOnlyList<ItemChecklistResponse> ChecklistAdmissional);

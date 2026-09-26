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
